using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaServers.Service;
using static Milimoe.FunGame.Core.Library.Constant.General;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Grid = Milimoe.FunGame.Core.Library.Common.Addon.Grid;
using Panel = System.Windows.Controls.Panel;
using Rectangle = System.Windows.Shapes.Rectangle;
using UserControl = System.Windows.Controls.UserControl;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    // ... (CharacterQueueItem, FirstCharConverter, CharacterToStringWithLevelConverter 保持不变) ...

    public class CharacterQueueItem(Character character, double atDelay)
    {
        public Character Character { get; set; } = character;
        public double ATDelay { get; set; } = atDelay;
    }

    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && s.Length > 0)
            {
                return s[0].ToString().ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CharacterToStringWithLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Character character)
            {
                return character.ToString();
            }
            return "[未知角色]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// GameMapViewer.xaml 的交互逻辑
    /// </summary>
    public partial class GameMapViewer : UserControl
    {
        private readonly GameMapController _controller; // 控制器引用

        // 存储UI元素（Rectangle）与Grid对象的关联，通过Grid的ID进行查找
        private readonly Dictionary<long, Rectangle> _gridIdToUiElement = [];
        // 存储UI元素（Rectangle）到Grid对象的反向关联，用于点击事件
        private readonly Dictionary<Rectangle, Grid> _uiElementToGrid = [];

        // 新增：存储UI元素（Border）与Character对象的关联
        private readonly Dictionary<Character, Border> _characterToUiElement = [];
        // 新增：存储UI元素（Border）到Character对象的反向关联
        private readonly Dictionary<Border, Character> _uiElementToCharacter = [];

        // 新增：用于左侧动态队列的ObservableCollection
        public ObservableCollection<CharacterQueueItem> CharacterQueueItems { get; set; }

        // 新增：用于目标选择UI的ObservableCollection
        public ObservableCollection<Character> SelectedTargets { get; set; } = [];

        // 回调Action，用于将UI选择结果传递给Controller
        private Action<long>? _resolveCharacterSelection;
        private Action<CharacterActionType>? _resolveActionType;
        private Action<List<Character>>? _resolveTargetSelection;
        private Action<long>? _resolveSkillSelection;
        private Action<long>? _resolveItemSelection;
        private Action<bool>? _resolveContinuePrompt;

        // 目标选择的内部状态
        private Character? _actingCharacterForTargetSelection;
        private List<Character> _potentialTargetsForSelection = [];
        private long _maxTargetsForSelection;
        private bool _canSelectSelf, _canSelectEnemy, _canSelectTeammate;
        private bool _isSelectingTargets = false; // 标记当前是否处于目标选择模式

        public GameMapViewer()
        {
            InitializeComponent();
            CharacterQueueItems = [];
            this.DataContext = this; // 将UserControl自身设置为DataContext，以便ItemsControl可以绑定到CharacterQueueItems和SelectedTargets属性

            // 初始化 SelectedTargetsItemsControl 的 ItemsSource
            SelectedTargetsItemsControl.ItemsSource = SelectedTargets;

            // 初始禁用所有操作按钮
            SetActionButtonsEnabled(false);

            // 控制器将在构造函数中被设置
            _controller = new GameMapController(this);
            TaskUtility.NewTask(async () => await _controller.Start());
        }

        // CurrentGameMap 依赖属性：允许外部传入GameMap实例
        public static readonly DependencyProperty CurrentGameMapProperty =
            DependencyProperty.Register("CurrentGameMap", typeof(GameMap), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnCurrentGameMapChanged));

        public GameMap CurrentGameMap
        {
            get { return (GameMap)GetValue(CurrentGameMapProperty); }
            set { SetValue(CurrentGameMapProperty, value); }
        }

        // 新增 CurrentCharacter 依赖属性：用于显示当前玩家角色的信息
        public static readonly DependencyProperty CurrentCharacterProperty =
            DependencyProperty.Register("CurrentCharacter", typeof(Character), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnCurrentCharacterChanged));

        public Character? CurrentCharacter
        {
            get { return (Character)GetValue(CurrentCharacterProperty); }
            set { SetValue(CurrentCharacterProperty, value); }
        }

        // 新增 PlayerCharacter 依赖属性：用于存储玩家控制的角色
        public static readonly DependencyProperty PlayerCharacterProperty =
            DependencyProperty.Register("PlayerCharacter", typeof(Character), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnPlayerCharacterChanged));

        public Character? PlayerCharacter
        {
            get { return (Character)GetValue(PlayerCharacterProperty); }
            set { SetValue(PlayerCharacterProperty, value); }
        }

        // 当 PlayerCharacter 属性改变时，目前不需要直接更新UI，因为 CurrentCharacter 的更新逻辑会处理
        private static void OnPlayerCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // GameMapViewer viewer = (GameMapViewer)d;
            // 逻辑已移至 Grid_MouseLeftButtonDown 和 GameMapCanvas_MouseLeftButtonDown
        }

        // 新增 CharacterQueueData 依赖属性：用于传入左侧动态队列的数据
        public static readonly DependencyProperty CharacterQueueDataProperty =
            DependencyProperty.Register("CharacterQueueData", typeof(Dictionary<Character, double>), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnCharacterQueueDataChanged));

        public Dictionary<Character, double>? CharacterQueueData
        {
            get { return (Dictionary<Character, double>?)GetValue(CharacterQueueDataProperty); }
            set { SetValue(CharacterQueueDataProperty, value); }
        }

        public static readonly DependencyProperty CharacterStatisticsProperty =
            DependencyProperty.Register("CharacterStatistics", typeof(Dictionary<Character, CharacterStatistics>), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnCharacterStatisticsChanged));

        public Dictionary<Character, CharacterStatistics> CharacterStatistics
        {
            get { return (Dictionary<Character, CharacterStatistics>)GetValue(CharacterStatisticsProperty); }
            set { SetValue(CharacterStatisticsProperty, value); }
        }

        // 新增 MaxRespawnTimes 依赖属性：用于判断是否显示死亡数
        public static readonly DependencyProperty MaxRespawnTimesProperty =
            DependencyProperty.Register("MaxRespawnTimes", typeof(int), typeof(GameMapViewer),
                                        new PropertyMetadata(0, OnMaxRespawnTimesChanged));

        public int MaxRespawnTimes
        {
            get { return (int)GetValue(MaxRespawnTimesProperty); }
            set { SetValue(MaxRespawnTimesProperty, value); }
        }


        public void Invoke(Action action)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => action());
            }
            else action();
        }

        public async Task Invoke(Func<Task> action)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.BeginInvoke(async () => await action());
            }
            else await action();
        }

        // 原有的 ShowInput 方法不再用于游戏逻辑，可以删除或保留用于其他非游戏逻辑的通用输入
        // public string ShowInput(string msg, string title = "")
        // {
        //     string? result = "";
        //     Invoke(() =>
        //     {
        //         result = InputDialog.Show(msg, title, Window.GetWindow(this));
        //     });
        //     return result;
        // }

        // 当CurrentGameMap属性改变时，触发地图重新渲染
        private static void OnCurrentGameMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.RenderMap();
            viewer.UpdateCharacterPositionsOnMap();
        }

        // 当CurrentCharacter属性改变时，更新底部信息面板
        private static void OnCurrentCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.UpdateBottomInfoPanel();
            viewer.UpdateCharacterStatisticsPanel(); // 角色改变时也更新统计面板
            // 角色改变时，清除装备/状态描述
            viewer.DescriptionTextBlock.Text = "点击装备或状态图标查看详情。";
            viewer.ClearEquipSlotHighlights();
            viewer.ClearStatusIconHighlights();
        }

        // 新增：当CharacterQueueData属性改变时，更新左侧队列面板
        private static void OnCharacterQueueDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.UpdateLeftQueuePanel();
        }

        // 新增：当CharacterStatistics属性改变时，更新数据统计面板
        private static void OnCharacterStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.UpdateCharacterStatisticsPanel();
        }

        // 新增：当MaxRespawnTimes属性改变时，更新数据统计面板（因为会影响死亡数显示）
        private static void OnMaxRespawnTimesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.UpdateCharacterStatisticsPanel();
        }

        /// <summary>
        /// 向调试日志文本框添加一条消息。
        /// </summary>
        /// <param name="message">要添加的日志消息。</param>
        public void AppendDebugLog(string message)
        {
            // 检查当前线程是否是UI线程
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(new Action(() => AppendDebugLog(message)));
                return;
            }

            int maxLines = 1000;

            string currentText = DebugLogTextBlock.Text;
            List<string> lines = [.. currentText.Split([Environment.NewLine], StringSplitOptions.None)];

            if (lines.Count == 1 && lines[0] == "调试日志:")
            {
                lines.Clear();
            }

            lines.Add($"{message}");

            while (lines.Count > maxLines)
            {
                lines.RemoveAt(0);
            }

            DebugLogTextBlock.Text = string.Join(Environment.NewLine, lines);
            DebugLogScrollViewer?.ScrollToEnd();
        }

        /// <summary>
        /// 渲染地图：根据CurrentGameMap对象在Canvas上绘制所有格子
        /// </summary>
        private void RenderMap()
        {
            GameMapCanvas.Children.Clear(); // 清除Canvas上所有旧的UI元素 (包括角色图标，后续会重新绘制)
            _gridIdToUiElement.Clear();     // 清除旧的关联
            _uiElementToGrid.Clear();       // 清除旧的关联
            _characterToUiElement.Clear();  // 清除旧的角色UI关联
            _uiElementToCharacter.Clear();  // 清除旧的角色UI反向关联
            GridInfoPanel.Visibility = Visibility.Collapsed; // 地图重绘时隐藏格子信息面板

            if (CurrentGameMap == null)
            {
                AppendDebugLog("地图未加载。");
                return;
            }

            double maxCanvasWidth = 0;
            double maxCanvasHeight = 0;

            foreach (var gridEntry in CurrentGameMap.GridsByCoordinate)
            {
                Grid grid = gridEntry.Value;

                if (grid.Z != 0) continue;

                Rectangle rect = new()
                {
                    Width = (int)CurrentGameMap.Size,
                    Height = (int)CurrentGameMap.Size,
                    Fill = ToWpfBrush(grid.Color),
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                };

                Canvas.SetLeft(rect, grid.X * CurrentGameMap.Size);
                Canvas.SetTop(rect, grid.Y * CurrentGameMap.Size);
                Panel.SetZIndex(rect, 0); // 确保格子在底部

                rect.MouseLeftButtonDown += Grid_MouseLeftButtonDown;

                GameMapCanvas.Children.Add(rect);

                _gridIdToUiElement.Add(grid.Id, rect);
                _uiElementToGrid.Add(rect, grid);

                maxCanvasWidth = Math.Max(maxCanvasWidth, (grid.X + 1) * CurrentGameMap.Size);
                maxCanvasHeight = Math.Max(maxCanvasHeight, (grid.Y + 1) * CurrentGameMap.Size);
            }

            GameMapCanvas.Width = maxCanvasWidth;
            GameMapCanvas.Height = maxCanvasHeight;
        }

        /// <summary>
        /// 在地图上更新所有角色的位置和显示。
        /// </summary>
        public void UpdateCharacterPositionsOnMap()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => UpdateCharacterPositionsOnMap());
                return;
            }

            // 清除Canvas上所有旧的角色UI元素
            foreach (Border border in _characterToUiElement.Values)
            {
                GameMapCanvas.Children.Remove(border);
            }
            _characterToUiElement.Clear();
            _uiElementToCharacter.Clear();

            if (CurrentGameMap?.Characters == null || CurrentGameMap.Characters.Count == 0)
            {
                return;
            }

            double iconSize = 28; // 角色图标大小，已从 40 调整为 28
            // 计算居中偏移量，使图标位于格子中心
            double offset = (CurrentGameMap.Size - iconSize) / 2;

            foreach (var kvp in CurrentGameMap.Characters)
            {
                Character character = kvp.Key;
                Grid grid = kvp.Value;

                if (grid.Z != 0) continue; // 只显示Z=0平面的角色

                Border characterBorder = new()
                {
                    Style = (Style)this.FindResource("CharacterIconStyle"),
                    ToolTip = character.ToStringWithLevel(),
                    IsHitTestVisible = true // 确保角色图标可以被点击
                };

                TextBlock characterText = new()
                {
                    Style = (Style)this.FindResource("CharacterIconTextStyle"),
                    Text = character.NickName.Length > 0 ? character.NickName[0].ToString().ToUpper() : "?"
                };
                characterBorder.Child = characterText;

                // 设置位置
                Canvas.SetLeft(characterBorder, grid.X * CurrentGameMap.Size + offset);
                Canvas.SetTop(characterBorder, grid.Y * CurrentGameMap.Size + offset);
                // 设置ZIndex，确保角色图标在格子之上
                Panel.SetZIndex(characterBorder, 10);

                characterBorder.MouseLeftButtonDown += CharacterIcon_MouseLeftButtonDown;

                GameMapCanvas.Children.Add(characterBorder);
                _characterToUiElement.Add(character, characterBorder);
                _uiElementToCharacter.Add(characterBorder, character);
            }

            // 如果处于目标选择模式，重新应用高亮
            if (_isSelectingTargets)
            {
                UpdateCharacterHighlights();
            }
        }

        /// <summary>
        /// 更新左侧动态队列面板，显示角色及其AT Delay。
        /// </summary>
        public void UpdateLeftQueuePanel()
        {
            // 确保在UI线程上执行
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => UpdateLeftQueuePanel());
                return;
            }

            CharacterQueueItems.Clear(); // 清除现有项

            if (CharacterQueueData != null)
            {
                // 按AT Delay值升序排序
                var sortedQueue = CharacterQueueData.OrderBy(kvp => kvp.Value);

                foreach (var kvp in sortedQueue)
                {
                    if (kvp.Key.HP > 0) // 只显示活着的角色
                    {
                        CharacterQueueItems.Add(new CharacterQueueItem(kvp.Key, kvp.Value));
                    }
                }
            }
        }

        /// <summary>
        /// 更新底部信息面板显示当前角色的详细信息、装备和状态。
        /// </summary>
        public void UpdateBottomInfoPanel()
        {
            // 确保在UI线程上执行
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => UpdateBottomInfoPanel());
                return;
            }

            // 每次更新面板时，清除装备/状态描述和高亮
            DescriptionTextBlock.Text = "点击装备或状态图标查看详情。";
            ClearEquipSlotHighlights();
            ClearStatusIconHighlights();

            Character? character = CurrentCharacter;
            if (character != null)
            {
                CharacterNameTextBlock.Text = $"角色名称: {character.ToStringWithLevel()}";
                CharacterAvatarTextBlock.Text = character.NickName.Length > 0 ? character.NickName[0].ToString().ToUpper() : "?";

                // --- HP条 (包含护盾) ---
                double barWidth = 120; // HP条的固定宽度
                HpFillRectangle.Width = (character.HP / character.MaxHP) * barWidth;
                HpFillRectangle.Fill = Brushes.Green;

                double totalShieldValue = character.Shield.TotalPhysical + character.Shield.TotalMagical + character.Shield.TotalMix;
                if (totalShieldValue > 0)
                {
                    ShieldFillRectangle.Visibility = Visibility.Visible;
                    // 护盾宽度基于总生命值，但要确保不超过剩余的条宽
                    double shieldWidth = (totalShieldValue / character.MaxHP) * barWidth;
                    ShieldFillRectangle.Width = Math.Min(shieldWidth, barWidth); // 护盾可以覆盖整个条

                    // 护盾颜色优先级：混合 > 物理 > 魔法
                    if (character.Shield.TotalMix > 0)
                    {
                        ShieldFillRectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD2, 0xB4, 0x8C)); // Tan (棕黄色)
                    }
                    else if (character.Shield.TotalPhysical > 0)
                    {
                        ShieldFillRectangle.Fill = Brushes.LightGray; // 灰白色
                    }
                    else if (character.Shield.TotalMagical > 0)
                    {
                        ShieldFillRectangle.Fill = Brushes.LightBlue; // 淡蓝色
                    }

                    // 更新HP值文本，包含护盾详情
                    List<string> shieldParts = [];
                    if (character.Shield.TotalPhysical > 0) shieldParts.Add($"P: {character.Shield.TotalPhysical:0.##}");
                    if (character.Shield.TotalMagical > 0) shieldParts.Add($"M: {character.Shield.TotalMagical:0.##}");
                    if (character.Shield.TotalMix > 0) shieldParts.Add($"Mix: {character.Shield.TotalMix:0.##}");

                    string shieldDetails = shieldParts.Count != 0 ? $"（{string.Join("，", shieldParts)}）" : "";
                    HpValueTextBlock.Text = $"{character.HP:0.##}/{character.MaxHP:0.##}{shieldDetails}";
                }
                else
                {
                    ShieldFillRectangle.Visibility = Visibility.Collapsed;
                    HpValueTextBlock.Text = $"{character.HP:0.##}/{character.MaxHP:0.##}";
                }

                // MP条
                MpProgressBar.Value = character.MP;
                MpProgressBar.Maximum = character.MaxMP;
                MpValueTextBlock.Text = $"{character.MP:0.##}/{character.MaxMP:0.##}";

                // EP条
                EpProgressBar.Value = character.EP;
                EpProgressBar.Maximum = GameplayEquilibriumConstant.MaxEP;
                EpValueTextBlock.Text = $"{character.EP:0.##}/{GameplayEquilibriumConstant.MaxEP}";

                // --- 更新装备槽位 ---
                EquipSlot equipSlot = character.EquipSlot;
                UpdateEquipSlot(MagicCardPackSlotText, MagicCardPackBorder, equipSlot.MagicCardPack, "魔");
                UpdateEquipSlot(WeaponSlotText, WeaponBorder, equipSlot.Weapon, "武");
                UpdateEquipSlot(ArmorSlotText, ArmorBorder, equipSlot.Armor, "防");
                UpdateEquipSlot(ShoesSlotText, ShoesBorder, equipSlot.Shoes, "鞋");
                UpdateEquipSlot(Accessory1SlotText, Accessory1Border, equipSlot.Accessory1, "饰1");
                UpdateEquipSlot(Accessory2SlotText, Accessory2Border, equipSlot.Accessory2, "饰2");

                // --- 更新状态条 ---
                CharacterEffectsPanel.Children.Clear(); // 清除所有旧状态图标
                Effect[] effects = [.. character.Effects.Where(e => e.ShowInStatusBar)];
                if (effects.Length != 0)
                {
                    foreach (Effect effect in effects)
                    {
                        Border effectBorder = new()
                        {
                            Style = (Style)this.FindResource("StatusIconStyle"),
                            ToolTip = effect.Description ?? effect.GetType().Name // 鼠标悬停显示完整效果名称或描述
                        };
                        TextBlock effectText = new()
                        {
                            Style = (Style)this.FindResource("StatusIconTextStyle"),
                            Text = effect.GetType().Name.Length > 0 ? effect.GetType().Name[0].ToString().ToUpper() : "?"
                        };
                        effectBorder.Child = effectText;
                        effectBorder.Tag = effect; // 存储Effect对象，以便点击时获取其描述
                        effectBorder.MouseLeftButtonDown += StatusIcon_MouseLeftButtonDown; // 添加点击事件
                        CharacterEffectsPanel.Children.Add(effectBorder);
                    }
                }

                // --- 更新其他角色属性 ---
                bool showGrowth = false; // 假设不显示成长值

                AttackTextBlock.Text = $"攻击力：{character.ATK:0.##}" + (character.ExATK != 0 ? $" [{character.BaseATK:0.##} {(character.ExATK >= 0 ? "+" : "-")} {Math.Abs(character.ExATK):0.##}]" : "");

                double exDEF = character.ExDEF + character.ExDEF2 + character.ExDEF3;
                PhysicalDefTextBlock.Text = $"物理护甲：{character.DEF:0.##}" + (exDEF != 0 ? $" [{character.BaseDEF:0.##} {(exDEF >= 0 ? "+" : "-")} {Math.Abs(exDEF):0.##}]" : "") + $" ({character.PDR * 100:0.##}%)";

                MagicResTextBlock.Text = character.GetMagicResistanceInfo().Trim();

                double exSPD = character.AGI * GameplayEquilibriumConstant.AGItoSPDMultiplier + character.ExSPD;
                SpeedTextBlock.Text = $"行动速度：{character.SPD:0.##}" + (exSPD != 0 ? $" [{character.InitialSPD:0.##} {(exSPD >= 0 ? "+" : "-")} {Math.Abs(exSPD):0.##}]" : "") + $" ({character.ActionCoefficient * 100:0.##}%)";

                PrimaryAttrTextBlock.Text = $"核心属性：{CharacterSet.GetPrimaryAttributeName(character.PrimaryAttribute)}";

                double exSTR = character.ExSTR + character.ExSTR2;
                StrengthTextBlock.Text = $"力量：{character.STR:0.##}" + (exSTR != 0 ? $" [{character.BaseSTR:0.##} {(exSTR >= 0 ? "+" : "-")} {Math.Abs(exSTR):0.##}]" : "") + (showGrowth ? $"（{(character.STRGrowth >= 0 ? "+" : "-")}{Math.Abs(character.STRGrowth)}/Lv）" : "");

                double exAGI = character.ExAGI + character.ExAGI2;
                AgilityTextBlock.Text = $"敏捷：{character.AGI:0.##}" + (exAGI != 0 ? $" [{character.BaseAGI:0.##} {(exAGI >= 0 ? "+" : "-")} {Math.Abs(exAGI):0.##}]" : "") + (showGrowth ? $"（{(character.AGIGrowth >= 0 ? "+" : "-")}{Math.Abs(character.AGIGrowth)}/Lv）" : "");

                double exINT = character.ExINT + character.ExINT2;
                IntellectTextBlock.Text = $"智力：{character.INT:0.##}" + (exINT != 0 ? $" [{character.BaseINT:0.##} {(exINT >= 0 ? "+" : "-")} {Math.Abs(exINT):0.##}]" : "") + (showGrowth ? $"（{(character.INTGrowth >= 0 ? "+" : "-")}{Math.Abs(character.INTGrowth)}/Lv）" : "");

                HpRegenTextBlock.Text = $"生命回复：{character.HR:0.##}" + (character.ExHR != 0 ? $" [{character.InitialHR + character.STR * GameplayEquilibriumConstant.STRtoHRFactor:0.##} {(character.ExHR >= 0 ? "+" : "-")} {Math.Abs(character.ExHR):0.##}]" : "");

                MpRegenTextBlock.Text = $"魔法回复：{character.MR:0.##}" + (character.ExMR != 0 ? $" [{character.InitialMR + character.INT * GameplayEquilibriumConstant.INTtoMRFactor:0.##} {(character.ExMR >= 0 ? "+" : "-")} {Math.Abs(character.ExMR):0.##}]" : "");

                CritRateTextBlock.Text = $"暴击率：{character.CritRate * 100:0.##}%";
                CritDmgTextBlock.Text = $"暴击伤害：{character.CritDMG * 100:0.##}%";
                EvadeRateTextBlock.Text = $"闪避率：{character.EvadeRate * 100:0.##}%";
                LifestealTextBlock.Text = $"生命偷取：{character.Lifesteal * 100:0.##}%";
                CdrTextBlock.Text = $"冷却缩减：{character.CDR * 100:0.##}%";
                AccelCoeffTextBlock.Text = $"加速系数：{character.AccelerationCoefficient * 100:0.##}%";
                PhysPenTextBlock.Text = $"物理穿透：{character.PhysicalPenetration * 100:0.##}%";
                MagicPenTextBlock.Text = $"魔法穿透：{character.MagicalPenetration * 100:0.##}%";
            }
            else
            {
                // 当没有角色被选中时，重置所有显示
                CharacterNameTextBlock.Text = "角色名称: [未选择]";
                CharacterAvatarTextBlock.Text = "?";
                HpFillRectangle.Width = 0;
                ShieldFillRectangle.Visibility = Visibility.Collapsed;
                HpValueTextBlock.Text = "0/0";

                MpProgressBar.Value = 0;
                MpProgressBar.Maximum = 100; // 默认最大值
                MpValueTextBlock.Text = "0/0";
                EpProgressBar.Value = 0;
                EpProgressBar.Maximum = 100;
                EpValueTextBlock.Text = "0/0";

                // 重置装备槽位
                UpdateEquipSlot(MagicCardPackSlotText, MagicCardPackBorder, null, "魔");
                UpdateEquipSlot(WeaponSlotText, WeaponBorder, null, "武");
                UpdateEquipSlot(ArmorSlotText, ArmorBorder, null, "防");
                UpdateEquipSlot(ShoesSlotText, ShoesBorder, null, "鞋");
                UpdateEquipSlot(Accessory1SlotText, Accessory1Border, null, "饰1");
                UpdateEquipSlot(Accessory2SlotText, Accessory2Border, null, "饰2");

                // 清空状态条
                CharacterEffectsPanel.Children.Clear();

                // 清空角色属性
                AttackTextBlock.Text = "攻击力:";
                PhysicalDefTextBlock.Text = "物理护甲:";
                MagicResTextBlock.Text = "魔法抗性:";
                SpeedTextBlock.Text = "行动速度:";
                PrimaryAttrTextBlock.Text = "核心属性:";
                StrengthTextBlock.Text = "力量:";
                AgilityTextBlock.Text = "敏捷:";
                IntellectTextBlock.Text = "智力:";
                HpRegenTextBlock.Text = "生命回复:";
                MpRegenTextBlock.Text = "魔法回复:";
                CritRateTextBlock.Text = "暴击率:";
                CritDmgTextBlock.Text = "暴击伤害:";
                EvadeRateTextBlock.Text = "闪避率:";
                LifestealTextBlock.Text = "生命偷取:";
                CdrTextBlock.Text = "冷却缩减:";
                AccelCoeffTextBlock.Text = "加速系数:";
                PhysPenTextBlock.Text = "物理穿透:";
                MagicPenTextBlock.Text = "魔法穿透:";
            }

            // 更新数据统计面板
            UpdateCharacterStatisticsPanel();
        }

        /// <summary>
        /// 辅助方法：更新单个装备槽位的显示。
        /// </summary>
        /// <param name="textBlock">装备槽位内的TextBlock。</param>
        /// <param name="parentBorder">装备槽位的Border。</param>
        /// <param name="item">装备物品，如果为null则表示空槽。</param>
        /// <param name="defaultText">空槽时显示的默认文本（如"魔"、"武"）。</param>
        private static void UpdateEquipSlot(TextBlock textBlock, Border parentBorder, Item? item, string defaultText)
        {
            if (item != null)
            {
                textBlock.Text = item.Name.Length > 0 ? item.Name[0].ToString().ToUpper() : defaultText;
                parentBorder.Background = Brushes.LightGreen; // 装备槽有物品时显示浅绿色背景
                textBlock.Foreground = Brushes.Black; // 文本颜色变深
                parentBorder.ToolTip = item.Description ?? item.Name; // 显示物品名称或描述作为ToolTip
            }
            else
            {
                textBlock.Text = defaultText;
                parentBorder.Background = Brushes.LightGray; // 装备槽为空时显示浅灰色背景
                textBlock.Foreground = Brushes.DimGray; // 文本颜色变淡
                parentBorder.ToolTip = $"未装备 {defaultText}"; // 空槽的ToolTip
            }
            // 每次更新时重置边框样式，以便点击时可以高亮
            parentBorder.BorderBrush = Brushes.DarkGray;
            parentBorder.BorderThickness = new Thickness(1);
        }

        /// <summary>
        /// 更新数据统计面板显示指定角色的统计信息。
        /// </summary>
        public void UpdateCharacterStatisticsPanel()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => UpdateCharacterStatisticsPanel());
                return;
            }

            // 清空所有统计文本块
            StatsRatingKillsAssistsDeathsTextBlock.Text = "";
            StatsLiveTimeRoundTurnTextBlock.Text = "";
            StatsControlHealShieldTextBlock.Text = "";
            StatsTotalDamageTextBlock.Text = "";
            StatsTotalTakenDamageTextBlock.Text = "";
            StatsTrueDamageTextBlock.Text = "";
            StatsDamagePerSecondTurnTextBlock.Text = "";
            StatsTrueDamageTextBlock.Visibility = Visibility.Collapsed; // 默认隐藏真实伤害行

            // 尝试将传入的 CharacterStatistics 对象转换为 dynamic 类型，以便访问其属性
            Dictionary<Character, CharacterStatistics> dict = CharacterStatistics;
            if (dict != null)
            {
                CharacterStatistics? stats = dict.Where(kv => kv.Key == CurrentCharacter).Select(kv => kv.Value).FirstOrDefault();
                if (stats != null)
                {
                    // 第一行：技术得分 / 击杀数 / 助攻数 / 死亡数 (可选)
                    string deathPart = (MaxRespawnTimes != 0) ? $" / 死亡数：{stats.Deaths}" : "";
                    StatsRatingKillsAssistsDeathsTextBlock.Text = $"技术得分：{FunGameService.CalculateRating(stats):0.0#} / 击杀数：{stats.Kills} / 助攻数：{stats.Assists}{deathPart}";

                    // 第二行：存活时长 / 存活回合数 / 行动回合数
                    StatsLiveTimeRoundTurnTextBlock.Text = $"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn}";

                    // 第三行：控制时长 / 总计治疗 / 护盾抵消
                    StatsControlHealShieldTextBlock.Text = $"控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}";

                    // 第四行：总计伤害 / 总计物理伤害 / 总计魔法伤害
                    StatsTotalDamageTextBlock.Text = $"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##}";

                    // 第五行：总承受伤害 / 总承受物理伤害 / 总承受魔法伤害
                    StatsTotalTakenDamageTextBlock.Text = $"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##}";

                    // 第六行：总计真实伤害 / 总承受真实伤害 (如果存在真实伤害则显示)
                    if (stats.TotalTrueDamage > 0 || stats.TotalTakenTrueDamage > 0)
                    {
                        StatsTrueDamageTextBlock.Text = $"总计真实伤害：{stats.TotalTrueDamage:0.##} / 总承受真实伤害：{stats.TotalTakenTrueDamage:0.##}";
                        StatsTrueDamageTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StatsTrueDamageTextBlock.Visibility = Visibility.Collapsed;
                    }

                    // 第七行：每秒伤害 / 每回合伤害
                    StatsDamagePerSecondTurnTextBlock.Text = $"每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}";
                }
            }
            else
            {
                // 当没有统计数据时，显示默认文本
                StatsRatingKillsAssistsDeathsTextBlock.Text = "技术得分: - / 击杀数: - / 助攻数: -";
                StatsLiveTimeRoundTurnTextBlock.Text = "存活时长: - / 存活回合数: - / 行动回合数: -";
                StatsControlHealShieldTextBlock.Text = "控制时长: - / 总计治疗: - / 护盾抵消: -";
                StatsTotalDamageTextBlock.Text = "总计伤害: - / 总计物理伤害: - / 总计魔法伤害: -";
                StatsTotalTakenDamageTextBlock.Text = "总承受伤害: - / 总承受物理伤害: - / 总承受魔法伤害: -";
                StatsTrueDamageTextBlock.Text = "总计真实伤害: - / 总承受真实伤害: -";
                StatsDamagePerSecondTurnTextBlock.Text = "每秒伤害: - / 每回合伤害: -";
            }
        }

        /// <summary>
        /// 更新格子信息面板显示指定格子的信息。
        /// </summary>
        /// <param name="grid">要显示信息的格子。</param>
        private void UpdateGridInfoPanel(Grid grid)
        {
            string effectNames = grid.Effects.Count != 0
                                     ? string.Join(", ", grid.Effects.Select(ef => ef.GetType().Name))
                                     : "无";

            GridIdTextBlock.Text = $"ID: {grid.Id}";
            GridCoordTextBlock.Text = $"坐标: ({grid.X}, {grid.Y}, {grid.Z})";
            GridColorTextBlock.Text = $"颜色: {grid.Color.Name}";
            GridEffectsTextBlock.Text = $"效果: {effectNames}";

            // 更新GridCharactersInfoItemsControl
            GridCharactersInfoItemsControl.ItemsSource = grid.Characters;

            GridInfoPanel.Visibility = Visibility.Visible; // 显示格子信息面板
        }

        /// <summary>
        /// 处理格子点击事件：更新格子信息面板并高亮选中格子，并设置当前角色
        /// </summary>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle clickedRect && _uiElementToGrid.TryGetValue(clickedRect, out Grid? grid))
            {
                // 如果处于目标选择模式，点击格子目前不进行任何操作（因为目标是角色）
                if (_isSelectingTargets) return;

                // 移除所有格子的旧高亮效果
                ClearGridHighlights();
                // 高亮当前选中的格子
                clickedRect.Stroke = Brushes.Red; // 红色边框
                clickedRect.StrokeThickness = 2;   // 更粗的边框

                // 更新格子信息面板
                UpdateGridInfoPanel(grid);

                // --- 设置 CurrentCharacter 属性 ---
                // 如果格子上有角色，则默认选中第一个角色。
                if (grid.Characters.Count != 0)
                {
                    this.CurrentCharacter = grid.Characters.First();
                }
                else
                {
                    // 如果格子为空，将 CurrentCharacter 设置为 PlayerCharacter
                    this.CurrentCharacter = this.PlayerCharacter;
                }

                AppendDebugLog($"选中格子: ID={grid.Id}, 坐标=({grid.X},{grid.Y},{grid.Z})");
                e.Handled = true; // 标记事件已处理，防止冒泡到Canvas
            }
        }

        /// <summary>
        /// 处理角色图标点击事件：选中角色并高亮其所在格子，或进行目标选择。
        /// </summary>
        private void CharacterIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && _uiElementToCharacter.TryGetValue(clickedBorder, out Character? character))
            {
                if (_isSelectingTargets)
                {
                    // 如果处于目标选择模式，则处理目标选择逻辑
                    HandleTargetSelectionClick(character);
                }
                else
                {
                    // 否则，正常选中角色并更新UI
                    this.CurrentCharacter = character; // 设置当前选中角色

                    // 找到角色所在的格子
                    if (CurrentGameMap != null && CurrentGameMap.Characters.TryGetValue(character, out Grid? grid))
                    {
                        ClearGridHighlights(); // 清除所有格子的旧高亮效果
                        // 高亮当前选中的格子
                        if (_gridIdToUiElement.TryGetValue(grid.Id, out Rectangle? gridRect))
                        {
                            gridRect.Stroke = Brushes.Red; // 红色边框
                            gridRect.StrokeThickness = 2;   // 更粗的边框
                        }
                        UpdateGridInfoPanel(grid);
                    }
                    AppendDebugLog($"选中角色: {character.ToStringWithLevel()} (通过点击图标)");
                }
                e.Handled = true; // 阻止事件冒泡到下方的Grid_MouseLeftButtonDown 或 GameMapCanvas_MouseLeftButtonDown
            }
        }

        /// <summary>
        /// 处理Canvas空白区域的点击事件，用于取消选择并重置CurrentCharacter为PlayerCharacter。
        /// </summary>
        private void GameMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果处于目标选择模式，不响应空白区域点击
            if (_isSelectingTargets) return;

            // 只有当点击事件的原始源是Canvas本身时才处理，这意味着没有点击到任何子元素（如格子或角色图标）
            if (e.OriginalSource == GameMapCanvas)
            {
                AppendDebugLog("点击了地图空白区域。");
                // 隐藏格子信息面板
                GridInfoPanel.Visibility = Visibility.Collapsed;
                // 清除所有格子的边框高亮
                ClearGridHighlights();
                // 将当前角色设置回玩家角色
                this.CurrentCharacter = this.PlayerCharacter;
            }
        }

        /// <summary>
        /// 关闭格子信息面板的点击事件处理
        /// </summary>
        private void CloseGridInfoButton_Click(object sender, RoutedEventArgs e)
        {
            GridInfoPanel.Visibility = Visibility.Collapsed;
            ClearGridHighlights(); // 关闭时清除格子高亮
            // 关闭格子信息面板时，如果CurrentCharacter不是PlayerCharacter，可以考虑将其设置回PlayerCharacter
            if (CurrentCharacter != PlayerCharacter)
            {
                CurrentCharacter = PlayerCharacter;
            }
        }

        /// <summary>
        /// 清除所有格子的边框高亮。
        /// </summary>
        private void ClearGridHighlights()
        {
            foreach (var item in _gridIdToUiElement.Values)
            {
                item.Stroke = Brushes.Black; // 恢复默认边框颜色
                item.StrokeThickness = 0.5;  // 恢复默认边框粗细
            }
        }

        // --- 新增：装备槽位点击事件和辅助方法 ---
        private void EquipSlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder)
            {
                ClearEquipSlotHighlights(); // 清除所有装备槽位的旧高亮
                ClearStatusIconHighlights(); // 清除所有状态图标的旧高亮
                clickedBorder.BorderBrush = Brushes.Blue; // 高亮当前点击的槽位
                clickedBorder.BorderThickness = new Thickness(2);

                Item? item = null;
                if (CurrentCharacter?.EquipSlot != null)
                {
                    if (clickedBorder == MagicCardPackBorder) item = CurrentCharacter.EquipSlot.MagicCardPack;
                    else if (clickedBorder == WeaponBorder) item = CurrentCharacter.EquipSlot.Weapon;
                    else if (clickedBorder == ArmorBorder) item = CurrentCharacter.EquipSlot.Armor;
                    else if (clickedBorder == ShoesBorder) item = CurrentCharacter.EquipSlot.Shoes;
                    else if (clickedBorder == Accessory1Border) item = CurrentCharacter.EquipSlot.Accessory1;
                    else if (clickedBorder == Accessory2Border) item = CurrentCharacter.EquipSlot.Accessory2;
                }

                if (item != null)
                {
                    DescriptionTextBlock.Text = item.Description ?? item.Name; // 显示物品描述，如果不存在则显示名称
                    AppendDebugLog($"查看装备: {item.Name}");
                }
                else
                {
                    DescriptionTextBlock.Text = "此槽位未装备物品。";
                    AppendDebugLog("查看空装备槽位。");
                }
            }
        }

        /// <summary>
        /// 清除所有装备槽位的边框高亮。
        /// </summary>
        private void ClearEquipSlotHighlights()
        {
            MagicCardPackBorder.BorderBrush = Brushes.DarkGray;
            MagicCardPackBorder.BorderThickness = new Thickness(1);
            WeaponBorder.BorderBrush = Brushes.DarkGray;
            WeaponBorder.BorderThickness = new Thickness(1);
            ArmorBorder.BorderBrush = Brushes.DarkGray;
            ArmorBorder.BorderThickness = new Thickness(1);
            ShoesBorder.BorderBrush = Brushes.DarkGray;
            ShoesBorder.BorderThickness = new Thickness(1);
            Accessory1Border.BorderBrush = Brushes.DarkGray;
            Accessory1Border.BorderThickness = new Thickness(1);
            Accessory2Border.BorderBrush = Brushes.DarkGray;
            Accessory2Border.BorderThickness = new Thickness(1);
        }

        // --- 新增：状态图标点击事件和辅助方法 ---
        private void StatusIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is Effect effect)
            {
                ClearStatusIconHighlights(); // 清除所有状态图标的旧高亮
                ClearEquipSlotHighlights(); // 清除所有装备槽位的旧高亮
                clickedBorder.BorderBrush = Brushes.Blue; // 高亮当前点击的状态图标
                clickedBorder.BorderThickness = new Thickness(1.5);

                DescriptionTextBlock.Text = effect.Description ?? effect.GetType().Name; // 显示效果描述，如果不存在则显示类型名称
                AppendDebugLog($"查看状态: {effect.GetType().Name}");
            }
        }

        /// <summary>
        /// 清除所有状态图标的边框高亮。
        /// </summary>
        private void ClearStatusIconHighlights()
        {
            foreach (Border border in CharacterEffectsPanel.Children.OfType<Border>())
            {
                border.BorderBrush = Brushes.Gray;
                border.BorderThickness = new Thickness(0.5);
            }
        }

        // --- 操作按钮 (底部右侧) ---

        /// <summary>
        /// 启用或禁用所有操作按钮。
        /// </summary>
        /// <param name="enabled">是否启用。</param>
        private void SetActionButtonsEnabled(bool enabled)
        {
            MoveButton.IsEnabled = enabled;
            AttackButton.IsEnabled = enabled;
            SkillButton.IsEnabled = enabled;
            CastButton.IsEnabled = enabled;
            UseItemButton.IsEnabled = enabled;
            EndTurnButton.IsEnabled = enabled;
        }

        /// <summary>
        /// 操作按钮的通用点击事件处理。
        /// </summary>
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CharacterActionType actionType)
            {
                _resolveActionType?.Invoke(actionType);
            }
        }

        // --- UI 提示方法 (由 GameMapController 调用) ---

        /// <summary>
        /// 显示角色选择提示面板。
        /// </summary>
        /// <param name="availableCharacters">可供选择的角色列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowCharacterSelectionPrompt(List<Character> availableCharacters, Action<long> callback)
        {
            _resolveCharacterSelection = callback;
            CharacterSelectionItemsControl.ItemsSource = availableCharacters;
            CharacterSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 角色选择项的点击事件。
        /// </summary>
        private void CharacterSelectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is long characterId)
            {
                _resolveCharacterSelection?.Invoke(characterId);
            }
        }

        /// <summary>
        /// 隐藏角色选择提示面板。
        /// </summary>
        public void HideCharacterSelectionPrompt()
        {
            CharacterSelectionOverlay.Visibility = Visibility.Collapsed;
            CharacterSelectionItemsControl.ItemsSource = null;
            _resolveCharacterSelection = null;
        }

        /// <summary>
        /// 显示行动按钮，并根据可用技能和物品启用/禁用相关按钮。
        /// </summary>
        /// <param name="character">当前行动的角色。</param>
        /// <param name="availableSkills">可用的技能列表。</param>
        /// <param name="availableItems">可用的物品列表。</param>
        /// <param name="callback">选择行动后调用的回调函数。</param>
        public void ShowActionButtons(Character character, List<Skill> availableSkills, List<Item> availableItems, Action<CharacterActionType> callback)
        {
            _resolveActionType = callback;
            SetActionButtonsEnabled(true);

            // 根据实际情况启用/禁用技能和物品按钮
            // 技能按钮：检查是否有任何技能是当前角色可施放的
            SkillButton.IsEnabled = character.Skills.Any(availableSkills.Contains);
            // 物品按钮：检查是否有任何物品可用
            UseItemButton.IsEnabled = availableItems.Count != 0;
            // 移动按钮：假设总是可用，或者需要更复杂的逻辑来判断

            // 如果当前角色不是正在行动的角色，更新CurrentCharacter
            if (CurrentCharacter != character)
            {
                CurrentCharacter = character; // 这将触发UpdateBottomInfoPanel
            }
        }

        /// <summary>
        /// 隐藏所有行动按钮。
        /// </summary>
        public void HideActionButtons()
        {
            SetActionButtonsEnabled(false);
            _resolveActionType = null;
        }

        /// <summary>
        /// 显示技能选择UI。
        /// </summary>
        /// <param name="availableSkills">可供选择的技能列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowSkillSelectionUI(Character character, List<Skill> availableSkills, Action<long> callback)
        {
            _resolveSkillSelection = callback;
            SkillItemSelectionTitle.Text = "请选择技能";
            // 只显示当前角色可施放的技能
            SkillItemSelectionItemsControl.ItemsSource = character.Skills.Where(availableSkills.Contains).ToList();
            SkillItemSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 显示物品选择UI。
        /// </summary>
        /// <param name="availableItems">可供选择的物品列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowItemSelectionUI(List<Item> availableItems, Action<long> callback)
        {
            _resolveItemSelection = callback;
            SkillItemSelectionTitle.Text = "请选择物品";
            SkillItemSelectionItemsControl.ItemsSource = availableItems;
            SkillItemSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 技能/物品选择项的点击事件。
        /// </summary>
        private void SkillItemSelectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is long id)
            {
                if (SkillItemSelectionTitle.Text == "请选择技能")
                {
                    _resolveSkillSelection?.Invoke(id);
                }
                else if (SkillItemSelectionTitle.Text == "请选择物品")
                {
                    _resolveItemSelection?.Invoke(id);
                }
            }
        }

        /// <summary>
        /// 取消技能/物品选择的点击事件。
        /// </summary>
        private void CancelSkillItemSelection_Click(object sender, RoutedEventArgs e)
        {
            if (SkillItemSelectionTitle.Text == "请选择技能")
            {
                _resolveSkillSelection?.Invoke(-1); // 返回-1表示取消
            }
            else if (SkillItemSelectionTitle.Text == "请选择物品")
            {
                _resolveItemSelection?.Invoke(-1); // 返回-1表示取消
            }
        }

        /// <summary>
        /// 隐藏技能选择UI。
        /// </summary>
        public void HideSkillSelectionUI()
        {
            SkillItemSelectionOverlay.Visibility = Visibility.Collapsed;
            SkillItemSelectionItemsControl.ItemsSource = null;
            _resolveSkillSelection = null;
        }

        /// <summary>
        /// 隐藏物品选择UI。
        /// </summary>
        public void HideItemSelectionUI()
        {
            SkillItemSelectionOverlay.Visibility = Visibility.Collapsed;
            SkillItemSelectionItemsControl.ItemsSource = null;
            _resolveItemSelection = null;
        }

        /// <summary>
        /// 显示目标选择UI。
        /// </summary>
        /// <param name="actor">发起行动的角色。</param>
        /// <param name="potentialTargets">所有潜在的可选目标列表。</param>
        /// <param name="maxTargets">最大可选目标数量。</param>
        /// <param name="canSelectSelf">是否可选择自身。</param>
        /// <param name="canSelectEnemy">是否可选择敌方。</param>
        /// <param name="canSelectTeammate">是否可选择友方。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowTargetSelectionUI(Character actor, List<Character> potentialTargets, long maxTargets, bool canSelectSelf, bool canSelectEnemy, bool canSelectTeammate, Action<List<Character>> callback)
        {
            _resolveTargetSelection = callback;
            _actingCharacterForTargetSelection = actor;
            _potentialTargetsForSelection = potentialTargets;
            _maxTargetsForSelection = maxTargets;
            _canSelectSelf = canSelectSelf;
            _canSelectEnemy = canSelectEnemy;
            _canSelectTeammate = canSelectTeammate;
            _isSelectingTargets = true; // 进入目标选择模式

            SelectedTargets.Clear(); // 清空之前的选择
            TargetSelectionTitle.Text = $"选择 {actor.NickName} 的目标 (最多 {maxTargets} 个)";
            TargetSelectionOverlay.Visibility = Visibility.Visible;

            // 更新地图上角色的高亮，以显示潜在目标和已选目标
            UpdateCharacterHighlights();
        }

        /// <summary>
        /// 处理在目标选择模式下点击角色图标的逻辑。
        /// </summary>
        /// <param name="clickedCharacter">被点击的角色。</param>
        private void HandleTargetSelectionClick(Character clickedCharacter)
        {
            // 检查是否是潜在目标
            if (_potentialTargetsForSelection == null || !_potentialTargetsForSelection.Contains(clickedCharacter))
            {
                AppendDebugLog($"无法选择 {clickedCharacter.NickName}：不是潜在目标。");
                return;
            }

            // 根据选择规则检查目标是否有效
            bool isValidTarget = false;
            if (clickedCharacter == _actingCharacterForTargetSelection && _canSelectSelf)
            {
                isValidTarget = true;
            }
            else if (_actingCharacterForTargetSelection != null)
            {
                bool isTeammate = _controller.IsTeammate(_actingCharacterForTargetSelection, clickedCharacter);
                if (_canSelectEnemy && !isTeammate)
                {
                    isValidTarget = true;
                }
                else if (_canSelectTeammate && isTeammate)
                {
                    isValidTarget = true;
                }
            }

            if (!isValidTarget)
            {
                AppendDebugLog($"无法选择 {clickedCharacter.NickName}：不符合目标选择规则。");
                return;
            }

            if (!SelectedTargets.Remove(clickedCharacter))
            {
                if (SelectedTargets.Count < _maxTargetsForSelection)
                {
                    SelectedTargets.Add(clickedCharacter);
                }
                else
                {
                    AppendDebugLog($"已达到最大目标数量 ({_maxTargetsForSelection})。");
                }
            }
            UpdateCharacterHighlights(); // 更新地图上的高亮显示
        }

        /// <summary>
        /// 更新地图上角色的高亮显示，区分潜在目标和已选目标。
        /// </summary>
        private void UpdateCharacterHighlights()
        {
            // 清除所有角色的高亮
            foreach (var border in _characterToUiElement.Values)
            {
                border.BorderBrush = Brushes.DarkBlue; // 默认颜色
                border.BorderThickness = new Thickness(1.5);
            }

            // 高亮潜在目标（黄色边框）
            if (_potentialTargetsForSelection != null)
            {
                foreach (Character potentialTarget in _potentialTargetsForSelection)
                {
                    if (_characterToUiElement.TryGetValue(potentialTarget, out Border? border))
                    {
                        border.BorderBrush = Brushes.Yellow;
                        border.BorderThickness = new Thickness(2);
                    }
                }
            }

            // 高亮当前已选目标（红色边框）
            foreach (Character selectedTarget in SelectedTargets)
            {
                if (_characterToUiElement.TryGetValue(selectedTarget, out Border? border))
                {
                    border.BorderBrush = Brushes.Red; // 已选目标颜色
                    border.BorderThickness = new Thickness(3);
                }
            }
        }

        /// <summary>
        /// 从已选目标列表中移除一个角色。
        /// </summary>
        private void RemoveTarget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is long characterId)
            {
                Character? charToRemove = SelectedTargets.FirstOrDefault(c => c.Id == characterId);
                if (charToRemove != null)
                {
                    SelectedTargets.Remove(charToRemove);
                    UpdateCharacterHighlights(); // 移除后更新高亮
                }
            }
        }

        /// <summary>
        /// 确认目标选择的点击事件。
        /// </summary>
        private void ConfirmTargets_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetSelection?.Invoke([.. SelectedTargets.Select(c => c)]);
        }

        /// <summary>
        /// 取消目标选择的点击事件。
        /// </summary>
        private void CancelTargetSelection_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetSelection?.Invoke([]); // 返回空表示取消
        }

        /// <summary>
        /// 隐藏目标选择UI。
        /// </summary>
        public void HideTargetSelectionUI()
        {
            TargetSelectionOverlay.Visibility = Visibility.Collapsed;
            SelectedTargets.Clear();
            _isSelectingTargets = false; // 退出目标选择模式
            _actingCharacterForTargetSelection = null;
            _potentialTargetsForSelection = [];
            _resolveTargetSelection = null;
            UpdateCharacterHighlights(); // 清除所有高亮
        }

        /// <summary>
        /// 显示“继续”提示面板。
        /// </summary>
        /// <param name="message">提示消息。</param>
        /// <param name="callback">点击“继续”后调用的回调函数。</param>
        public void ShowContinuePrompt(string message, Action<bool> callback)
        {
            _resolveContinuePrompt = callback;
            ContinuePromptTextBlock.Text = message;
            ContinuePromptOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// “继续”按钮的点击事件。
        /// </summary>
        private void ContinuePromptButton_Click(object sender, RoutedEventArgs e)
        {
            _resolveContinuePrompt?.Invoke(true);
        }

        /// <summary>
        /// 隐藏“继续”提示面板。
        /// </summary>
        public void HideContinuePrompt()
        {
            ContinuePromptOverlay.Visibility = Visibility.Collapsed;
            _resolveContinuePrompt = null;
        }

        /// <summary>
        /// 辅助方法：将 System.Drawing.Color 转换为 System.Windows.Media.SolidColorBrush
        /// WPF UI元素使用System.Windows.Media.Brush，而Grid类使用System.Drawing.Color
        /// </summary>
        /// <param name="color">System.Drawing.Color对象</param>
        /// <returns>System.Windows.Media.SolidColorBrush对象</returns>
        private static SolidColorBrush ToWpfBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}

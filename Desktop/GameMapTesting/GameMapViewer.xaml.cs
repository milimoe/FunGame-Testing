using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using static Milimoe.FunGame.Core.Library.Constant.General;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Grid = Milimoe.FunGame.Core.Library.Common.Addon.Grid;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using RichTextBox = System.Windows.Controls.RichTextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
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

        // 新增：用于目标选择UI的ObservableCollection
        public ObservableCollection<Character> SelectedTargets { get; set; } = [];

        // 新增：用于移动目标选择UI的ObservableCollection
        public ObservableCollection<Grid> SelectedTargetGrid { get; set; } = [];

        // 新增：用于非指向性目标选择UI的ObservableCollection
        public ObservableCollection<Grid> SelectedTargetGrids { get; set; } = [];

        public DecisionPoints DP { get; set; } = new();

        // 新增：倒计时相关的字段
        private int _remainingCountdownSeconds;
        private Action<bool>? _currentContinueCallback;

        // 回调Action，用于将UI选择结果传递给Controller
        private Action<Character?>? _resolveCharacterSelection;
        private Action<InquiryResponse?>? _resolveInquiryResponseSelection;
        private Action<Grid?>? _resolveTargetGridSelection;
        private Action<List<Grid>>? _resolveTargetGridsSelection;
        private Action<CharacterActionType>? _resolveActionType;
        private Action<List<Character>>? _resolveTargetSelection;
        private Action<Skill?>? _resolveSkillSelection;
        private Action<Item?>? _resolveItemSelection;
        private Action<bool>? _resolveContinuePrompt;

        // 目标选择的内部状态
        private Character? _actingCharacterForTargetSelection;
        private List<Character> _potentialTargetsForSelection = [];
        private long _maxTargetsForSelection;
        private bool _canSelectAllTeammates, _canSelectAllEnemies, _canSelectSelf, _canSelectEnemy, _canSelectTeammate;
        private bool _isSelectingTargets = false; // 标记当前是否处于目标选择模式
        private readonly CharacterQueueItem _selectionPredictCharacter = new(Factory.GetCharacter(), 0); // 选择时进行下轮预测（用于行动顺序表显示）
        private Grid? _actingCharacterCurrentGridForTargetSelection;
        private bool _isSelectingTargetGrid = false; // 标记当前是否处于格子选择模式
        private List<Grid> _potentialTargetGridForSelection = [];
        private Brush DefaultGridBrush { get; } = ToWpfBrush(System.Drawing.Color.Gray);

        // 新增：用于跟踪当前高亮的技能和物品图标
        private Border? _highlightedSkillIcon;
        private Border? _highlightedItemIcon;

        // 新增：用于非指向性技能的鼠标移动高亮
        private List<Grid> _mouseHoverHighlightedGrids = [];
        private Skill? _currentNonDirectionalSkill = null;
        private Grid? _hoveredGrid = null;

        // 新增：非指向性技能选择的相关状态
        private bool _isSelectingNonDirectionalGrids = false;

        // 模式
        private bool _isAutoMode = false;
        private bool _isFastMode = false;

        public GameMapViewer()
        {
            InitializeComponent();
            CharacterQueueItems = [];
            _selectionPredictCharacter.Character.Promotion = 1800;
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

        // 当前回合
        public static readonly DependencyProperty CurrentRoundProperty =
            DependencyProperty.Register("CurrentRound", typeof(int), typeof(GameMapViewer),
                                        new PropertyMetadata(0, OnCurrentRoundChanged));

        public int CurrentRound
        {
            get { return (int)GetValue(CurrentRoundProperty); }
            set { SetValue(CurrentRoundProperty, value); }
        }

        // 回合奖励
        public static readonly DependencyProperty TurnRewardsProperty =
            DependencyProperty.Register("TurnRewards", typeof(Dictionary<int, List<Skill>>), typeof(GameMapViewer),
                                        new PropertyMetadata(new Dictionary<int, List<Skill>>(), OnTurnRewardsChanged));

        public Dictionary<int, List<Skill>> TurnRewards
        {
            get { return (Dictionary<int, List<Skill>>)GetValue(TurnRewardsProperty); }
            set { SetValue(TurnRewardsProperty, value); }
        }

        // 新增 CurrentCharacter 依赖属性：用于显示当前玩家角色的信息
        public static readonly DependencyProperty CurrentCharacterProperty =
            DependencyProperty.Register("CurrentCharacter", typeof(Character), typeof(GameMapViewer),
                                        new PropertyMetadata(null, OnCurrentCharacterChanged));

        public static readonly DependencyProperty CharacterQueueItemsProperty =
        DependencyProperty.Register("CharacterQueueItems", typeof(ObservableCollection<CharacterQueueItem>), typeof(GameMapViewer),
                                    new PropertyMetadata(null, OnCharacterQueueItemsChanged));

        // 用于左侧动态队列的ObservableCollection
        public ObservableCollection<CharacterQueueItem> CharacterQueueItems
        {
            get { return (ObservableCollection<CharacterQueueItem>)GetValue(CharacterQueueItemsProperty); }
            set { SetValue(CharacterQueueItemsProperty, value); }
        }

        // 用于 UI 绑定的 ViewModel 集合
        public ObservableCollection<CharacterQueueItemViewModel> CharacterQueueDisplayItems { get; } = [];
        public ObservableCollection<CharacterViewModel> TeammateCharacters { get; set; } = [];
        public ObservableCollection<CharacterViewModel> EnemyCharacters { get; set; } = [];

        // 技能组
        public CharacterSkillsAndItemsViewModel CharacterSkillsAndItems { get; set; } = new();

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

        public async Task InvokeAsync(Action action)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(action);
            }
            else
            {
                action();
            }
        }

        public async Task InvokeAsync(Func<Task> task)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(task);
            }
            else await task();
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
            _ = viewer.RenderMap();
            _ = viewer.UpdateCharacterPositionsOnMap();
        }

        // CurrentRound
        private static void OnCurrentRoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            viewer.CurrentRoundChanged();
            viewer.UpdateCharacterQueueDisplayItems();
        }

        private static void OnCharacterQueueItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GameMapViewer viewer)
            {
                // 解除旧集合的事件订阅
                if (e.OldValue is ObservableCollection<CharacterQueueItem> oldCollection)
                {
                    oldCollection.CollectionChanged -= viewer.CharacterQueueItems_CollectionChanged;
                }
                // 订阅新集合的事件
                if (e.NewValue is ObservableCollection<CharacterQueueItem> newCollection)
                {
                    newCollection.CollectionChanged += viewer.CharacterQueueItems_CollectionChanged;
                }
                // 立即更新显示队列
                viewer.UpdateCharacterQueueDisplayItems();
            }
        }

        // 当原始队列 CharacterQueueItems 内部发生变化时 (添加、删除、移动、替换)
        private void CharacterQueueItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 任何集合内容的变动或排序变动，都需要重新计算 PredictedTurnNumber 和奖励
            // 因为索引变了，PredictedTurnNumber 就会变，进而可能影响奖励
            UpdateCharacterQueueDisplayItems();
        }

        // TurnRewards
        private static void OnTurnRewardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            foreach (CharacterQueueItemViewModel vm in viewer.CharacterQueueDisplayItems)
            {
                vm.UpdateRewardProperties();
            }
        }

        // 当CurrentCharacter属性改变时，更新底部信息面板
        private static void OnCurrentCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            _ = viewer.UpdateBottomInfoPanel(viewer.DP);
            _ = viewer.UpdateCharacterStatisticsPanel(); // 角色改变时也更新统计面板
            // 角色改变时，清除装备/状态/技能/物品描述和高亮
            viewer.ResetDescriptionAndHighlights(); // 使用新的重置方法
        }

        // 新增：当CharacterQueueData属性改变时，更新左侧队列面板
        private static void OnCharacterQueueDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            _ = viewer.UpdateLeftQueuePanelGrid(viewer.DP);
        }

        // 新增：当CharacterStatistics属性改变时，更新数据统计面板
        private static void OnCharacterStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            _ = viewer.UpdateCharacterStatisticsPanel();
        }

        // 新增：当MaxRespawnTimes属性改变时，更新数据统计面板（因为会影响死亡数显示）
        private static void OnMaxRespawnTimesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameMapViewer viewer = (GameMapViewer)d;
            _ = viewer.UpdateCharacterStatisticsPanel();
        }

        /// <summary>
        /// 向调试日志文本框添加一条消息。
        /// </summary>
        /// <param name="message">要添加的日志消息。</param>
        public async Task AppendDebugLog(string message)
        {
            // 检查当前线程是否是UI线程
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(async () => await AppendDebugLog(message));
                return;
            }

            int maxLines = 250;

            // 获取 FlowDocument
            FlowDocument doc = DebugLogRichTextBox.Document;
            if (doc == null)
            {
                doc = new FlowDocument();
                DebugLogRichTextBox.Document = doc;
            }

            // 如果是初始的“调试日志:”段落，则清空它
            if (doc.Blocks.FirstBlock is Paragraph firstParagraph && firstParagraph.Inlines.FirstInline is Run firstRun && firstRun.Text == "调试日志:")
            {
                doc.Blocks.Clear();
            }

            // 获取当前滚动位置
            double verticalOffsetBefore = DebugLogScrollViewer.VerticalOffset;

            // 添加新的段落
            Paragraph newParagraph = new(new Run(message))
            {
                Margin = new Thickness(0) // 移除默认段落间距
            };
            doc.Blocks.Add(newParagraph);

            // 限制行数
            while (doc.Blocks.Count > maxLines)
            {
                doc.Blocks.Remove(doc.Blocks.FirstBlock);
            }

            bool wasAtBottom = verticalOffsetBefore == 0;

            if (wasAtBottom)
            {
                // 滚动到底部
                DebugLogRichTextBox.ScrollToEnd();
                DebugLogScrollViewer.ScrollToEnd();
            }
        }

        private void CurrentRoundChanged()
        {
            QueueTitle.Text = $"行动顺序表{(CurrentRound > 0 ? $" - 第 {CurrentRound} 回合" : "")}";
        }

        /// <summary>
        /// 渲染地图：根据CurrentGameMap对象在Canvas上绘制所有格子
        /// </summary>
        private async Task RenderMap()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(async () => await RenderMap());
                return;
            }

            GameMapCanvas.Children.Clear(); // 清除Canvas上所有旧的UI元素 (包括角色图标，后续会重新绘制)
            _gridIdToUiElement.Clear();     // 清除旧的关联
            _uiElementToGrid.Clear();       // 清除旧的关联
            _characterToUiElement.Clear();  // 清除旧的角色UI关联
            _uiElementToCharacter.Clear();  // 清除旧的角色UI反向关联
            GridInfoPanel.Visibility = Visibility.Collapsed; // 地图重绘时隐藏格子信息面板

            if (CurrentGameMap == null)
            {
                _ = AppendDebugLog("地图未加载。");
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
                Panel.SetZIndex(rect, 5); // 确保格子在底部，但留出一些空间。

                rect.MouseLeftButtonDown += Grid_MouseLeftButtonDown;

                GameMapCanvas.Children.Add(rect);

                _gridIdToUiElement.Add(grid.Id, rect);
                _uiElementToGrid.Add(rect, grid);

                maxCanvasWidth = Math.Max(maxCanvasWidth, (grid.X + 1) * CurrentGameMap.Size);
                maxCanvasHeight = Math.Max(maxCanvasHeight, (grid.Y + 1) * CurrentGameMap.Size);
            }

            GameMapCanvas.Width = maxCanvasWidth;
            GameMapCanvas.Height = maxCanvasHeight;

            if (_controller.TeamMode && PlayerCharacter != null)
            {
                List<Team> teams = _controller.GetTeams();
                foreach (Team team in teams)
                {
                    if (team.IsOnThisTeam(PlayerCharacter))
                    {
                        TeammateTextBlock.Text = team.Name;
                    }
                    else
                    {
                        EnemyTextBlock.Text = team.Name;
                    }
                }
            }
        }

        /// <summary>
        /// 在地图上更新所有角色的位置和显示。
        /// </summary>
        /// <summary>
        /// 在地图上更新所有角色的位置和显示。
        /// </summary>
        public async Task UpdateCharacterPositionsOnMap()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(async () => await UpdateCharacterPositionsOnMap());
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
                    ToolTip = character.GetInfo(showMapRelated: true),
                    IsHitTestVisible = true // 确保角色图标可以被点击
                };

                // --- 根据HP动态设置背景颜色 END ---

                TextBlock characterText = new()
                {
                    Style = (Style)this.FindResource("CharacterIconTextStyle"),
                    Text = character.NickName.Length > 0 ? character.NickName[0].ToString().ToUpper() : "?"
                };

                // --- 根据HP动态设置背景颜色 START ---
                double hpPercentage = character.HP / character.MaxHP;

                if (hpPercentage > 0.75)
                {
                    characterBorder.Background = Brushes.Green;
                }
                else if (hpPercentage > 0.50)
                {
                    characterBorder.Background = Brushes.Yellow;
                    characterText.Foreground = Brushes.Black;
                }
                else if (hpPercentage > 0.25)
                {
                    characterBorder.Background = Brushes.Orange;
                    characterText.Foreground = Brushes.Black;
                }
                else if (hpPercentage > 0)
                {
                    characterBorder.Background = Brushes.Red;
                }
                else
                {
                    characterBorder.Background = Brushes.Gray;
                }
                characterBorder.Child = characterText;
                characterBorder.BorderBrush = character.Promotion switch
                {
                    200 => Brushes.BurlyWood,
                    300 => Brushes.SkyBlue,
                    400 => Brushes.Orchid,
                    _ => Brushes.Salmon
                };
                characterBorder.BorderThickness = new Thickness(2);

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

            if (_controller.TeamMode && PlayerCharacter != null)
            {
                List<Team> teams = _controller.GetTeams();
                if (teams.Count > 0)
                {
                    PointsTextBlock.Visibility = Visibility.Visible;
                    PointsTextBlock.Text = $"{string.Join(" : ", teams.OrderBy(t => t.IsOnThisTeam(PlayerCharacter) ? 0 : 1).Select(t => $"{t.Name} ({t.Score})"))}";
                }
            }
            else
            {
                PointsTextBlock.Visibility = Visibility.Visible;
                PointsTextBlock.Text = $"剩余 {CharacterQueueDisplayItems.Count} 人";
            }
        }

        /// <summary>
        /// 更新左侧动态队列面板，显示角色及其AT Delay。
        /// </summary>
        public async Task UpdateLeftQueuePanelGrid(DecisionPoints dp)
        {
            // 确保在UI线程上执行
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(async () => await UpdateLeftQueuePanelGrid(dp));
                return;
            }

            DP = dp;
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
        public async Task UpdateBottomInfoPanel(DecisionPoints dp)
        {
            // 确保在UI线程上执行
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.InvokeAsync(async () => await UpdateBottomInfoPanel(dp));
                return;
            }

            DP = dp;

            // 每次更新面板时，清除所有描述和高亮
            ResetDescriptionAndHighlights(true);

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
                PreCastButton.IsEnabled = character.CharacterState == CharacterState.Actionable || character.CharacterState == CharacterState.AttackRestricted ||
                    character.CharacterState == CharacterState.Casting || character.HP > 0 || character.EP >= 100;

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
                            ToolTip = effect.ToString() // 鼠标悬停显示完整效果名称或描述
                        };
                        TextBlock effectText = new()
                        {
                            Style = (Style)this.FindResource("StatusIconTextStyle"),
                            Text = effect.Name.Length > 0 ? effect.Name[0].ToString() : "?"
                        };
                        effectBorder.Child = effectText;
                        effectBorder.Tag = effect; // 存储Effect对象，以便点击时获取其描述
                        effectBorder.MouseLeftButtonDown += StatusIcon_MouseLeftButtonDown; // 添加点击事件
                        CharacterEffectsPanel.Children.Add(effectBorder);
                    }
                }

                // 更新技能组
                CharacterSkillsAndItems.Skills = [character.NormalAttack, .. character.Skills];
                CharacterSkillsAndItems.Items = [.. character.Items];

                // --- 更新其他角色属性 ---
                bool showGrowth = false; // 假设不显示成长值

                double exHP = character.ExHP + character.ExHP2 + character.ExHP3;
                List<string> shield = [];
                if (character.Shield.TotalPhysical > 0) shield.Add($"物理：{character.Shield.TotalPhysical:0.##}");
                if (character.Shield.TotalMagical > 0) shield.Add($"魔法：{character.Shield.TotalMagical:0.##}");
                if (character.Shield.TotalMix > 0) shield.Add($"混合：{character.Shield.TotalMix:0.##}");
                HpTextBlock.Text = $"生命值：{character.HP:0.##} / {character.MaxHP:0.##}" + (exHP != 0 ? $" [{character.BaseHP:0.##} {(exHP >= 0 ? "+" : "-")} {Math.Abs(exHP):0.##}]" : "") + (shield.Count > 0 ? $"（{string.Join("，", shield)}）" : "");

                double exMP = character.ExMP + character.ExMP2 + character.ExMP3;
                MpTextBlock.Text = $"魔法值：{character.MP:0.##} / {character.MaxMP:0.##}" + (exMP != 0 ? $" [{character.BaseMP:0.##} {(exMP >= 0 ? "+" : "-")} {Math.Abs(exMP):0.##}]" : "");

                double exATK = character.ExATK + character.ExATK2 + character.ExATK3;
                AttackTextBlock.Text = $"攻击力：{character.ATK:0.##}" + (exATK != 0 ? $" [{character.BaseATK:0.##} {(exATK >= 0 ? "+" : "-")} {Math.Abs(exATK):0.##}]" : "");

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
                MovTextBlock.Text = $"移动距离：{character.MOV}";
                AtrTextBlock.Text = $"攻击距离：{character.ATR}";
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

                // 清空技能组
                CharacterSkillsAndItems.Skills = [];
                CharacterSkillsAndItems.Items = [];

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
            await UpdateCharacterStatisticsPanel();
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
                parentBorder.ToolTip = item.ToString(); // 显示物品名称或描述作为ToolTip
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
        public async Task UpdateCharacterStatisticsPanel()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                await this.Dispatcher.Invoke(async () => await UpdateCharacterStatisticsPanel());
                return;
            }

            // 清空所有统计文本块
            StatsTextBlock1.Text = "";
            StatsTextBlock2.Text = "";
            StatsTextBlock3.Text = "";
            StatsTextBlock4.Text = "";

            // 尝试将传入的 CharacterStatistics 对象转换为 dynamic 类型，以便访问其属性
            Dictionary<Character, CharacterStatistics> dict = CharacterStatistics;
            if (dict != null)
            {
                CharacterStatistics? stats = dict.Where(kv => kv.Key == CurrentCharacter).Select(kv => kv.Value).FirstOrDefault();
                if (stats != null)
                {
                    StatsTextBlock1.Text = $"击杀数：{stats.Kills} / 助攻数：{stats.Assists} / 死亡数：{stats.Deaths} / 每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}";
                    StatsTextBlock2.Text = $"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn} / 控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}";
                    StatsTextBlock3.Text = $"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##} / 总计真实伤害：{stats.TotalTrueDamage:0.##}";
                    StatsTextBlock4.Text = $"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##} / 总承受真实伤害：{stats.TotalTakenTrueDamage:0.##}";
                    StatsTextBlock4.Text = $"总计决策数：{stats.TurnDecisions} / 使用决策点：{stats.UseDecisionPoints} / 当前决策点：{DP.CurrentDecisionPoints} / {DP.MaxDecisionPoints}";
                }
            }
            else
            {
                // 当没有统计数据时，显示默认文本
                StatsTextBlock1.Text = "击杀数：- / 助攻数：- / 死亡数：- / 每秒伤害：- / 每回合伤害：-";
                StatsTextBlock2.Text = "存活时长：- / 存活回合数：- / 行动回合数：- / 控制时长：- / 总计治疗：- / 护盾抵消：-";
                StatsTextBlock3.Text = "总计伤害：- / 总计物理伤害：- / 总计魔法伤害：- / 总计真实伤害：-";
                StatsTextBlock4.Text = "总承受伤害：- / 总承受物理伤害：- / 总承受魔法伤害：- / 总承受真实伤害：-";
                StatsTextBlock4.Text = "总计决策数：- / 使用决策点：-";
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

            if (grid.Characters.Count > 0)
            {
                _potentialTargetGridForSelection = CurrentGameMap.GetGridsByRange(grid, grid.Characters.First().MOV, true);
                UpdateGridHighlights();
            }
        }

        /// <summary>
        /// 处理格子点击事件：更新格子信息面板并高亮选中格子，并设置当前角色
        /// </summary>
        private async void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle clickedRect && _uiElementToGrid.TryGetValue(clickedRect, out Grid? grid))
            {
                // 如果处于非指向性技能目标选择模式
                if (_isSelectingNonDirectionalGrids)
                {
                    await HandleNonDirectionalGridSelectionClick(grid);
                    e.Handled = true;
                    return;
                }

                // 如果处于普通目标格子选择模式
                if (_isSelectingTargetGrid)
                {
                    await HandleTargetGridSelectionClick(grid);
                    e.Handled = true;
                    return;
                }

                // 如果处于目标选择模式，点击格子目前不进行任何操作（因为目标是角色）
                if (_isSelectingTargets) return;

                // 移除所有格子的旧高亮效果
                ClearGridHighlights();
                // 高亮当前选中的格子
                clickedRect.Stroke = Brushes.Red; // 红色边框
                clickedRect.StrokeThickness = 2;   // 更粗的边框

                // 更新格子信息面板
                UpdateGridInfoPanel(grid);

                await AppendDebugLog($"选中格子: ID={grid.Id}, 坐标=({grid.X},{grid.Y},{grid.Z})");

                if (_isSelectingTargetGrid)
                {
                    await HandleTargetGridSelectionClick(grid);
                    e.Handled = true;
                    return;
                }

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

                e.Handled = true; // 标记事件已处理，防止冒泡到Canvas
            }
        }

        /// <summary>
        /// 处理角色列表项的点击事件。模拟地图上点击角色的行为。
        /// </summary>
        private async void CharacterSummary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 确保点击的是一个 Border 且其 Tag 绑定了 CharacterViewModel
            if (sender is Border border && border.Tag is CharacterViewModel vm)
            {
                Character clickedCharacter = vm.Character; // 从 ViewModel 中获取实际的 Character 对象

                if (_isSelectingTargets)
                {
                    await HandleTargetSelectionClick(clickedCharacter);
                }
                else if (CurrentGameMap != null && CurrentGameMap.Characters.TryGetValue(clickedCharacter, out Grid? characterGrid))
                {
                    ClearGridHighlights();

                    if (_isSelectingTargetGrid)
                    {
                        await HandleTargetGridSelectionClick(characterGrid);
                    }

                    if (_gridIdToUiElement.TryGetValue(characterGrid.Id, out Rectangle? gridRect))
                    {
                        gridRect.Stroke = Brushes.Red;
                        gridRect.StrokeThickness = 2;
                    }

                    UpdateGridInfoPanel(characterGrid);

                    await AppendDebugLog($"选中角色: {clickedCharacter.ToStringWithLevel()} (通过侧边面板点击)");
                }
                else
                {
                    await AppendDebugLog($"错误: 无法找到角色 {clickedCharacter.NickName} 所在的格子。");
                    // 此时可以隐藏格子信息面板
                    GridInfoPanel.Visibility = Visibility.Collapsed;
                }

                // 标记事件已处理，防止冒泡到下方的 Grid_MouseLeftButtonDown 或 GameMapCanvas_MouseLeftButtonDown
                e.Handled = true;
            }
        }

        /// <summary>
        /// 处理角色图标点击事件：选中角色并高亮其所在格子，或进行目标选择。
        /// </summary>
        private async void CharacterIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && _uiElementToCharacter.TryGetValue(clickedBorder, out Character? character))
            {
                if (_isSelectingNonDirectionalGrids)
                {
                    // 找到角色所在的格子
                    if (CurrentGameMap != null && CurrentGameMap.Characters.TryGetValue(character, out Grid? characterGrid))
                    {
                        // 尝试选择角色所在的格子
                        await HandleNonDirectionalGridSelectionClick(characterGrid);
                    }
                }
                else if (_isSelectingTargetGrid)
                {
                    if (CurrentGameMap != null && CurrentGameMap.Characters.TryGetValue(character, out Grid? characterGrid))
                    {
                        await HandleTargetGridSelectionClick(characterGrid);
                    }
                }
                else if (_isSelectingTargets)
                {
                    // 如果处于目标选择模式，则处理目标选择逻辑
                    await HandleTargetSelectionClick(character);
                }
                else
                {
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
                        if (_isSelectingTargetGrid)
                        {
                            await HandleTargetGridSelectionClick(grid);
                            return;
                        }
                    }

                    // 正常选中角色并更新UI
                    this.CurrentCharacter = character; // 设置当前选中角色

                    await AppendDebugLog($"选中角色: {character.ToStringWithLevel()} (通过点击图标)");
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
            if (_isSelectingTargets || _isSelectingTargetGrid) return;

            // 只有当点击事件的原始源是Canvas本身时才处理，这意味着没有点击到任何子元素（如格子或角色图标）
            if (e.OriginalSource == GameMapCanvas)
            {
                _ = AppendDebugLog("点击了地图空白区域。");
                // 调用关闭格子信息面板的逻辑，它现在也会重置描述和高亮
                CloseGridInfoButton_Click(new(), new());
                // 将当前角色设置回玩家角色
                this.CurrentCharacter = this.PlayerCharacter;
                e.Handled = true; // 标记事件已处理，防止冒泡
            }
        }

        /// <summary>
        /// 关闭格子信息面板的点击事件处理
        /// </summary>
        private void CloseGridInfoButton_Click(object sender, RoutedEventArgs e)
        {
            _potentialTargetGridForSelection = [];
            GridInfoPanel.Visibility = Visibility.Collapsed;
            ClearGridHighlights(); // 关闭时清除格子高亮
            UpdateGridHighlights();
            // 关闭格子信息面板时，如果CurrentCharacter不是PlayerCharacter，可以考虑将其设置回PlayerCharacter
            if (CurrentCharacter != PlayerCharacter)
            {
                CurrentCharacter = PlayerCharacter;
            }
            // 新增：关闭格子信息面板时，重置装备/状态/技能/物品描述和高亮
            ResetDescriptionAndHighlights();
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

        private void EquipSlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder)
            {
                ClearEquipSlotHighlights();
                ClearStatusIconHighlights();
                ClearSkillIconHighlights();
                ClearItemIconHighlights();

                clickedBorder.BorderBrush = Brushes.Blue;
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
                    SetRichTextBoxText(DescriptionRichTextBox, item.ToString());
                    _ = AppendDebugLog($"查看装备: {item.Name}");
                }
                else
                {
                    SetRichTextBoxText(DescriptionRichTextBox, "此槽位未装备物品。");
                    _ = AppendDebugLog("查看空装备槽位。");
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
                ClearSkillIconHighlights(); // 清除技能图标高亮
                ClearItemIconHighlights();  // 清除物品图标高亮

                clickedBorder.BorderBrush = Brushes.Blue; // 高亮当前点击的状态图标
                clickedBorder.BorderThickness = new Thickness(1.5);

                SetRichTextBoxText(DescriptionRichTextBox, effect.ToString());
                _ = AppendDebugLog($"查看状态: {effect.Name}");
            }
        }

        /// <summary>
        /// 清除所有状态图标的边框高亮。
        /// </summary>
        private void ClearStatusIconHighlights()
        {
            foreach (Border border in CharacterEffectsPanel.Children.OfType<Border>())
            {
                // 获取默认样式，以便恢复默认边框颜色和粗细
                Style defaultStyle = (Style)this.FindResource("StatusIconStyle");
                border.BorderBrush = (Brush)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderBrushProperty)?.Value ?? Brushes.Gray);
                border.BorderThickness = (Thickness)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderThicknessProperty)?.Value ?? new Thickness(0.5));
            }
        }

        // --- 新增：技能图标点击事件和辅助方法 ---
        private void SkillIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is ISkill skill)
            {
                ClearSkillIconHighlights();
                ClearItemIconHighlights();
                ClearEquipSlotHighlights();
                ClearStatusIconHighlights();

                _highlightedSkillIcon = clickedBorder;
                _highlightedSkillIcon.BorderBrush = Brushes.Blue;
                _highlightedSkillIcon.BorderThickness = new Thickness(1.5);

                SetRichTextBoxText(DescriptionRichTextBox, skill.ToString() ?? "");
                _ = AppendDebugLog($"查看技能: {skill.Name}");
            }
        }

        /// <summary>
        /// 清除所有技能图标的边框高亮。
        /// </summary>
        private void ClearSkillIconHighlights()
        {
            if (_highlightedSkillIcon != null)
            {
                Style defaultStyle = (Style)this.FindResource("StatusIconStyle");
                _highlightedSkillIcon.BorderBrush = (Brush)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderBrushProperty)?.Value ?? Brushes.Gray);
                _highlightedSkillIcon.BorderThickness = (Thickness)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderThicknessProperty)?.Value ?? new Thickness(0.5));
                _highlightedSkillIcon = null;
            }
            // 遍历 ItemsControl 中的所有生成的容器，确保所有图标都恢复默认样式
            if (CharacterSkillsPanel.ItemsSource != null)
            {
                foreach (var item in CharacterSkillsPanel.ItemsSource)
                {
                    if (CharacterSkillsPanel.ItemContainerGenerator.ContainerFromItem(item) is Border border)
                    {
                        Style defaultStyle = (Style)this.FindResource("StatusIconStyle");
                        border.BorderBrush = (Brush)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderBrushProperty)?.Value ?? Brushes.Gray);
                        border.BorderThickness = (Thickness)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderThicknessProperty)?.Value ?? new Thickness(0.5));
                    }
                }
            }
        }

        // --- 新增：物品图标点击事件和辅助方法 ---
        private void ItemIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is Item item)
            {
                ClearSkillIconHighlights();
                ClearItemIconHighlights();
                ClearEquipSlotHighlights();
                ClearStatusIconHighlights();

                _highlightedItemIcon = clickedBorder;
                _highlightedItemIcon.BorderBrush = Brushes.Blue;
                _highlightedItemIcon.BorderThickness = new Thickness(1.5);

                SetRichTextBoxText(DescriptionRichTextBox, item.ToString() ?? "");
                _ = AppendDebugLog($"查看物品: {item.Name}");
            }
        }

        /// <summary>
        /// 清除所有物品图标的边框高亮。
        /// </summary>
        private void ClearItemIconHighlights()
        {
            if (_highlightedItemIcon != null)
            {
                Style defaultStyle = (Style)this.FindResource("StatusIconStyle");
                _highlightedItemIcon.BorderBrush = (Brush)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderBrushProperty)?.Value ?? Brushes.Gray);
                _highlightedItemIcon.BorderThickness = (Thickness)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderThicknessProperty)?.Value ?? new Thickness(0.5));
                _highlightedItemIcon = null;
            }
            // 遍历 ItemsControl 中的所有生成的容器，确保所有图标都恢复默认样式
            if (CharacterItemsPanel.ItemsSource != null)
            {
                foreach (var item in CharacterItemsPanel.ItemsSource)
                {
                    if (CharacterItemsPanel.ItemContainerGenerator.ContainerFromItem(item) is Border border)
                    {
                        Style defaultStyle = (Style)this.FindResource("StatusIconStyle");
                        border.BorderBrush = (Brush)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderBrushProperty)?.Value ?? Brushes.Gray);
                        border.BorderThickness = (Thickness)(defaultStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Border.BorderThicknessProperty)?.Value ?? new Thickness(0.5));
                    }
                }
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

        /// <summary>
        /// 预释放爆发技的特殊处理。
        /// </summary>
        private async void PreCastSkillButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerCharacter != null)
            {
                Skill? skill = PlayerCharacter.Skills.FirstOrDefault(s => s.IsSuperSkill && s.Enable && s.CurrentCD == 0 && s.RealEPCost <= PlayerCharacter.EP);
                if (skill != null)
                {
                    _controller.SetPreCastSuperSkill(PlayerCharacter, skill);
                }
                else
                {
                    await AppendDebugLog("当前无法预释放爆发技，因为找不到可用的爆发技。");
                }
            }
            else
            {
                await AppendDebugLog("找不到角色。");
            }
        }

        /// <summary>
        /// 自动模式（托管）的特殊处理。
        /// </summary>
        private async void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerCharacter != null)
            {
                _isAutoMode = !_isAutoMode;
                AutoModeButton.Content = _isAutoMode ? "自动模式：开" : "自动模式：关";
                _controller.SetAutoMode(!_isAutoMode, PlayerCharacter);
                await AppendDebugLog($"已切换到{(_isAutoMode ? "自动模式" : "手动模式")}。");
                if (_resolveActionType != null)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "是否跳过当前回合？AI将代替你操作。",
                        "自动模式跳过当前回合确认",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );
                    if (result == MessageBoxResult.Yes)
                    {
                        _resolveActionType.Invoke(GamingQueue.GetActionType(DP, 0.33, 0.33, 0.34));
                    }
                    else
                    {
                        await AppendDebugLog("自动模式将在下一回合开始接管你的回合。");
                    }
                }
            }
            else
            {
                await AppendDebugLog("找不到角色。");
            }
        }

        /// <summary>
        /// 快速模式（仅看我的回合）的特殊处理。
        /// </summary>
        private async void FastModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerCharacter != null)
            {
                _isFastMode = !_isFastMode;
                FastModeButton.Content = _isFastMode ? "快速模式：开" : "快速模式：关";
                _controller.SetFastMode(_isFastMode);
                await AppendDebugLog($"已切换到{(_isFastMode ? "快速模式" : "正常模式")}。");
            }
            else
            {
                await AppendDebugLog("找不到角色。");
            }
        }

        // --- UI 提示方法 (由 GameMapController 调用) ---

        /// <summary>
        /// 显示角色选择提示面板。
        /// </summary>
        /// <param name="availableCharacters">可供选择的角色列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowCharacterSelectionPrompt(List<Character> availableCharacters, Action<Character?> callback)
        {
            _resolveCharacterSelection = callback;
            CharacterSelectionItemsControl.ItemsSource = availableCharacters;
            SetRichTextBoxText(CharacterDetailsRichTextBox, "将鼠标悬停在角色名称上以查看详情。");
            CharacterSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 显示反应选择面板。
        /// </summary>
        /// <param name="options">询问内容。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowInquiryResponseSelectionPrompt(InquiryOptions options, Action<InquiryResponse?> callback)
        {
            switch (options.InquiryType)
            {
                case InquiryType.Choice:
                case InquiryType.BinaryChoice:
                    _resolveInquiryResponseSelection = callback;
                    InquiryResponseSelectionTitle.Text = options.Description;
                    List<InquiryResponse> responses = [];
                    foreach (string key in options.Choices.Keys)
                    {
                        responses.Add(new(options.InquiryType, options.Topic)
                        {
                            Choices = [key],
                            TextResult = options.Choices[key]
                        });
                    }
                    InquiryResponseSelectionItemsControl.ItemsSource = responses;
                    SetRichTextBoxText(InquiryResponseDetailsRichTextBox, "将鼠标悬停在选项上以查看详情。");
                    InquiryResponseSelectionOverlay.Visibility = Visibility.Visible;
                    InquiryResponseCancel.Visibility = Visibility.Hidden;
                    break;
            }
        }

        /// <summary>
        /// 角色选择项的点击事件。
        /// </summary>
        private void CharacterSelectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Character character)
            {
                _resolveCharacterSelection?.Invoke(character);
            }
        }

        /// <summary>
        /// 反应选择项的点击事件。
        /// </summary>
        private void InquiryResponseSelectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is InquiryResponse response)
            {
                _resolveInquiryResponseSelection?.Invoke(response);
            }
        }

        /// <summary>
        /// 角色选择项的鼠标进入事件。
        /// </summary>
        private void CharacterSelectionItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Tag 现在是 Character 对象
            if (sender is Border border && border.Tag is Character hoveredCharacter)
            {
                string details = hoveredCharacter.GetInfo(showMapRelated: true);
                SetRichTextBoxText(CharacterDetailsRichTextBox, details);
            }
        }

        /// <summary>
        /// 反应选择项的鼠标进入事件。
        /// </summary>
        private void InquiryResponseSelectionItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is InquiryResponse response)
            {
                string details = response.TextResult;
                SetRichTextBoxText(InquiryResponseDetailsRichTextBox, details);
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
            SetRichTextBoxText(CharacterDetailsRichTextBox, "");
        }

        /// <summary>
        /// 隐藏反应选择提示面板。
        /// </summary>
        public void HideInquiryResponseSelectionPrompt()
        {
            InquiryResponseSelectionOverlay.Visibility = Visibility.Collapsed;
            InquiryResponseSelectionItemsControl.ItemsSource = null;
            _resolveInquiryResponseSelection = null;
            SetRichTextBoxText(InquiryResponseDetailsRichTextBox, "");
        }

        /// <summary>
        /// 取消反应选择的点击事件。
        /// </summary>
        private void CancelInquiryResponseSelection_Click(object sender, RoutedEventArgs e)
        {
            _resolveInquiryResponseSelection?.Invoke(null);
        }

        /// <summary>
        /// 显示行动按钮，并根据可用技能和物品启用/禁用相关按钮。
        /// </summary>
        /// <param name="character">当前行动的角色。</param>
        /// <param name="availableItems">可用的物品列表。</param>
        /// <param name="callback">选择行动后调用的回调函数。</param>
        public void ShowActionButtons(Character character, List<Item> availableItems, Action<CharacterActionType> callback)
        {
            _resolveActionType = callback;
            SetActionButtonsEnabled(true);

            MoveButton.IsEnabled = character.CharacterState != CharacterState.NotActionable && character.CharacterState != CharacterState.ActionRestricted;
            AttackButton.IsEnabled = character.CharacterState != CharacterState.NotActionable && character.CharacterState != CharacterState.ActionRestricted &&
                        character.CharacterState != CharacterState.BattleRestricted && character.CharacterState != CharacterState.AttackRestricted;
            SkillButton.IsEnabled = true;
            UseItemButton.IsEnabled = availableItems.Count != 0;

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
        /// <param name="character">可供选择的技能列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowSkillSelectionUI(Character character, Action<Skill?> callback)
        {
            _resolveSkillSelection = callback;
            SkillItemSelectionTitle.Text = "请选择技能";
            SkillItemDescription.Text = "技能详情";
            SetRichTextBoxText(SkillItemDetailsRichTextBox, "将鼠标悬停在名称上以查看详情。");
            SkillItemSelectionItemsControl.ItemsSource = character.Skills.Where(s => s.SkillType != SkillType.Passive).ToList();
            SkillItemSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 显示物品选择UI。
        /// </summary>
        /// <param name="character">可供选择的物品列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowItemSelectionUI(Character character, Action<Item?> callback)
        {
            _resolveItemSelection = callback;
            SkillItemSelectionTitle.Text = "请选择物品";
            SkillItemDescription.Text = "物品详情";
            SetRichTextBoxText(SkillItemDetailsRichTextBox, "将鼠标悬停在名称上以查看详情。");
            SkillItemSelectionItemsControl.ItemsSource = character.Items;
            SkillItemSelectionOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 技能/物品选择项的点击事件。
        /// </summary>
        private void SkillItemSelectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                if (border.Tag is Skill skill)
                {
                    _resolveSkillSelection?.Invoke(skill);
                }
                else if (border.Tag is Item item)
                {
                    _resolveItemSelection?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// 角色选择项的鼠标进入事件。
        /// </summary>
        private void SkillItemSelectionItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                string details = "";
                double hardnessTime = 0;
                string name = "";
                if (border.Tag is Skill hoveredSkill)
                {
                    details = hoveredSkill.ToString();
                    if (!hoveredSkill.IsMagic) hardnessTime = hoveredSkill.RealHardnessTime;
                    else hardnessTime = hoveredSkill.RealCastTime;
                    name = hoveredSkill.Character?.NickName ?? "未知角色";
                }
                else if (border.Tag is Item hoveredItem)
                {
                    details = hoveredItem.ToString();
                    if (hoveredItem.Skills.Active != null)
                    {
                        hardnessTime = hoveredItem.Skills.Active.RealHardnessTime;
                    }
                    if (hardnessTime == 0)
                    {
                        hardnessTime = 5;
                    }
                    name = hoveredItem.Character?.NickName ?? "未知角色";
                }
                SetRichTextBoxText(SkillItemDetailsRichTextBox, details);
                SetPredictCharacter(name, hardnessTime);
            }
        }

        /// <summary>
        /// 取消技能/物品选择的点击事件。
        /// </summary>
        private void CancelSkillItemSelection_Click(object sender, RoutedEventArgs e)
        {
            if (SkillItemDescription.Text == "技能详情")
            {
                SkillItemDescription.Text = "";
                _resolveSkillSelection?.Invoke(null);
            }
            else if (SkillItemDescription.Text == "物品详情")
            {
                SkillItemDescription.Text = "";
                _resolveItemSelection?.Invoke(null);
            }
            CharacterQueueItems.Remove(_selectionPredictCharacter);
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
        /// <param name="character">发起行动的角色。</param>
        /// <param name="skill">请求选择目标的技能/普攻。</param>
        /// <param name="selectable">所有可选择的目标列表。</param>
        /// <param name="enemys">所有可选敌方目标列表。</param>
        /// <param name="teammates">所有可选友方目标列表。</param>
        /// <param name="range">所有可选友方目标列表。</param>
        /// <param name="callback">选择完成后调用的回调函数。</param>
        public void ShowTargetSelectionUI(Character character, ISkill skill, List<Character> selectable, List<Character> enemys, List<Character> teammates, List<Grid> range, Action<List<Character>> callback)
        {
            _resolveTargetSelection = callback;
            _actingCharacterForTargetSelection = character;
            _potentialTargetsForSelection = selectable;
            _potentialTargetGridForSelection = range;
            _maxTargetsForSelection = skill.RealCanSelectTargetCount(enemys, teammates);
            _canSelectAllTeammates = skill.SelectAllTeammates;
            _canSelectAllEnemies = skill.SelectAllEnemies;
            _canSelectSelf = skill.CanSelectSelf;
            _canSelectEnemy = skill.CanSelectEnemy;
            _canSelectTeammate = skill.CanSelectTeammate;
            _isSelectingTargets = true; // 进入目标选择模式

            SelectedTargets.Clear(); // 清空之前的选择
            TargetSelectionTitle.Text = $"选择 {skill.Name} 的目标 (最多 {_maxTargetsForSelection} 个)";
            TargetSelectionOverlay.Visibility = Visibility.Visible;
            if (_canSelectAllTeammates)
            {
                SelectedTargets.Add(character);
                foreach (Character teammate in teammates)
                {
                    SelectedTargets.Add(teammate);
                }
            }
            else if (_canSelectAllEnemies)
            {
                foreach (Character enemy in enemys)
                {
                    SelectedTargets.Add(enemy);
                }
            }
            else if (_canSelectSelf && !_canSelectEnemy && !_canSelectTeammate)
            {
                SelectedTargets.Add(character);
            }

            if (!CharacterQueueItems.Contains(_selectionPredictCharacter))
            {
                SetPredictCharacter(character.NickName, skill.RealHardnessTime);
            }

            // 更新地图上角色的高亮，以显示潜在目标和已选目标
            UpdateCharacterHighlights();
            // 更新地图上格子的高亮
            UpdateGridHighlights();
        }

        /// <summary>
        /// 显示移动目标选择UI。
        /// </summary>
        /// <param name="character"></param>
        /// <param name="currentGrid"></param>
        /// <param name="selectable"></param>
        /// <param name="callback"></param>
        public void ShowTargetGridSelectionUI(Character character, Grid currentGrid, List<Grid> selectable, Action<Grid?> callback)
        {
            _resolveTargetGridSelection = callback;
            _actingCharacterForTargetSelection = character;
            _actingCharacterCurrentGridForTargetSelection = currentGrid;
            _potentialTargetGridForSelection = selectable;
            _isSelectingTargetGrid = true;

            SelectedTargetGrid.Clear(); // 清空之前的选择
            TargetGridSelectionOverlay.Visibility = Visibility.Visible;

            // 更新地图上格子的高亮，以显示潜在目标和已选目标
            UpdateGridHighlights();
        }

        /// <summary>
        /// 显示非指向性目标选择UI。
        /// </summary>
        /// <param name="character"></param>
        /// <param name="skill"></param>
        /// <param name="currentGrid"></param>
        /// <param name="selectable"></param>
        /// <param name="callback"></param>
        public void ShowTargetGridsSelectionUI(Character character, Skill skill, List<Character> enemys, List<Character> teammates, Grid currentGrid, List<Grid> selectable, Action<List<Grid>> callback)
        {
            _resolveTargetGridsSelection = callback;
            _actingCharacterForTargetSelection = character;
            _actingCharacterCurrentGridForTargetSelection = currentGrid;
            _potentialTargetGridForSelection = selectable;
            _isSelectingTargetGrid = true;
            _isSelectingNonDirectionalGrids = true;
            _currentNonDirectionalSkill = skill;

            SelectedTargetGrids.Clear(); // 清空之前的选择
            TargetGridsSelectionOverlay.Visibility = Visibility.Visible;

            // 更新标题，显示技能范围信息
            string rangeType = GetSkillRangeTypeDisplayName(skill.SkillRangeType);
            string rangeInfo = skill.CanSelectTargetRange == 0 ? "单格" : $"{rangeType} {skill.CanSelectTargetRange}格";
            TargetGridsSelectionTitle.Text = $"选择 {skill.Name} 的目标区域 ({rangeInfo})";

            // 为Canvas添加鼠标移动事件
            GameMapCanvas.MouseMove += GameMapCanvas_MouseMoveForNonDirectional;
            GameMapCanvas.MouseLeave += GameMapCanvas_MouseLeaveForNonDirectional;

            // 更新地图上格子的高亮，以显示潜在目标和已选目标
            UpdateGridHighlights();
        }

        /// <summary>
        /// 非指向性技能选择时的鼠标移动事件
        /// </summary>
        private void GameMapCanvas_MouseMoveForNonDirectional(object sender, MouseEventArgs e)
        {
            if (!_isSelectingNonDirectionalGrids || _currentNonDirectionalSkill == null)
                return;

            // 获取鼠标位置对应的格子
            Point mousePos = e.GetPosition(GameMapCanvas);
            Grid? hoveredGrid = GetGridAtPosition(mousePos);

            if (hoveredGrid == null || hoveredGrid == _hoveredGrid)
                return;

            _hoveredGrid = hoveredGrid;

            // 清除之前的鼠标悬停高亮
            ClearMouseHoverHighlights();

            // 如果鼠标悬停的格子在可选范围内
            if (_potentialTargetGridForSelection.Contains(hoveredGrid))
            {
                // 计算技能影响范围
                _mouseHoverHighlightedGrids = _currentNonDirectionalSkill.SelectNonDirectionalTargets(
                    _actingCharacterForTargetSelection!, hoveredGrid, _currentNonDirectionalSkill.SelectIncludeCharacterGrid);

                // 应用悬停高亮
                foreach (Grid grid in _mouseHoverHighlightedGrids)
                {
                    if (_gridIdToUiElement.TryGetValue(grid.Id, out Rectangle? rect))
                    {
                        // 如果这个格子已经被选中，跳过（保持选中高亮）
                        if (!_currentNonDirectionalSkill.SelectIncludeCharacterGrid && grid.Characters.Count > 0)
                        {
                            rect.Fill = Brushes.DarkGray;
                        }
                        else if (!SelectedTargetGrids.Contains(grid))
                        {
                            rect.Fill = Brushes.LightBlue; // 悬停高亮颜色
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标离开Canvas时清除悬停高亮
        /// </summary>
        private void GameMapCanvas_MouseLeaveForNonDirectional(object sender, MouseEventArgs e)
        {
            ClearMouseHoverHighlights();
            _hoveredGrid = null;
        }

        /// <summary>
        /// 根据位置获取格子
        /// </summary>
        private Grid? GetGridAtPosition(Point position)
        {
            if (CurrentGameMap == null)
                return null;

            int gridX = (int)(position.X / CurrentGameMap.Size);
            int gridY = (int)(position.Y / CurrentGameMap.Size);

            // 查找对应的格子
            foreach (Grid grid in _uiElementToGrid.Values)
            {
                if (grid.X == gridX && grid.Y == gridY && grid.Z == 0)
                    return grid;
            }

            return null;
        }

        /// <summary>
        /// 清除鼠标悬停高亮
        /// </summary>
        private void ClearMouseHoverHighlights()
        {
            foreach (Grid grid in _mouseHoverHighlightedGrids)
            {
                if (_gridIdToUiElement.TryGetValue(grid.Id, out Rectangle? rect))
                {
                    if (SelectedTargetGrid.Contains(grid))
                    {
                        rect.Fill = Brushes.Red;
                    }
                    else if (SelectedTargetGrids.Contains(grid))
                    {
                        rect.Fill = Brushes.Red;
                    }
                    else if (!(_currentNonDirectionalSkill?.SelectIncludeCharacterGrid ?? true) && grid.Characters.Count > 0)
                    {
                        rect.Fill = Brushes.DarkGray;
                    }
                    else if (_potentialTargetGridForSelection.Contains(grid))
                    {
                        rect.Fill = Brushes.Yellow;
                    }
                    else
                    {
                        rect.Fill = DefaultGridBrush;
                    }
                }
            }
            _mouseHoverHighlightedGrids.Clear();
        }

        /// <summary>
        /// 获取技能范围类型的显示名称
        /// </summary>
        private static string GetSkillRangeTypeDisplayName(SkillRangeType rangeType)
        {
            return rangeType switch
            {
                SkillRangeType.Diamond => "菱形",
                SkillRangeType.Circle => "圆形",
                SkillRangeType.Square => "方形",
                SkillRangeType.Line => "直线",
                SkillRangeType.LinePass => "穿透直线",
                SkillRangeType.Sector => "扇形",
                _ => "菱形"
            };
        }

        /// <summary>
        /// 处理非指向性技能格子选择的点击逻辑
        /// </summary>
        private async Task HandleNonDirectionalGridSelectionClick(Grid clickedGrid)
        {
            if (_currentNonDirectionalSkill == null)
                return;

            // 检查是否是潜在目标
            if (!_potentialTargetGridForSelection.Contains(clickedGrid))
            {
                await AppendDebugLog($"无法选择 {clickedGrid}：不在技能施放范围内。");
                return;
            }


            if (!_currentNonDirectionalSkill.SelectIncludeCharacterGrid && clickedGrid.Characters.Count > 0)
            {
                await AppendDebugLog($"无法选择 {clickedGrid}：此技能不能选择有角色的格子。");
                return;
            }

            // 清除之前的选择
            SelectedTargetGrids.Clear();
            ClearMouseHoverHighlights();

            // 计算技能影响范围
            List<Grid> affectedGrids = _currentNonDirectionalSkill.SelectNonDirectionalTargets(
                _actingCharacterForTargetSelection!, clickedGrid, _currentNonDirectionalSkill.SelectIncludeCharacterGrid);

            // 添加新选择
            affectedGrids.ForEach(ag => SelectedTargetGrids.Add(ag));

            // 移除可能存在的重复项
            SelectedTargetGrids = [.. SelectedTargetGrids.Distinct()];

            await AppendDebugLog($"选择了技能中心点: 坐标({clickedGrid.X}, {clickedGrid.Y})，影响{SelectedTargetGrids.Count}个格子");

            // 更新地图上的高亮显示
            UpdateGridHighlights();

            // 清除鼠标悬停高亮
            _mouseHoverHighlightedGrids.Clear();
            _hoveredGrid = null;
        }

        /// <summary>
        /// 处理在目标选择模式下点击角色图标的逻辑。
        /// </summary>
        /// <param name="clickedCharacter">被点击的角色。</param>
        private async Task HandleTargetSelectionClick(Character clickedCharacter)
        {
            // 检查是否是潜在目标
            if (_potentialTargetsForSelection == null || !_potentialTargetsForSelection.Contains(clickedCharacter))
            {
                await AppendDebugLog($"无法选择 {clickedCharacter.NickName}：不是潜在目标。");
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
                await AppendDebugLog($"无法选择 {clickedCharacter.NickName}：不符合目标选择规则。");
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
                    await AppendDebugLog($"已达到最大目标数量 ({_maxTargetsForSelection})。");
                }
            }
            UpdateCharacterHighlights(); // 更新地图上的高亮显示
        }

        /// <summary>
        /// 处理在移动目标选择模式下点击角色图标的逻辑。
        /// </summary>
        /// <param name="clickedGrid">被点击的格子。</param>
        private async Task HandleTargetGridSelectionClick(Grid clickedGrid)
        {
            // 检查是否是潜在目标
            if (_potentialTargetGridForSelection == null || !_potentialTargetGridForSelection.Contains(clickedGrid))
            {
                await AppendDebugLog($"无法选择 {clickedGrid}：不是潜在目标。");
                return;
            }

            if (!SelectedTargetGrid.Remove(clickedGrid))
            {
                if (SelectedTargetGrid.Count < 1)
                {
                    SelectedTargetGrid.Add(clickedGrid);
                }
                else
                {
                    await AppendDebugLog($"已选择过目标。");
                }
            }
            UpdateGridHighlights(); // 更新地图上的高亮显示
        }

        /// <summary>
        /// 更新地图上角色的高亮显示，区分潜在目标和已选目标。
        /// </summary>
        private void UpdateCharacterHighlights()
        {
            foreach (Character character in _characterToUiElement.Keys)
            {
                Border border = _characterToUiElement[character];

                // 恢复默认
                border.BorderBrush = character.Promotion switch
                {
                    200 => Brushes.BurlyWood,
                    300 => Brushes.SkyBlue,
                    400 => Brushes.Orchid,
                    _ => Brushes.Salmon
                };
                border.BorderThickness = new Thickness(2);

                if (SelectedTargets.Contains(character))
                {
                    // 高亮当前已选目标（红色边框）
                    border.BorderBrush = Brushes.Red; // 已选目标颜色
                    border.BorderThickness = new Thickness(6);
                }
                else if (_potentialTargetsForSelection.Contains(character))
                {
                    // 高亮潜在目标（黄色边框）
                    border.BorderBrush = Brushes.Yellow;
                    border.BorderThickness = new Thickness(4);
                }
            }
        }

        /// <summary>
        /// 更新地图上格子的高亮显示，区分潜在目标和已选目标。
        /// </summary>
        private void UpdateGridHighlights()
        {
            foreach (Rectangle rectangle in _uiElementToGrid.Keys)
            {
                Grid grid = _uiElementToGrid[rectangle];

                if (SelectedTargetGrid.Contains(grid))
                {
                    rectangle.Fill = Brushes.Red;
                }
                else if (SelectedTargetGrids.Contains(grid))
                {
                    rectangle.Fill = Brushes.Red; // 已选目标颜色
                }
                else if (_mouseHoverHighlightedGrids.Contains(grid))
                {
                    rectangle.Fill = Brushes.LightBlue; // 鼠标悬停高亮
                }
                else if (!(_currentNonDirectionalSkill?.SelectIncludeCharacterGrid ?? true) && grid.Characters.Count > 0)
                {
                    rectangle.Fill = Brushes.DarkGray;
                }
                else if (_potentialTargetGridForSelection.Contains(grid))
                {
                    rectangle.Fill = Brushes.Yellow;
                }
                else
                {
                    rectangle.Fill = DefaultGridBrush;
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
        /// 从已选目标列表中移除一个格子。
        /// </summary>
        private void RemoveTargetGrid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is long gid)
            {
                Grid? gridToRemove = SelectedTargetGrid.FirstOrDefault(g => g.Id == gid);
                if (gridToRemove != null)
                {
                    SelectedTargetGrid.Remove(gridToRemove);
                    UpdateGridHighlights();
                }
            }
        }

        /// <summary>
        /// 移除所有已选的格子。
        /// </summary>
        private void RemoveTargetGrids_Click(object sender, RoutedEventArgs e)
        {
            SelectedTargetGrids.Clear();
            UpdateGridHighlights();
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
            CharacterQueueItems.Remove(_selectionPredictCharacter);
        }

        /// <summary>
        /// 确认目标格子选择的点击事件。
        /// </summary>
        private void ConfirmTargetGrid_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetGridSelection?.Invoke(SelectedTargetGrid.FirstOrDefault());
        }

        /// <summary>
        /// 取消目标格子选择的点击事件。
        /// </summary>
        private void CancelTargetGridSelection_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetGridSelection?.Invoke(null); // 返回空表示取消
        }

        /// <summary>
        /// 确认非指向性目标格子选择的点击事件。
        /// </summary>
        private void ConfirmTargetGrids_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetGridsSelection?.Invoke([.. SelectedTargetGrids]);
        }

        /// <summary>
        /// 取消非指向性目标格子选择的点击事件。
        /// </summary>
        private void CancelTargetGridsSelection_Click(object sender, RoutedEventArgs e)
        {
            _resolveTargetGridsSelection?.Invoke([]); // 返回空表示取消
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
            _potentialTargetGridForSelection = [];
            UpdateCharacterHighlights(); // 清除所有高亮
            UpdateGridHighlights();
        }

        /// <summary>
        /// 隐藏移动目标选择UI。
        /// </summary>
        public void HideTargetGridSelectionUI()
        {
            TargetGridSelectionOverlay.Visibility = Visibility.Collapsed;
            SelectedTargetGrid.Clear();
            _isSelectingTargetGrid = false;
            _actingCharacterCurrentGridForTargetSelection = null;
            _potentialTargetGridForSelection = [];
            _resolveTargetGridSelection = null;
            UpdateGridHighlights();
            CloseGridInfoButton_Click(new(), new());
        }

        /// <summary>
        /// 隐藏非指向性目标选择UI。
        /// </summary>
        public void HideTargetGridsSelectionUI()
        {
            TargetGridsSelectionOverlay.Visibility = Visibility.Collapsed;
            SelectedTargetGrids.Clear();
            _isSelectingTargetGrid = false;
            _isSelectingNonDirectionalGrids = false;
            _currentNonDirectionalSkill = null;
            _actingCharacterCurrentGridForTargetSelection = null;
            _potentialTargetGridForSelection = [];
            _resolveTargetGridsSelection = null;
            // 移除鼠标事件
            GameMapCanvas.MouseMove -= GameMapCanvas_MouseMoveForNonDirectional;
            GameMapCanvas.MouseLeave -= GameMapCanvas_MouseLeaveForNonDirectional;
            // 清除所有高亮
            ClearMouseHoverHighlights();
            UpdateGridHighlights();
            CloseGridInfoButton_Click(new(), new());
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

        private async Task StartCountdownTimer()
        {
            while (_remainingCountdownSeconds > 0)
            {
                // 更新倒计时文本
                CountdownTextBlock.Text = $"{_remainingCountdownSeconds} 秒后继续...";
                await Task.Delay(1000);
                _remainingCountdownSeconds--;
            }
            // 倒计时结束
            CountdownTextBlock.Visibility = Visibility.Collapsed;

            // 触发继续回调
            _currentContinueCallback?.Invoke(true);
            _currentContinueCallback = null;
        }

        /// <summary>
        /// 启动倒计时，并在倒计时结束后自动触发继续。
        /// </summary>
        /// <param name="seconds">倒计时秒数。</param>
        /// <param name="callback">倒计时结束后调用的回调函数。</param>
        public async Task StartCountdownForContinue(int seconds, Action<bool> callback)
        {
            _remainingCountdownSeconds = seconds;
            _currentContinueCallback = callback;

            // 显示倒计时文本并设置初始值
            CountdownTextBlock.Text = $"{_remainingCountdownSeconds} 秒后继续...";
            CountdownTextBlock.Visibility = Visibility.Visible;

            // 启动计时器
            await InvokeAsync(StartCountdownTimer);
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

        private void EquipStatusInfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 只有当点击事件的原始源是Border本身时才处理，这意味着没有点击到任何子元素（如装备槽位或状态图标）
            if (sender is Border clickedBorder && e.OriginalSource == clickedBorder)
            {
                ResetDescriptionAndHighlights();
                _ = AppendDebugLog("点击了装备/状态区域空白处。");
                e.Handled = true; // 标记事件已处理
            }
        }

        /// <summary>
        /// 重置所有描述文本框并清除所有高亮。
        /// </summary>
        private void ResetDescriptionAndHighlights(bool clearHightlightsOnly = false)
        {
            if (!clearHightlightsOnly) SetRichTextBoxText(DescriptionRichTextBox, "点击装备、状态、技能或物品的图标以查看详情。");
            ClearEquipSlotHighlights();
            ClearStatusIconHighlights();
            ClearSkillIconHighlights();
            ClearItemIconHighlights();
        }

        /// <summary>
        /// 设置 RichTextBox 的纯文本内容。
        /// </summary>
        /// <param name="richTextBox">要设置内容的 RichTextBox。</param>
        /// <param name="text">要设置的纯文本。</param>
        private static void SetRichTextBoxText(RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(0) });
        }

        public static void InsertSorted<T>(ObservableCollection<T> collection, T item, Func<T, double> keySelector)
        {
            // 处理空集合情况
            if (collection.Count == 0)
            {
                collection.Add(item);
                return;
            }

            // 二分查找插入位置
            int low = 0;
            int high = collection.Count - 1;
            int index = 0;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                double midValue = keySelector(collection[mid]);
                double newValue = keySelector(item);

                if (Math.Abs(midValue - newValue) < double.Epsilon) // 处理浮点精度
                {
                    index = mid + 1; // 相同值插入后面
                    break;
                }
                else if (midValue < newValue)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }

                if (low > high) index = low;
            }

            collection.Insert(index, item);
        }

        private void UpdateCharacterQueueDisplayItems()
        {
            if (CharacterQueueItems == null)
            {
                CharacterQueueDisplayItems.Clear();
                return;
            }

            // 1. 创建一个新的 ViewModel 列表，同时尝试重用现有实例
            List<CharacterQueueItemViewModel> newDisplayItems = [];
            // 使用字典快速查找现有 ViewModel
            Dictionary<CharacterQueueItem, CharacterQueueItemViewModel> existingVmMap = CharacterQueueDisplayItems.ToDictionary(vm => vm.Model);

            for (int i = 0; i < CharacterQueueItems.Count; i++)
            {
                CharacterQueueItem rawItem = CharacterQueueItems[i];
                CharacterQueueItemViewModel vm;

                // 尝试从现有 ViewModel 映射中获取
                if (existingVmMap.TryGetValue(rawItem, out CharacterQueueItemViewModel? existingVm))
                {
                    vm = existingVm;
                }
                else
                {
                    // 如果没有，则创建新的 ViewModel
                    vm = new CharacterQueueItemViewModel(rawItem, () => TurnRewards);
                }

                // 2. 更新 ViewModel 的派生属性
                // 预测回合数会因为队列顺序或 CurrentRound 变化而变化
                int predictedTurn = CurrentRound + i;
                if (vm.PredictedTurnNumber != predictedTurn) // 只有当实际变化时才设置，触发 INPC
                {
                    vm.PredictedTurnNumber = predictedTurn;
                }
                // 奖励信息可能因为 PredictedTurnNumber 变化而变化 (即使 TurnRewards 字典本身不变)
                vm.UpdateRewardProperties(); // 会在内部检查 TurnRewardSkillName 是否变化并触发 INPC

                newDisplayItems.Add(vm);
            }

            // 3. 高效同步 CharacterQueueDisplayItems
            // 这是一个简单的同步策略：清空并重新添加。
            CharacterQueueDisplayItems.Clear();
            foreach (CharacterQueueItemViewModel vm in newDisplayItems)
            {
                CharacterQueueDisplayItems.Add(vm);
            }
        }

        public void SetPredictCharacter(string name, double ht)
        {
            CharacterQueueItems.Remove(_selectionPredictCharacter);
            _selectionPredictCharacter.Character.NickName = $"{name} [ 下轮预测 ]";
            _selectionPredictCharacter.ATDelay = ht;
            InsertSorted(CharacterQueueItems, _selectionPredictCharacter, cq => cq.ATDelay);
        }
    }
}

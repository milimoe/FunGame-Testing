using System.Windows;
using System.Windows.Controls;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Model;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace Milimoe.FunGame.Testing.Desktop.Solutions.NovelEditor
{
    /// <summary>
    /// NovelEditor.xaml 的交互逻辑
    /// </summary>
    public partial class NovelEditor : Window
    {
        public static Dictionary<Character, int> Likability { get; } = [];
        public static Dictionary<string, Func<bool>> Conditions { get; } = [];
        public static Character MainCharacter { get; set; } = Factory.GetCharacter();
        public static Character 马猴烧酒 { get; set; } = Factory.GetCharacter();

        private readonly NovelConfig _config;

        public NovelEditor()
        {
            InitializeComponent();

            MainCharacter.Name = "主角";
            MainCharacter.NickName = "主角";
            马猴烧酒.Name = "马猴烧酒";
            马猴烧酒.NickName = "魔法少女";
            Likability.Add(马猴烧酒, 100);

            Conditions.Add("马猴烧酒的好感度低于50", () => 好感度低于50(马猴烧酒));
            Conditions.Add("主角攻击力大于20", () => 攻击力大于20(MainCharacter));
            Conditions.Add("马猴烧酒攻击力大于20", () => 攻击力大于20(马猴烧酒));

            // 如果需要，初始化小说
            //NovelTest.CreateNovels();

            // 小说配置
            _config = new NovelConfig("novel1", "chapter1");
            LoadNovelData();

            // 绑定节点列表
            NodeListBox.ItemsSource = _config.Values;
            if (_config.Count > 0)
            {
                NodeListBox.SelectedIndex = 0;
            }
        }

        private void LoadNovelData()
        {
            // 加载已有的小说数据
            _config.LoadConfig(Conditions);
        }

        private void NodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeListBox.SelectedItem is NovelNode selectedNode)
            {
                // 显示节点详情
                NodeKeyText.Text = "编号：" + selectedNode.Key;
                NodeNameText.Text = "角色：" + selectedNode.Name;
                NodeContentText.Text = "文本：" + selectedNode.Content;

                // 显示节点的显示条件
                NodeConditionsText.Text = FormatConditions(selectedNode);

                // 显示选项
                OptionsListBox.ItemsSource = selectedNode.Options.Select(option => new
                {
                    option.Name,
                    option.Targets,
                    Conditions = FormatOptionConditions(option)
                }).ToList();
            }
        }
        private void PreviousSentence_Click(object sender, RoutedEventArgs e)
        {
            if (NodeListBox.SelectedItem is NovelNode selectedNode)
            {
                if (selectedNode.Previous is null)
                {
                    if (NodeListBox.SelectedIndex - 1 >= 0)
                    {
                        MessageBox.Show("此节点没有定义上一个节点！现在跳转列表的上一个节点。");
                        NodeListBox.SelectedIndex--;
                    }
                    else
                    {
                        MessageBox.Show("此节点没有定义上一个节点，并且已经达到列表顶部！");
                    }
                }
                else
                {
                    NodeListBox.SelectedItem = selectedNode.Previous;
                }
            }
        }

        private void NextSentence_Click(object sender, RoutedEventArgs e)
        {
            if (NodeListBox.SelectedItem is NovelNode selectedNode)
            {
                NovelNode? next = selectedNode.Next;
                if (next is null)
                {
                    if (NodeListBox.SelectedIndex + 1 < NodeListBox.Items.Count)
                    {
                        MessageBox.Show("此节点没有定义下一个节点！现在跳转列表的下一个节点。");
                        NodeListBox.SelectedIndex++;
                    }
                    else
                    {
                        MessageBox.Show("此节点没有定义下一个节点，并且已经到达列表底部！现在跳转到列表的起始处。");
                        NodeListBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    NodeListBox.SelectedItem = next;
                }
            }
        }

        private static string FormatConditions(NovelNode node)
        {
            List<string> conditions = [];

            if (node.AndPredicates.Count != 0)
            {
                conditions.Add("需满足以下所有条件：");
                foreach (string condition in node.AndPredicates.Keys)
                {
                    conditions.Add($"- {condition}");
                }
            }

            if (node.OrPredicates.Count != 0)
            {
                conditions.Add("需满足以下任意一个条件：");
                foreach (string condition in node.OrPredicates.Keys)
                {
                    conditions.Add($"- {condition}");
                }
            }

            return conditions.Count != 0 ? string.Join(Environment.NewLine, conditions) : "无显示条件";
        }

        private static string FormatOptionConditions(NovelOption option)
        {
            List<string> conditions = [];

            if (option.AndPredicates.Count != 0)
            {
                conditions.Add("需满足以下所有条件：");
                foreach (string condition in option.AndPredicates.Keys)
                {
                    conditions.Add($"- {condition}");
                }
            }

            if (option.OrPredicates.Count != 0)
            {
                conditions.Add("需满足以下任意一个条件：");
                foreach (string condition in option.OrPredicates.Keys)
                {
                    conditions.Add($"- {condition}");
                }
            }

            return conditions.Count != 0 ? string.Join(Environment.NewLine, conditions) : "";
        }

        private void AddNodeButton_Click(object sender, RoutedEventArgs e)
        {
            NovelNode newNode = new()
            {
                Key = "示例节点编号",
                Name = "示例发言人",
                Content = "示例发言内容"
            };

            _config.Add(newNode.Key, newNode);
            NodeListBox.Items.Refresh();
        }

        private void EditNodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (NodeListBox.SelectedItem is NovelNode selectedNode)
            {
                selectedNode.Content = "此示例节点已被编辑";
                NodeContentText.Text = "文本：" + selectedNode.Content;
            }
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is NovelNode node && NodeListBox.Items.OfType<NovelNode>().FirstOrDefault(n => n.Key == node.Key) is NovelNode target)
            {
                NodeListBox.SelectedItem = target;
                NodeListBox.ScrollIntoView(target);
            }
        }

        public static bool 攻击力大于20(Character character)
        {
            return character.ATK > 20;
        }

        public static bool 好感度低于50(Character character)
        {
            return Likability[character] > 50;
        }
    }
}

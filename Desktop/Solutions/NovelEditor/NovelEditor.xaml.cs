using System.Windows;
using System.Windows.Controls;
using Milimoe.FunGame.Core.Api.Utility;
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
        private NovelConfig _config;

        public NovelEditor()
        {
            InitializeComponent();

            // 初始化必要常量
            NovelConstant.InitConstatnt();

            // 如果需要，初始化小说
            //NovelConstant.CreateNovels();

            // 小说配置
            _config = new NovelConfig("NovelEditor", "example");
            _config.LoadConfig(NovelConstant.Conditions);
            OpenedFileName.Text = _config.FileName;

            // 绑定节点列表
            NodeListBox.ItemsSource = _config.Values;
            if (_config.Count > 0)
            {
                NodeListBox.SelectedIndex = 0;
            }
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
            if (!Title.StartsWith('*')) Title = "* " + Title;
        }

        private void EditNodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (NodeListBox.SelectedItem is NovelNode selectedNode)
            {
                selectedNode.Content = "此示例节点已被编辑";
                NodeContentText.Text = "文本：" + selectedNode.Content;
                if (!Title.StartsWith('*')) Title = "* " + Title;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "小说配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "选择小说配置文件"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    _config = NovelConfig.LoadFrom(filePath, "NovelEditor", false, NovelConstant.Conditions);
                    OpenedFileName.Text = _config.FileName;
                    NodeListBox.ItemsSource = _config.Values;
                    NodeListBox.Items.Refresh();
                    Title = Title.Replace("*", "").Trim();
                    if (NovelConfig.ExistsFile("NovelEditor", _config.FileName))
                    {
                        MessageBox.Show("这是一个在配置文件中已存在相同文件名的文件，建议保存时使用另存为功能。", "提示");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开文件失败：{ex.Message}");
                }
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _config.SaveConfig();
            NodeListBox.Items.Refresh();
            Title = Title.Replace("*", "").Trim();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new("请输入新的小说章节名称：", "另存为");
            if (inputDialog.ShowDialog() == true)
            {
                string name = inputDialog.ResponseText;
                if (name.Trim() != "")
                {
                    if (NovelConfig.ExistsFile("NovelEditor", name))
                    {
                        if (MessageBox.Show("文件已经存在，是否覆盖？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }
                    NovelConfig config2 = new("NovelEditor", name);
                    foreach (string key in _config.Keys)
                    {
                        config2[key] = _config[key];
                    }
                    _config = config2;
                    _config.SaveConfig();
                    OpenedFileName.Text = _config.FileName;
                    NodeListBox.ItemsSource = _config.Values;
                    NodeListBox.Items.Refresh();
                    Title = Title.Replace("*", "").Trim();
                    MessageBox.Show($"已经添加新的文件：{name}.json", "提示");
                }
                else
                {
                    MessageBox.Show("文件名不能为空，另存为失败。");
                }
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
    }
}

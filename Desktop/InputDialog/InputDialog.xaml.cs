using System.Windows;

namespace Milimoe.FunGame.Testing.Desktop
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        /// <summary>
        /// 获取或设置用户输入的文本。
        /// </summary>
        public string InputText
        {
            get { return InputTextBox.Text; }
            set { InputTextBox.Text = value; }
        }

        /// <summary>
        /// 构造函数，允许设置提示文本和初始值。
        /// </summary>
        /// <param name="prompt">显示给用户的提示信息。</param>
        /// <param name="initialValue">输入框的初始值。</param>
        public InputDialog(string prompt, string initialValue = "")
        {
            InitializeComponent();
            PromptTextBlock.Text = prompt;
            InputTextBox.Text = initialValue;
            InputTextBox.Focus(); // 自动聚焦到输入框
            InputTextBox.SelectAll(); // 选中所有文本，方便用户直接输入覆盖
        }

        /// <summary>
        /// "确定" 按钮点击事件。
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // 设置对话框结果为 true，表示用户点击了确定
            Close();
        }

        /// <summary>
        /// "取消" 按钮点击事件。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // 设置对话框结果为 false，表示用户点击了取消
            Close();
        }

        /// <summary>
        /// 静态方法，方便外部调用显示输入弹窗。
        /// </summary>
        /// <param name="prompt">显示给用户的提示信息。</param>
        /// <param name="initialValue">输入框的初始值。</param>
        /// <param name="owner">弹窗的拥有者窗口，用于居中显示。</param>
        /// <returns>如果用户点击确定，返回输入的字符串；否则返回 null。</returns>
        public static string? Show(string prompt, string initialValue = "", Window? owner = null)
        {
            InputDialog dialog = new(prompt, initialValue);
            if (owner != null)
            {
                dialog.Owner = owner;
            }

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                return dialog.InputText;
            }
            return null;
        }
    }
}

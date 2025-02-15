using System.Windows;

namespace Milimoe.FunGame.Testing.Desktop.Solutions.NovelEditor
{
    public partial class InputDialog : Window
    {
        public string ResponseText { get; set; } = "";

        public InputDialog(string question, string title = "InputDialog")
        {
            InitializeComponent();
            lblQuestion.Text = question;
            Title = title;
            txtInput.Loaded += (sender, e) => txtInput.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = txtInput.Text;
            DialogResult = true;
            Close();
        }
    }
}

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class Showlist : Form
    {
        public string SelectItem { get; set; } = "";

        public Showlist()
        {
            InitializeComponent();
        }

        public void AddListItem(object[] items)
        {
            选择表.Items.Clear();
            选择表.Items.AddRange(items);
        }

        private void 选择表_MouseDoubleClick(object sender, EventArgs e)
        {
            SelectItem = 选择表.SelectedItem?.ToString() ?? "";
            Dispose();
        }
    }
}

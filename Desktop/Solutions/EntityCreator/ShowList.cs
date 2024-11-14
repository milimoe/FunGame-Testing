using System.ComponentModel;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class ShowList : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectItem { get; set; } = "";

        public ShowList()
        {
            InitializeComponent();
            Text = "双击选择一项";
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

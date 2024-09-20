using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class ItemCreator : Form
    {
        private ItemManager ItemManager { get; }
        private Item? EditItem { get; }

        public ItemCreator(ItemManager manager, Item? item = null)
        {
            InitializeComponent();
            ItemManager = manager;
            if (item != null)
            {
                Text = "物品编辑器";
                BtnCreate.Text = "编辑";
                EditItem = item;
                TextID.Text = item.Id.ToString();
                TextName.Text = item.Name;
                TextCode.Text = manager.LoadedItems.Where(kv => kv.Value.Equals(item)).Select(kv => kv.Key).FirstOrDefault() ?? "";
                ComboItemType.Text = ItemSet.GetItemTypeName(item.ItemType);
                CheckboxIsWeapon.Checked = item.ItemType == ItemType.Weapon;
                ComboWeapon.Text = ItemSet.GetWeaponTypeName(item.WeaponType);
                ComboWeapon.Enabled = item.ItemType == ItemType.Weapon;
                CheckboxIsEquip.Checked = item.Equipable;
                ComboEquip.Text = ItemSet.GetEquipSlotTypeName(item.EquipSlotType);
                ComboEquip.Enabled = item.Equipable;
                CheckboxIsPurchasable.Checked = item.IsPurchasable;
                TextPrice.Text = item.Price.ToString();
                TextPrice.Enabled = item.IsPurchasable;
                CheckboxIsSellable.Checked = item.IsSellable;
                CheckboxIsTradable.Checked = item.IsTradable;
                if (item.NextSellableTime > DateTime.Now && item.NextSellableTime < DateTime.MaxValue) DateTimePickerSell.Value = item.NextSellableTime;
                DateTimePickerSell.Enabled = !item.IsSellable;
                if (item.NextTradableTime > DateTime.Now && item.NextTradableTime < DateTime.MaxValue) DateTimePickerTrade.Value = item.NextTradableTime;
                DateTimePickerTrade.Enabled = !item.IsTradable;
            }
            else
            {
                CheckboxIsWeapon.Checked = false;
                ComboWeapon.Enabled = false;
                CheckboxIsEquip.Checked = false;
                ComboEquip.Enabled = false;
                CheckboxIsPurchasable.Checked = false;
                TextPrice.Enabled = false;
                CheckboxIsSellable.Checked = true;
                CheckboxIsTradable.Checked = true;
                DateTimePickerSell.Value = DateTime.Now;
                DateTimePickerSell.Enabled = false;
                DateTimePickerTrade.Value = DateTime.Now;
                DateTimePickerTrade.Enabled = false;
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            Item i;
            if (EditItem != null)
            {
                i = EditItem;
            }
            else
            {
                i = Factory.GetItem();
            }
            string name;

            if (TextID.Text.Trim() != "" && long.TryParse(TextID.Text.Trim(), out long id))
            {
                i.Id = id;
            }
            else
            {
                MessageBox.Show("ID不能为空。");
                return;
            }

            if (TextName.Text.Trim() != "")
            {
                i.Name = TextName.Text.Trim();
            }
            else
            {
                MessageBox.Show("名称不能为空。");
                return;
            }

            if (TextCode.Text.Trim() != "")
            {
                name = TextCode.Text.Trim();
            }
            else
            {
                MessageBox.Show("物品存档标识不能为空。");
                return;
            }

            if (CheckboxIsWeapon.Checked && ComboWeapon.Text.Trim() != "")
            {
                i.WeaponType = ItemSet.GetWeaponTypeFromName(ComboWeapon.Text.Trim());
            }

            if (CheckboxIsEquip.Checked && ComboEquip.Text.Trim() != "")
            {
                i.EquipSlotType = ItemSet.GetEquipSlotTypeFromName(ComboEquip.Text.Trim());
            }

            if (CheckboxIsPurchasable.Checked && TextPrice.Text.Trim() != "" && double.TryParse(TextPrice.Text.Trim(), out double price))
            {
                i.Price = price;
            }

            if (!CheckboxIsSellable.Checked && DateTime.Now < DateTimePickerSell.Value)
            {
                i.NextSellableTime = DateTimePickerSell.Value;
            }

            if (!CheckboxIsTradable.Checked && DateTime.Now < DateTimePickerTrade.Value)
            {
                i.NextTradableTime = DateTimePickerTrade.Value;
            }

            if (EditItem != null)
            {
                MessageBox.Show("保存成功！");
                Dispose();
                return;
            }
            else
            {
                if (ItemManager.Add(name, i))
                {
                    ShowDetail d = new(null, null, ItemManager);
                    d.SetText(-1, i, i.ToString());
                    d.Text = "预览模式";
                    d.ShowDialog();
                    d.Dispose();
                    if (MessageBox.Show("添加成功！是否继续添加？", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        Dispose();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("添加失败！");
                }
            }

            TextID.Text = "";
            TextName.Text = "";
            TextCode.Text = "";
            ComboItemType.Text = "";
            CheckboxIsWeapon.Checked = false;
            ComboWeapon.Text = "";
            ComboWeapon.Enabled = false;
            CheckboxIsEquip.Checked = false;
            ComboEquip.Text = "";
            ComboEquip.Enabled = false;
            CheckboxIsPurchasable.Checked = false;
            TextPrice.Text = "";
            TextPrice.Enabled = false;
            CheckboxIsSellable.Checked = true;
            CheckboxIsTradable.Checked = true;
            DateTimePickerSell.Value = DateTime.Now;
            DateTimePickerSell.Enabled = false;
            DateTimePickerTrade.Value = DateTime.Now;
            DateTimePickerTrade.Enabled = false;
        }

        private void CheckboxIsWeapon_CheckedChanged(object sender, EventArgs e)
        {
            ComboWeapon.Enabled = CheckboxIsWeapon.Checked;
        }

        private void CheckboxIsEquip_CheckedChanged(object sender, EventArgs e)
        {
            ComboEquip.Enabled = CheckboxIsEquip.Checked;
        }

        private void CheckboxIsPurchasable_CheckedChanged(object sender, EventArgs e)
        {
            TextPrice.Enabled = CheckboxIsPurchasable.Checked;
        }

        private void CheckboxIsSellable_CheckedChanged(object sender, EventArgs e)
        {
            DateTimePickerSell.Enabled = !CheckboxIsSellable.Checked;
        }

        private void CheckboxIsTradable_CheckedChanged(object sender, EventArgs e)
        {
            DateTimePickerTrade.Enabled = !CheckboxIsTradable.Checked;
        }
    }
}

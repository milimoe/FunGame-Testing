using Microsoft.VisualBasic;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class EntityCreator : Form
    {
        private static GameModuleLoader? GameModuleLoader { get; set; } = null;
        private CharacterCreator CharacterCreator { get; } = new();
        private SkillCreator SkillCreator { get; } = new();
        private ItemCreator ItemCreator { get; } = new();
        private bool CheckSelectedIndex => 实际列表.SelectedIndex != -1 && 实际列表.SelectedIndex < 实际列表.Items.Count;
        private int nowClick = 0;

        public EntityCreator()
        {
            InitializeComponent();
            GameModuleLoader = LoadModules();
            CharacterCreator.Load();
            SkillCreator.Load();
            ItemCreator.Load();
            查看现有技能方法();
            查看现有物品方法();
            查看现有角色方法();
        }

        private void 查看现有角色方法()
        {
            实际列表.Items.Clear();
            foreach (string name in CharacterCreator.LoadedCharacters.Keys)
            {
                实际列表.Items.Add(CharacterCreator.LoadedCharacters[name]);
            }
            nowClick = 0;
        }

        private void 查看现有技能方法()
        {
            实际列表.Items.Clear();
            foreach (string name in SkillCreator.LoadedSkills.OrderBy(kv => kv.Value.Id).Select(kv => kv.Key))
            {
                实际列表.Items.Add(GetSkillDisplayName(SkillCreator, name));
            }
            nowClick = 1;
        }

        private void 查看现有物品方法()
        {
            实际列表.Items.Clear();
            foreach (string name in ItemCreator.LoadedItems.OrderBy(kv => kv.Value.Id).Select(kv => kv.Key))
            {
                实际列表.Items.Add(GetItemDisplayName(ItemCreator, name));
            }
            nowClick = 2;
        }

        private void 查看现有角色_Click(object sender, EventArgs e)
        {
            if (nowClick != 0)
            {
                查看现有角色方法();
            }
        }

        private void 查看现有技能_Click(object sender, EventArgs e)
        {
            if (nowClick != 1)
            {
                查看现有技能方法();
            }
        }

        private void 查看现有物品_Click(object sender, EventArgs e)
        {
            if (nowClick != 2)
            {
                查看现有物品方法();
            }
        }

        private void 全部保存_Click(object sender, EventArgs e)
        {
            CharacterCreator.Save();
            SkillCreator.Save();
            ItemCreator.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存角色_Click(object sender, EventArgs e)
        {
            CharacterCreator.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存技能_Click(object sender, EventArgs e)
        {
            SkillCreator.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存物品_Click(object sender, EventArgs e)
        {
            ItemCreator.Save();
            MessageBox.Show("保存成功！");
        }

        public static Skill? 从模组加载器中获取技能(long id, string name, SkillType type)
        {
            if (GameModuleLoader != null)
            {
                foreach (SkillModule module in GameModuleLoader.Skills.Values)
                {
                    Skill? s = module.GetSkill(id, name, type);
                    if (s != null)
                    {
                        return s;
                    }
                }
            }
            return null;
        }

        public static Item? 从模组加载器中获取物品(long id, string name, ItemType type)
        {
            if (GameModuleLoader != null)
            {
                foreach (ItemModule module in GameModuleLoader.Items.Values)
                {
                    Item? i = module.GetItem(id, name, type);
                    if (i != null)
                    {
                        return i;
                    }
                }
            }
            return null;
        }

        private void 实际列表_MouseDoubleClick(object sender, EventArgs e)
        {
            if (CheckSelectedIndex)
            {
                ShowDetail d = new(CharacterCreator, SkillCreator, ItemCreator);
                switch (nowClick)
                {
                    case 0:
                        Character? character = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                        d.SetText(nowClick, character, character?.GetInfo() ?? "");
                        d.ShowDialog();
                        break;
                    case 1:
                        Skill? s = SkillCreator.LoadedSkills.Where(kv => GetSkillDisplayName(SkillCreator, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).Select(kv => kv.Value).FirstOrDefault();
                        if (s != null)
                        {
                            Skill? s2 = 从模组加载器中获取技能(s.Id, s.Name, s.SkillType);
                            if (s2 != null)
                            {
                                s = s2;
                            }
                            d.SetText(nowClick, s, s.ToString() ?? "");
                            d.ShowDialog();
                        }
                        break;
                    case 2:
                        Item? i = ItemCreator.LoadedItems.Where(kv => GetItemDisplayName(ItemCreator, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).Select(kv => kv.Value).FirstOrDefault();
                        if (i != null)
                        {
                            Item? i2 = 从模组加载器中获取物品(i.Id, i.Name, i.ItemType);
                            if (i2 != null)
                            {
                                i.SetPropertyToItemModuleNew(i2);
                                i = i2;
                            }
                            d.SetText(nowClick, i, i.ToString() ?? "");
                            d.ShowDialog();
                        }
                        break;
                    default:
                        break;
                }
                d.Dispose();
            }
        }

        private void 为角色添加技能方法()
        {
            if (SkillCreator.LoadedSkills.Count != 0)
            {
                ShowList l = new();
                l.AddListItem(SkillCreator.LoadedSkills.OrderBy(kv => kv.Value.Id).Where(kv => kv.Value.SkillType != SkillType.Item).Select(kv => GetSkillDisplayName(SkillCreator, kv.Key)).ToArray());
                l.ShowDialog();
                string selected = l.SelectItem;
                Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                Skill? s = SkillCreator.LoadedSkills.Where(kv => GetSkillDisplayName(SkillCreator, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
                if (c != null && s != null)
                {
                    Skill? s2 = 从模组加载器中获取技能(s.Id, s.Name, s.SkillType);
                    if (s2 != null)
                    {
                        s = s2;
                    }
                    s.Character = c;
                    c.Skills.Add(s);
                }
            }
            else
            {
                MessageBox.Show("技能列表为空！");
            }
        }

        private void 为角色添加物品方法()
        {
            if (ItemCreator.LoadedItems.Count != 0)
            {
                ShowList l = new();
                l.AddListItem(ItemCreator.LoadedItems.OrderBy(kv => kv.Value.Id).Select(kv => GetItemDisplayName(ItemCreator, kv.Key)).ToArray());
                l.ShowDialog();
                string selected = l.SelectItem;
                Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                Item? i = ItemCreator.LoadedItems.Where(kv => GetItemDisplayName(ItemCreator, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
                if (c != null && i != null)
                {
                    Item? i2 = 从模组加载器中获取物品(i.Id, i.Name, i.ItemType);
                    if (i2 != null)
                    {
                        i.SetPropertyToItemModuleNew(i2);
                        i = i2;
                    }
                    c.Equip(i);
                }
            }
            else
            {
                MessageBox.Show("物品列表为空！");
            }
        }

        private void 删除角色技能方法()
        {
            Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
            if (c != null)
            {
                if (c.Skills.Count != 0)
                {
                    ShowList l = new();
                    l.AddListItem(c.Skills.Select(s => s.GetIdName()).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Skill? s = c.Skills.Where(s => s.GetIdName() == selected).FirstOrDefault();
                    if (s != null) c.Skills.Remove(s);
                }
                else
                {
                    MessageBox.Show("技能列表为空！");
                }
            }
        }

        private void 删除角色物品方法()
        {
            Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
            if (c != null)
            {
                if (c.Items.Count != 0)
                {
                    ShowList l = new();
                    l.AddListItem(c.Items.Select(s => s.GetIdName()).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Item? i = c.Items.Where(i => i.GetIdName() == selected).FirstOrDefault();
                    if (i != null)
                    {
                        c.UnEquip(c.EquipSlot.GetEquipItemToSlot(i));
                    }
                }
                else
                {
                    MessageBox.Show("物品列表为空！");
                }
            }
        }

        private void 为角色添加技能_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                为角色添加技能方法();
            }
        }

        private void 为角色添加物品_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                为角色添加物品方法();
            }
        }

        private void 删除角色技能_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                删除角色技能方法();
            }
        }

        private void 删除角色物品_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                删除角色物品方法();
            }
        }

        private void 创建角色_Click(object sender, EventArgs e)
        {
            CharacterCreator.OpenCreator();
            查看现有角色方法();
        }

        private void 创建技能_Click(object sender, EventArgs e)
        {
            SkillCreator.OpenCreator();
            查看现有技能方法();
        }

        private void 创建物品_Click(object sender, EventArgs e)
        {
            ItemCreator.OpenCreator();
            查看现有物品方法();
        }

        private void 删除角色_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0 && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = CharacterCreator.LoadedCharacters.Where(ky => ky.Value.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (CharacterCreator.Remove(name))
                {
                    MessageBox.Show("删除成功！");
                    查看现有角色方法();
                }
                else
                {
                    MessageBox.Show("删除失败！");
                }
            }
        }

        private void 删除技能_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 1 && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = SkillCreator.LoadedSkills.Where(kv => GetSkillDisplayName(SkillCreator, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (SkillCreator.Remove(name))
                {
                    MessageBox.Show("删除成功！");
                    查看现有技能方法();
                }
                else
                {
                    MessageBox.Show("删除失败！");
                }
            }
        }

        private void 删除物品_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 2 && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = ItemCreator.LoadedItems.Where(kv => GetItemDisplayName(ItemCreator, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (ItemCreator.Remove(name))
                {
                    MessageBox.Show("删除成功！");
                    查看现有物品方法();
                }
                else
                {
                    MessageBox.Show("删除失败！");
                }
            }
        }

        private void 重新读取全部_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("重新读取会丢失未保存的数据，是否确认重新读取全部？", "重新读取全部", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                CharacterCreator.Load();
                SkillCreator.Load();
                ItemCreator.Load();
                查看现有技能方法();
                查看现有物品方法();
                查看现有角色方法();
            }
        }

        public static string GetSkillDisplayName(SkillCreator skillCreator, string name)
        {
            if (skillCreator.LoadedSkills.TryGetValue(name, out Skill? skill) && skill != null)
            {
                return $"[ {name} ] {skill.GetIdName()}";
            }
            return "";
        }

        public static string GetItemDisplayName(ItemCreator itemCreator, string name)
        {
            if (itemCreator.LoadedItems.TryGetValue(name, out Item? item) && item != null)
            {
                return $"[ {name} ] {item.GetIdName()}";
            }
            return "";
        }

        private static ItemType SelectItemType()
        {
            ShowList l = new();
            l.AddListItem([ItemType.MagicCardPack, ItemType.Weapon, ItemType.Armor, ItemType.Shoes, ItemType.Accessory,
                ItemType.Consumable, ItemType.MagicCard, ItemType.Collectible, ItemType.SpecialItem, ItemType.QuestItem, ItemType.GiftBox, ItemType.Others]);
            l.Text = "选择一个物品类型";
            l.ShowDialog();
            return Enum.TryParse(l.SelectItem, out ItemType type) ? type : ItemType.Others;
        }

        private static GameModuleLoader LoadModules()
        {
            PluginLoader plugins = PluginLoader.LoadPlugins([]);
            foreach (string plugin in plugins.Plugins.Keys)
            {
                Console.WriteLine(plugin + " is loaded.");
            }

            return GameModuleLoader.LoadGameModules(FunGameInfo.FunGame.FunGame_Desktop, []);
        }
    }
}

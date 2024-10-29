using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class EntityEditor : Form
    {
        private static GameModuleLoader? GameModuleLoader { get; set; } = null;
        private CharacterManager CharacterManager { get; } = new();
        private SkillManager SkillManager { get; } = new();
        private ItemManager ItemManager { get; } = new();
        private SetConfigName ConfigSettings { get; }
        private bool CheckSelectedIndex => 实际列表.SelectedIndex != -1 && 实际列表.SelectedIndex < 实际列表.Items.Count;
        private int nowClick = 0;

        public EntityEditor()
        {
            InitializeComponent();
            ConfigSettings = new(CharacterManager, SkillManager, ItemManager);
            GameModuleLoader = LoadModules();
            CharacterManager.Load();
            SkillManager.Load();
            ItemManager.Load();
            foreach (Character character in CharacterManager.LoadedCharacters.Values)
            {
                Skill[] skills = [.. character.Skills];
                for (int i = 0; i < skills.Length; i++)
                {
                    Skill skill = skills[i];
                    character.Skills.Remove(skill);
                    Skill? s = 从模组加载器中获取技能(skill.Id, skill.Name, skill.SkillType);
                    if (s != null)
                    {
                        skill = s.Copy();
                        skill.Character = character;
                    }
                    character.Skills.Add(skill);
                }
                if (character.EquipSlot.MagicCardPack != null)
                {
                    Item item = character.EquipSlot.MagicCardPack;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                if (character.EquipSlot.Weapon != null)
                {
                    Item item = character.EquipSlot.Weapon;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                if (character.EquipSlot.Armor != null)
                {
                    Item item = character.EquipSlot.Armor;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                if (character.EquipSlot.Shoes != null)
                {
                    Item item = character.EquipSlot.Shoes;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                if (character.EquipSlot.Accessory1 != null)
                {
                    Item item = character.EquipSlot.Accessory1;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                if (character.EquipSlot.Accessory2 != null)
                {
                    Item item = character.EquipSlot.Accessory2;
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        character.Equip(item);
                    }
                }
                Item[] items = [.. character.Items];
                for (int j = 0; j < items.Length; j++)
                {
                    Item item = items[j];
                    character.Items.Remove(item);
                    Item? i = 从模组加载器中获取物品(item.Id, item.Name, item.ItemType);
                    if (i != null)
                    {
                        item.SetPropertyToItemModuleNew(i);
                        item = i.Copy();
                        item.Character = character;
                    }
                }
            }
            查看现有技能方法();
            查看现有物品方法();
            查看现有角色方法();
        }

        private void 查看现有角色方法()
        {
            实际列表.Items.Clear();
            foreach (string name in CharacterManager.LoadedCharacters.Keys)
            {
                实际列表.Items.Add(CharacterManager.LoadedCharacters[name]);
            }
            nowClick = 0;
        }

        private void 查看现有技能方法()
        {
            实际列表.Items.Clear();
            foreach (string name in SkillManager.LoadedSkills.OrderBy(kv => kv.Value.Id).Select(kv => kv.Key))
            {
                实际列表.Items.Add(GetSkillDisplayName(SkillManager, name));
            }
            nowClick = 1;
        }

        private void 查看现有物品方法()
        {
            实际列表.Items.Clear();
            foreach (string name in ItemManager.LoadedItems.OrderBy(kv => kv.Value.Id).Select(kv => kv.Key))
            {
                实际列表.Items.Add(GetItemDisplayName(ItemManager, name));
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
            CharacterManager.Save();
            SkillManager.Save();
            ItemManager.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存角色_Click(object sender, EventArgs e)
        {
            CharacterManager.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存技能_Click(object sender, EventArgs e)
        {
            SkillManager.Save();
            MessageBox.Show("保存成功！");
        }

        private void 保存物品_Click(object sender, EventArgs e)
        {
            ItemManager.Save();
            MessageBox.Show("保存成功！");
        }

        public static Skill? 从模组加载器中获取技能(long id, string name, SkillType type)
        {
            //if (GameModuleLoader != null)
            //{
            //    foreach (SkillModule module in GameModuleLoader.Skills.Values)
            //    {
            //        Skill? s = module.GetSkill(id, name, type);
            //        if (s != null)
            //        {
            //            return s;
            //        }
            //    }
            //}
            return null;
        }

        public static Item? 从模组加载器中获取物品(long id, string name, ItemType type)
        {
            //if (GameModuleLoader != null)
            //{
            //    foreach (ItemModule module in GameModuleLoader.Items.Values)
            //    {
            //        Item? i = module.GetItem(id, name, type);
            //        if (i != null)
            //        {
            //            return i;
            //        }
            //    }
            //}
            return null;
        }

        private void 实际列表_MouseDoubleClick(object sender, EventArgs e)
        {
            if (CheckSelectedIndex)
            {
                ShowDetail d = new(CharacterManager, SkillManager, ItemManager);
                switch (nowClick)
                {
                    case 0:
                        Character? character = CharacterManager.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                        d.SetText(nowClick, character, character?.GetInfo() ?? "");
                        d.ShowDialog();
                        break;
                    case 1:
                        Skill? s = SkillManager.LoadedSkills.Where(kv => GetSkillDisplayName(SkillManager, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).Select(kv => kv.Value).FirstOrDefault();
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
                        Item? i = ItemManager.LoadedItems.Where(kv => GetItemDisplayName(ItemManager, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).Select(kv => kv.Value).FirstOrDefault();
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
            if (SkillManager.LoadedSkills.Count != 0)
            {
                ShowList l = new();
                l.AddListItem(SkillManager.LoadedSkills.OrderBy(kv => kv.Value.Id).Where(kv => kv.Value.SkillType != SkillType.Item).Select(kv => GetSkillDisplayName(SkillManager, kv.Key)).ToArray());
                l.ShowDialog();
                string selected = l.SelectItem;
                Character? c = CharacterManager.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                Skill? s = SkillManager.LoadedSkills.Where(kv => GetSkillDisplayName(SkillManager, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
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
            if (ItemManager.LoadedItems.Count != 0)
            {
                ShowList l = new();
                l.AddListItem(ItemManager.LoadedItems.OrderBy(kv => kv.Value.Id).Select(kv => GetItemDisplayName(ItemManager, kv.Key)).ToArray());
                l.ShowDialog();
                string selected = l.SelectItem;
                Character? c = CharacterManager.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                Item? i = ItemManager.LoadedItems.Where(kv => GetItemDisplayName(ItemManager, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
                if (c != null && i != null)
                {
                    if (i.Equipable)
                    {
                        Item? i2 = 从模组加载器中获取物品(i.Id, i.Name, i.ItemType);
                        if (i2 != null)
                        {
                            i.SetPropertyToItemModuleNew(i2);
                            i = i2;
                        }
                        if (i.Equipable) c.Equip(i);
                        else c.Items.Add(i);
                    }
                    else c.Items.Add(i);
                }
            }
            else
            {
                MessageBox.Show("物品列表为空！");
            }
        }

        private void 删除角色技能方法()
        {
            Character? c = CharacterManager.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
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
            //Character? c = CharacterManager.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
            //if (c != null)
            //{
            //    if (c.Items.Count != 0 || c.EquipSlot.Any())
            //    {
            //        ShowList l = new();
            //        l.AddListItem(c.Items.Select(s => s.GetIdName()).ToArray());
            //        l.ShowDialog();
            //        string selected = l.SelectItem;
            //        Item? i = c.Items.Where(i => i.GetIdName() == selected).FirstOrDefault();
            //        if (i != null)
            //        {
            //            if (i.Equipable) c.UnEquip(c.EquipSlot.GetEquipItemToSlot(i));
            //            else c.Items.Remove(i);
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("物品列表为空！");
            //    }
            //}
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
            CharacterManager.OpenCreator();
            查看现有角色方法();
        }

        private void 创建技能_Click(object sender, EventArgs e)
        {
            SkillManager.OpenCreator();
            查看现有技能方法();
        }

        private void 创建物品_Click(object sender, EventArgs e)
        {
            ItemManager.OpenCreator();
            查看现有物品方法();
        }

        private void 删除角色_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0 && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = CharacterManager.LoadedCharacters.Where(ky => ky.Value.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (CharacterManager.Remove(name))
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
                string name = SkillManager.LoadedSkills.Where(kv => GetSkillDisplayName(SkillManager, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (SkillManager.Remove(name))
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
                string name = ItemManager.LoadedItems.Where(kv => GetItemDisplayName(ItemManager, kv.Key) == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
                if (ItemManager.Remove(name))
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
                CharacterManager.Load();
                SkillManager.Load();
                ItemManager.Load();
                查看现有技能方法();
                查看现有物品方法();
                查看现有角色方法();
            }
        }

        public static string GetSkillDisplayName(SkillManager skillCreator, string name)
        {
            if (skillCreator.LoadedSkills.TryGetValue(name, out Skill? skill) && skill != null)
            {
                return $"[ {name} ] {skill.GetIdName()}";
            }
            return "";
        }

        public static string GetItemDisplayName(ItemManager itemCreator, string name)
        {
            if (itemCreator.LoadedItems.TryGetValue(name, out Item? item) && item != null)
            {
                return $"[ {name} ] {item.GetIdName()}";
            }
            return "";
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

        private void 模组名称设置_Click(object sender, EventArgs e)
        {
            ConfigSettings.Show();
        }
    }
}

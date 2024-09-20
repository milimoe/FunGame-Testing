using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class ShowDetail : Form
    {
        private CharacterCreator? CharacterCreator { get; }
        private SkillCreator? SkillCreator { get; }
        private ItemCreator? ItemCreator { get; }
        private int NowClick { get; set; } = -1;
        private BaseEntity? BaseEntity { get; set; }

        public ShowDetail(CharacterCreator? characterCreator, SkillCreator? skillCreator, ItemCreator? itemCreator)
        {
            InitializeComponent();
            CharacterCreator = characterCreator;
            SkillCreator = skillCreator;
            ItemCreator = itemCreator;
            Text = "详细信息查看";
        }

        public void SetText(int nowClick, BaseEntity? entity, string text)
        {
            NowClick = nowClick;
            BaseEntity = entity;
            if (nowClick == 0)
            {
                删技能.Enabled = true;
                删物品.Enabled = true;
                加技能.Enabled = true;
                加物品.Enabled = true;
                编辑.Enabled = true;
            }
            else if (nowClick == 1)
            {
                删技能.Enabled = false;
                删物品.Enabled = false;
                加技能.Enabled = false;
                加物品.Enabled = false;
                编辑.Enabled = true;
            }
            else if (nowClick == 2)
            {
                删技能.Enabled = true;
                删物品.Enabled = false;
                加技能.Enabled = true;
                加物品.Enabled = false;
                编辑.Enabled = true;
            }
            else
            {
                删技能.Enabled = false;
                删物品.Enabled = false;
                加技能.Enabled = false;
                加物品.Enabled = false;
                编辑.Enabled = false;
            }
            详细内容.Text = text;
        }

        private void 编辑_Click(object sender, EventArgs e)
        {
            if (NowClick == 0 && BaseEntity is Character c)
            {
                CharacterCreator?.OpenCreator(c);
            }
            else if (NowClick == 1 && BaseEntity is Skill s)
            {
                SkillCreator?.OpenCreator(s);
            }
            else if (NowClick == 2 && BaseEntity is Item i)
            {
                ItemCreator?.OpenCreator(i);
            }
        }

        private void 加物品_Click(object sender, EventArgs e)
        {
            if (NowClick == 0 && ItemCreator != null && BaseEntity is Character c)
            {
                if (ItemCreator.LoadedItems.Count != 0)
                {
                    ShowList l = new();
                    l.AddListItem(ItemCreator.LoadedItems.OrderBy(kv => kv.Value.Id).Select(kv => EntityCreator.GetItemDisplayName(ItemCreator, kv.Key)).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Item? i = ItemCreator.LoadedItems.Where(kv => EntityCreator.GetItemDisplayName(ItemCreator, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
                    if (c != null && i != null)
                    {
                        Item? i2 = EntityCreator.从模组加载器中获取物品(i.Id, i.Name, i.ItemType);
                        if (i2 != null)
                        {
                            i.SetPropertyToItemModuleNew(i2);
                            i = i2;
                        }
                        c.Equip(i);
                        详细内容.Text = c.GetInfo();
                    }
                }
                else
                {
                    MessageBox.Show("物品列表为空！");
                }
            }
        }

        private void 加技能_Click(object sender, EventArgs e)
        {
            if (NowClick == 0 && SkillCreator != null && BaseEntity is Character c)
            {
                if (SkillCreator.LoadedSkills.Count != 0)
                {
                    ShowList l = new();
                    l.AddListItem(SkillCreator.LoadedSkills.OrderBy(kv => kv.Value.Id).Where(kv => kv.Value.SkillType != Core.Library.Constant.SkillType.Item).Select(kv => EntityCreator.GetSkillDisplayName(SkillCreator, kv.Key)).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Skill? s = SkillCreator.LoadedSkills.Where(kv => EntityCreator.GetSkillDisplayName(SkillCreator, kv.Key) == selected).Select(kv => kv.Value).FirstOrDefault();
                    if (c != null && s != null)
                    {
                        Skill? s2 = EntityCreator.从模组加载器中获取技能(s.Id, s.Name, s.SkillType);
                        if (s2 != null)
                        {
                            s = s2;
                        }
                        s.Character = c;
                        c.Skills.Add(s);
                        详细内容.Text = c.GetInfo();
                    }
                }
                else
                {
                    MessageBox.Show("技能列表为空！");
                }
            }
            else if (NowClick == 2)
            {
                MessageBox.Show("再说吧，暂不支持。");
            }
        }

        private void 删物品_Click(object sender, EventArgs e)
        {
            if (NowClick == 0 && BaseEntity is Character c)
            {
                if (c != null)
                {
                    if (c.Items.Count != 0)
                    {
                        ShowList l = new();
                        l.AddListItem(c.Items.OrderBy(i => i.Id).Select(i => i.GetIdName()).ToArray());
                        l.ShowDialog();
                        string selected = l.SelectItem;
                        Item? i = c.Items.Where(i => i.GetIdName() == selected).FirstOrDefault();
                        if (i != null)
                        {
                            c.UnEquip(c.EquipSlot.GetEquipItemToSlot(i));
                            详细内容.Text = c.GetInfo();
                        }
                    }
                    else
                    {
                        MessageBox.Show("物品列表为空！");
                    }
                }
            }
        }

        private void 删技能_Click(object sender, EventArgs e)
        {
            if (NowClick == 0 && BaseEntity is Character c)
            {
                if (c != null)
                {
                    if (c.Skills.Count != 0)
                    {
                        ShowList l = new();
                        l.AddListItem(c.Skills.OrderBy(s => s.Id).Select(s => s.GetIdName()).ToArray());
                        l.ShowDialog();
                        string selected = l.SelectItem;
                        Skill? s = c.Skills.Where(s => s.GetIdName() == selected).FirstOrDefault();
                        if (s != null)
                        {
                            c.Skills.Remove(s);
                            详细内容.Text = c.GetInfo();
                        }
                    }
                    else
                    {
                        MessageBox.Show("技能列表为空！");
                    }
                }
            }
            else if (NowClick == 2)
            {
                MessageBox.Show("再说吧，暂不支持。");
            }
        }
    }
}

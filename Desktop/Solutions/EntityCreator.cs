using Microsoft.VisualBasic;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class EntityCreator : Form
    {
        private CharacterCreator CharacterCreator { get; } = new();
        private SkillCreator SkillCreator { get; } = new();
        private ItemCreator ItemCreator { get; } = new();
        private GameModuleLoader GameModuleLoader { get; }
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
            foreach (string name in SkillCreator.LoadedSkills.Keys)
            {
                实际列表.Items.Add(SkillCreator.LoadedSkills[name].GetIdName());
            }
            nowClick = 1;
        }

        private void 查看现有物品方法()
        {
            实际列表.Items.Clear();
            foreach (string name in ItemCreator.LoadedItems.Keys)
            {
                实际列表.Items.Add(ItemCreator.LoadedItems[name].GetIdName());
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

        private Skill? 从模组加载器中获取技能(long id, string name)
        {
            foreach (SkillModule module in GameModuleLoader.Skills.Values)
            {
                Skill? s = module.GetSkill(id, name);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }
        
        private Item? 从模组加载器中获取物品(long id, string name)
        {
            foreach (ItemModule module in GameModuleLoader.Items.Values)
            {
                Item? i = module.GetItem(id, name);
                if (i != null)
                {
                    return i;
                }
            }
            return null;
        }

        private void 实际列表_MouseDoubleClick(object sender, EventArgs e)
        {
            if (CheckSelectedIndex)
            {
                ShowDetail d = new();
                switch (nowClick)
                {
                    case 0:
                        d.SetText(CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault()?.GetInfo() ?? "");
                        d.ShowDialog();
                        break;
                    case 1:
                        Skill? s = SkillCreator.LoadedSkills.Values.Where(c => c.GetIdName() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                        if (s != null)
                        {
                            s = 从模组加载器中获取技能(s.Id, s.Name);
                            if (s != null)
                            {
                                d.SetText(s.ToString() ?? "");
                                d.ShowDialog();
                            }
                        }
                        break;
                    case 2:
                        Item? i = ItemCreator.LoadedItems.Values.Where(c => c.GetIdName() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                        if (i != null)
                        {
                            i = 从模组加载器中获取物品(i.Id, i.Name);
                            if (i != null)
                            {
                                d.SetText(i.ToString() ?? "");
                                d.ShowDialog();
                            }
                        }
                        break;
                    default:
                        break;
                }
                d.Dispose();
            }
        }

        private void 为角色添加技能_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                if (SkillCreator.LoadedSkills.Count != 0)
                {
                    Showlist l = new();
                    l.AddListItem(SkillCreator.LoadedSkills.Values.Select(s => s.GetIdName()).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                    Skill? s = SkillCreator.LoadedSkills.Values.Where(s => s.GetIdName() == selected).FirstOrDefault();
                    if (c != null && s != null)
                    {
                        s = 从模组加载器中获取技能(s.Id, s.Name);
                        if (s != null) c.Skills.Add(s);
                    }
                }
                else
                {
                    MessageBox.Show("技能列表为空！");
                }
            }
        }

        private void 为角色添加物品_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && nowClick == 0)
            {
                if (ItemCreator.LoadedItems.Count != 0)
                {
                    Showlist l = new();
                    l.AddListItem(ItemCreator.LoadedItems.Values.Select(i => i.GetIdName()).ToArray());
                    l.ShowDialog();
                    string selected = l.SelectItem;
                    Character? c = CharacterCreator.LoadedCharacters.Values.Where(c => c.ToString() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault();
                    Item? i = ItemCreator.LoadedItems.Values.Where(i => i.GetIdName() == selected).FirstOrDefault();
                    if (c != null && i != null)
                    {
                        i = 从模组加载器中获取物品(i.Id, i.Name);
                        if (i != null) c.Items.Add(i);
                    }
                }
                else
                {
                    MessageBox.Show("物品列表为空！");
                }
            }
        }

        private void 创建角色_Click(object sender, EventArgs e)
        {
            Character c = Factory.GetCharacter();
            c.Name = Interaction.InputBox("输入姓", "姓", "");
            c.FirstName = Interaction.InputBox("输入名", "名", "");
            c.NickName = Interaction.InputBox("输入昵称", "昵称", "");

            string primaryAttributeInput = Interaction.InputBox("输入核心属性 (STR, AGI, INT)", "核心属性", "None");
            c.PrimaryAttribute = Enum.TryParse(primaryAttributeInput, out PrimaryAttribute primaryAttribute) ? primaryAttribute : PrimaryAttribute.None;

            // 解析 double 类型的输入
            c.InitialATK = double.Parse(Interaction.InputBox("输入初始攻击力", "初始攻击力", "0.0"));
            c.InitialHP = double.Parse(Interaction.InputBox("输入初始生命值", "初始生命值", "1.0"));
            c.InitialMP = double.Parse(Interaction.InputBox("输入初始魔法值", "初始魔法值", "0.0"));
            c.InitialSTR = double.Parse(Interaction.InputBox("输入初始力量", "初始力量", "0.0"));
            c.STRGrowth = double.Parse(Interaction.InputBox("输入力量成长", "力量成长", "0.0"));
            c.InitialAGI = double.Parse(Interaction.InputBox("输入初始敏捷", "初始敏捷", "0.0"));
            c.AGIGrowth = double.Parse(Interaction.InputBox("输入敏捷成长", "敏捷成长", "0.0"));
            c.InitialINT = double.Parse(Interaction.InputBox("输入初始智力", "初始智力", "0.0"));
            c.INTGrowth = double.Parse(Interaction.InputBox("输入智力成长", "智力成长", "0.0"));
            c.InitialSPD = double.Parse(Interaction.InputBox("输入初始行动速度", "初始行动速度", "0.0"));
            c.InitialHR = double.Parse(Interaction.InputBox("输入初始生命回复", "初始生命回复", "0.0"));
            c.InitialMR = double.Parse(Interaction.InputBox("输入初始魔法回复", "初始魔法回复", "0.0"));

            string name = Interaction.InputBox("输入角色代号以确认创建", "角色代号", "");
            if (name != "" && c.Name != "" && CharacterCreator.Add(name, c))
            {
                MessageBox.Show("创建成功！");
                查看现有角色方法();
                ShowDetail d = new();
                d.SetText(c.GetInfo());
                d.ShowDialog();
                d.Dispose();
            }
            else
            {
                MessageBox.Show("创建已取消。");
            }
        }

        private void 创建技能_Click(object sender, EventArgs e)
        {
            Skill s = Factory.GetSkill();
            s.Id = long.Parse(Interaction.InputBox("输入技能编号", "技能编号", "0"));
            s.Name = Interaction.InputBox("输入技能名称", "技能名称", "");

            string name = Interaction.InputBox("输入技能代号以确认创建", "技能代号", "");
            if (name != "" && s.Id != 0 && s.Name != "" && SkillCreator.Add(name, s))
            {
                MessageBox.Show("创建成功！");
                查看现有技能方法();
                ShowDetail d = new();
                d.SetText(s.ToString());
                d.ShowDialog();
                d.Dispose();
            }
            else
            {
                MessageBox.Show("创建已取消。");
            }
        }

        private void 创建物品_Click(object sender, EventArgs e)
        {
            Item i = Factory.GetItem();
            i.Id = long.Parse(Interaction.InputBox("输入物品编号", "物品编号", "0"));
            i.Name = Interaction.InputBox("输入物品名称", "物品名称", "");

            string name = Interaction.InputBox("输入物品代号以确认创建", "物品代号", "");
            if (name != "" && i.Id != 0 && i.Name != "" && ItemCreator.Add(name, i))
            {
                MessageBox.Show("创建成功！");
                查看现有物品方法();
                ShowDetail d = new();
                d.SetText(i.ToString());
                d.ShowDialog();
                d.Dispose();
            }
            else
            {
                MessageBox.Show("创建已取消。");
            }
        }

        private void 删除角色_Click(object sender, EventArgs e)
        {
            if (CheckSelectedIndex && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            if (CheckSelectedIndex && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = SkillCreator.LoadedSkills.Where(ky => ky.Value.GetIdName() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
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
            if (CheckSelectedIndex && MessageBox.Show("是否删除", "删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string name = ItemCreator.LoadedItems.Where(ky => ky.Value.GetIdName() == 实际列表.Items[实际列表.SelectedIndex].ToString()).FirstOrDefault().Key ?? "";
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

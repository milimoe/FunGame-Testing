using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class CreateSkill : Form
    {
        private SkillCreator SkillCreator { get; }
        private Skill? EditSkill { get; }

        public CreateSkill(SkillCreator creator, Skill? skill = null)
        {
            InitializeComponent();
            SkillCreator = creator;
            if (skill != null)
            {
                Text = "技能编辑器";
                BtnCreate.Text = "编辑";
                EditSkill = skill;
                TextID.Text = skill.Id.ToString();
                TextCode.Text = creator.LoadedSkills.Where(kv => kv.Value.Equals(skill)).Select(kv => kv.Key).FirstOrDefault() ?? "";
                TextName.Text = skill.Name;
                ComboSkillType.Text = SkillSet.GetSkillTypeName(skill.SkillType);
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            Skill s;
            if (EditSkill != null)
            {
                s = EditSkill;
            }
            else
            {
                s = Factory.GetSkill();
            }
            string name;

            if (TextID.Text.Trim() != "" && long.TryParse(TextID.Text.Trim(), out long id))
            {
                s.Id = id;
            }
            else
            {
                MessageBox.Show("ID不能为空。");
                return;
            }

            if (TextName.Text.Trim() != "")
            {
                s.Name = TextName.Text.Trim();
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
                MessageBox.Show("技能存档标识不能为空。");
                return;
            }
            
            if (EditSkill != null)
            {
                MessageBox.Show("保存成功！");
                Dispose();
                return;
            }
            else
            {
                if (SkillCreator.Add(name, s))
                {
                    ShowDetail d = new(null, SkillCreator, null);
                    d.SetText(-1, s, s.ToString());
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
            TextCode.Text = "";
            TextName.Text = "";
            ComboSkillType.Text = "";
        }
    }
}

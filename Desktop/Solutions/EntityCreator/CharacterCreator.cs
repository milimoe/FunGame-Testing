using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class CharacterCreator : Form
    {
        private CharacterManager CharacterManager { get; }
        private Character? EditCharacter { get; }

        public CharacterCreator(CharacterManager manager, Character? character = null)
        {
            InitializeComponent();
            CharacterManager = manager;
            if (character != null)
            {
                Text = "角色编辑器";
                BtnCreate.Text = "编辑";
                EditCharacter = character;
                TextName.Text = character.Name;
                TextCode.Text = manager.LoadedCharacters.Where(kv => kv.Value == character).Select(kv => kv.Key).FirstOrDefault() ?? "";
                TextFirstName.Text = character.FirstName;
                TextNickName.Text = character.NickName;
                TextATK.Text = character.InitialATK.ToString();
                TextHP.Text = character.InitialHP.ToString();
                TextMP.Text = character.InitialMP.ToString();
                TextHR.Text = character.InitialHR.ToString();
                TextMR.Text = character.InitialMR.ToString();
                ComboPA.Text = CharacterSet.GetPrimaryAttributeName(character.PrimaryAttribute);
                TextSTR.Text = character.InitialSTR.ToString();
                TextGrowthSTR.Text = character.STRGrowth.ToString();
                TextAGI.Text = character.InitialAGI.ToString();
                TextGrowthAGI.Text = character.AGIGrowth.ToString();
                TextINT.Text = character.InitialINT.ToString();
                TextGrowthINT.Text = character.INTGrowth.ToString();
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            Character c;
            if (EditCharacter != null)
            {
                c = EditCharacter;
            }
            else
            {
                c = Factory.GetCharacter();
            }
            string name;

            if (TextName.Text.Trim() != "")
            {
                c.Name = TextName.Text.Trim();
            }
            else
            {
                MessageBox.Show("姓不能为空。");
                return;
            }
            
            if (TextCode.Text.Trim() != "")
            {
                name = TextCode.Text.Trim();
            }
            else
            {
                MessageBox.Show("角色存档标识不能为空。");
                return;
            }
            
            if (TextFirstName.Text.Trim() != "")
            {
                c.FirstName = TextFirstName.Text.Trim();
            }
            
            if (TextNickName.Text.Trim() != "")
            {
                c.NickName = TextNickName.Text.Trim();
            }
            
            if (TextATK.Text.Trim() != "" && double.TryParse(TextATK.Text.Trim(), out double atk))
            {
                c.InitialATK = atk;
            }
            
            if (TextHP.Text.Trim() != "" && double.TryParse(TextHP.Text.Trim(), out double hp))
            {
                c.InitialHP = hp;
            }
            
            if (TextMP.Text.Trim() != "" && double.TryParse(TextMP.Text.Trim(), out double mp))
            {
                c.InitialMP = mp;
            }
            
            if (TextHR.Text.Trim() != "" && double.TryParse(TextHR.Text.Trim(), out double hr))
            {
                c.InitialHR = hr;
            }
            
            if (TextMR.Text.Trim() != "" && double.TryParse(TextMR.Text.Trim(), out double mr))
            {
                c.InitialMR = mr;
            }
            
            if (ComboPA.Text.Trim() != "")
            {
                string pa = ComboPA.Text.Trim();
                if (pa == "敏捷")
                {
                    c.PrimaryAttribute = PrimaryAttribute.AGI;
                }
                else if (pa == "智力")
                {
                    c.PrimaryAttribute = PrimaryAttribute.INT;
                }
                else
                {
                    c.PrimaryAttribute = PrimaryAttribute.STR;
                }
            }

            if (TextSTR.Text.Trim() != "" && double.TryParse(TextSTR.Text.Trim(), out double str))
            {
                c.InitialSTR = str;
            }
            
            if (TextGrowthSTR.Text.Trim() != "" && double.TryParse(TextGrowthSTR.Text.Trim(), out double strg))
            {
                c.STRGrowth = strg;
            }
            
            if (TextAGI.Text.Trim() != "" && double.TryParse(TextAGI.Text.Trim(), out double agi))
            {
                c.InitialAGI = agi;
            }
            
            if (TextGrowthAGI.Text.Trim() != "" && double.TryParse(TextGrowthAGI.Text.Trim(), out double agig))
            {
                c.AGIGrowth = agig;
            }
            
            if (TextINT.Text.Trim() != "" && double.TryParse(TextINT.Text.Trim(), out double @int))
            {
                c.InitialINT = @int;
            }
            
            if (TextGrowthINT.Text.Trim() != "" && double.TryParse(TextGrowthINT.Text.Trim(), out double intg))
            {
                c.INTGrowth = intg;
            }
            
            if (TextSPD.Text.Trim() != "" && double.TryParse(TextSPD.Text.Trim(), out double spd))
            {
                c.InitialSPD = spd;
            }

            if (EditCharacter != null)
            {
                MessageBox.Show("保存成功！");
                Dispose();
                return;
            }
            else
            {
                if (CharacterManager.Add(name, c))
                {
                    ShowDetail d = new(CharacterManager, null, null);
                    d.SetText(-1, c, c.GetInfo());
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

            TextName.Text = "";
            TextCode.Text = "";
            TextFirstName.Text = "";
            TextNickName.Text = "";
            TextATK.Text = "";
            TextHP.Text = "";
            TextMP.Text = "";
            TextHR.Text = "";
            TextMR.Text = "";
            ComboPA.Text = "";
            TextSTR.Text = "";
            TextGrowthSTR.Text = "";
            TextAGI.Text = "";
            TextGrowthAGI.Text = "";
            TextINT.Text = "";
            TextGrowthINT.Text = "";
        }
    }
}

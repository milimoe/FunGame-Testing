using Milimoe.FunGame.Core.Api.Utility;
using System.ComponentModel;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public partial class SetConfigName : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FileName { get; set; } = "";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ModuleName { get; set; } = "";
        public CharacterManager CharacterManager { get; }
        public SkillManager SkillManager { get; }
        public ItemManager ItemManager { get; }

        public SetConfigName(CharacterManager cm, SkillManager sm, ItemManager im)
        {
            InitializeComponent();
            CharacterManager = cm;
            SkillManager = sm;
            ItemManager = im;
            FileName = "Module.ini";
            ModuleName = "EntityEditor";
            if (INIHelper.ExistINIFile(FileName))
            {
                ModuleName = INIHelper.ReadINI("ModuleName", "Module", FileName);
                string character = INIHelper.ReadINI("ModuleName", "Character", FileName);
                string skill = INIHelper.ReadINI("ModuleName", "Skill", FileName);
                string item = INIHelper.ReadINI("ModuleName", "Item", FileName);
                if (ModuleName.Trim() == "") ModuleName = "EntityEditor";
                else ModuleName = ModuleName.Trim();
                if (character.Trim() == "") character = "characters";
                else character = character.Trim();
                if (skill.Trim() == "") skill = "skills";
                else skill = skill.Trim();
                if (item.Trim() == "") item = "items";
                else item = item.Trim();
                CharacterManager.ModuleName = ModuleName;
                SkillManager.ModuleName = ModuleName;
                ItemManager.ModuleName = ModuleName;
                CharacterManager.ConfigName = character;
                SkillManager.ConfigName = skill;
                ItemManager.ConfigName = item;
            }
            else
            {
                INIHelper.WriteINI("ModuleName", "Module", "EntityEditor", FileName);
                INIHelper.WriteINI("ModuleName", "Character", "characters", FileName);
                INIHelper.WriteINI("ModuleName", "Skill", "skills", FileName);
                INIHelper.WriteINI("ModuleName", "Item", "items", FileName);
            }
            FormClosing += SetConfigName_FormClosing;
            TextModule.Text = ModuleName;
            TextCharacter.Text = CharacterManager.ConfigName;
            TextSkill.Text = SkillManager.ConfigName;
            TextItem.Text = ItemManager.ConfigName;
        }

        private void SetConfigName_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            ModuleName = TextModule.Text;
            CharacterManager.ConfigName = TextCharacter.Text;
            SkillManager.ConfigName = TextSkill.Text;
            ItemManager.ConfigName = TextItem.Text;
            INIHelper.WriteINI("ModuleName", "Module", ModuleName, FileName);
            INIHelper.WriteINI("ModuleName", "Character", CharacterManager.ConfigName, FileName);
            INIHelper.WriteINI("ModuleName", "Skill", SkillManager.ConfigName, FileName);
            INIHelper.WriteINI("ModuleName", "Item", ItemManager.ConfigName, FileName);
            MessageBox.Show("保存成功！");
            Hide();
        }
    }
}

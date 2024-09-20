namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    partial class CreateSkill
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TipName = new Label();
            TextID = new TextBox();
            TextName = new TextBox();
            TipID = new Label();
            BtnCreate = new Button();
            TextCode = new TextBox();
            LabelCode = new Label();
            TipSkillType = new Label();
            ComboSkillType = new ComboBox();
            SuspendLayout();
            // 
            // TipName
            // 
            TipName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipName.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipName.Location = new Point(78, 95);
            TipName.Name = "TipName";
            TipName.Size = new Size(129, 27);
            TipName.TabIndex = 0;
            TipName.Text = "技能名称";
            TipName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextID
            // 
            TextID.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextID.Font = new Font("等线", 14.25F);
            TextID.Location = new Point(213, 45);
            TextID.Name = "TextID";
            TextID.Size = new Size(201, 27);
            TextID.TabIndex = 0;
            TextID.WordWrap = false;
            // 
            // TextName
            // 
            TextName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextName.Font = new Font("等线", 14.25F);
            TextName.Location = new Point(213, 95);
            TextName.Name = "TextName";
            TextName.Size = new Size(201, 27);
            TextName.TabIndex = 1;
            TextName.WordWrap = false;
            // 
            // TipID
            // 
            TipID.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipID.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipID.Location = new Point(78, 45);
            TipID.Name = "TipID";
            TipID.Size = new Size(129, 27);
            TipID.TabIndex = 2;
            TipID.Text = "技能编号";
            TipID.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // BtnCreate
            // 
            BtnCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCreate.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            BtnCreate.Location = new Point(420, 216);
            BtnCreate.Name = "BtnCreate";
            BtnCreate.Size = new Size(103, 45);
            BtnCreate.TabIndex = 4;
            BtnCreate.Text = "创建";
            BtnCreate.UseVisualStyleBackColor = true;
            BtnCreate.Click += BtnCreate_Click;
            // 
            // TextCode
            // 
            TextCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextCode.Font = new Font("等线", 14.25F);
            TextCode.Location = new Point(199, 226);
            TextCode.Name = "TextCode";
            TextCode.Size = new Size(215, 27);
            TextCode.TabIndex = 3;
            TextCode.WordWrap = false;
            // 
            // LabelCode
            // 
            LabelCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelCode.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelCode.Location = new Point(23, 226);
            LabelCode.Name = "LabelCode";
            LabelCode.Size = new Size(170, 27);
            LabelCode.TabIndex = 34;
            LabelCode.Text = "代号（存档标识）";
            LabelCode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TipSkillType
            // 
            TipSkillType.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipSkillType.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipSkillType.Location = new Point(78, 145);
            TipSkillType.Name = "TipSkillType";
            TipSkillType.Size = new Size(129, 27);
            TipSkillType.TabIndex = 35;
            TipSkillType.Text = "技能类型";
            TipSkillType.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ComboSkillType
            // 
            ComboSkillType.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            ComboSkillType.FormattingEnabled = true;
            ComboSkillType.Items.AddRange(new object[] { "魔法", "战技", "爆发技", "被动", "物品" });
            ComboSkillType.Location = new Point(213, 141);
            ComboSkillType.Name = "ComboSkillType";
            ComboSkillType.Size = new Size(201, 33);
            ComboSkillType.TabIndex = 57;
            // 
            // CreateSkill
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(535, 273);
            Controls.Add(ComboSkillType);
            Controls.Add(TipSkillType);
            Controls.Add(TextCode);
            Controls.Add(LabelCode);
            Controls.Add(BtnCreate);
            Controls.Add(TextName);
            Controls.Add(TipID);
            Controls.Add(TextID);
            Controls.Add(TipName);
            Name = "CreateSkill";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "技能创建器";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label TipName;
        private TextBox TextID;
        private TextBox TextName;
        private Label TipID;
        private Button BtnCreate;
        private TextBox TextCode;
        private Label LabelCode;
        private Label TipSkillType;
        private ComboBox ComboSkillType;
    }
}
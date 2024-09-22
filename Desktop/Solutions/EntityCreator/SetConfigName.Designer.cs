namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    partial class SetConfigName
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
            TipSkill = new Label();
            TextCharacter = new TextBox();
            TextSkill = new TextBox();
            TipCharacter = new Label();
            BtnSave = new Button();
            TextItem = new TextBox();
            TipItem = new Label();
            TextModule = new TextBox();
            TipModule = new Label();
            SuspendLayout();
            // 
            // TipSkill
            // 
            TipSkill.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipSkill.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipSkill.Location = new Point(78, 95);
            TipSkill.Name = "TipSkill";
            TipSkill.Size = new Size(129, 27);
            TipSkill.TabIndex = 0;
            TipSkill.Text = "技能配置名";
            TipSkill.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextCharacter
            // 
            TextCharacter.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextCharacter.Font = new Font("等线", 14.25F);
            TextCharacter.Location = new Point(213, 45);
            TextCharacter.Name = "TextCharacter";
            TextCharacter.Size = new Size(201, 27);
            TextCharacter.TabIndex = 0;
            TextCharacter.WordWrap = false;
            // 
            // TextSkill
            // 
            TextSkill.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextSkill.Font = new Font("等线", 14.25F);
            TextSkill.Location = new Point(213, 95);
            TextSkill.Name = "TextSkill";
            TextSkill.Size = new Size(201, 27);
            TextSkill.TabIndex = 1;
            TextSkill.WordWrap = false;
            // 
            // TipCharacter
            // 
            TipCharacter.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipCharacter.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipCharacter.Location = new Point(78, 45);
            TipCharacter.Name = "TipCharacter";
            TipCharacter.Size = new Size(129, 27);
            TipCharacter.TabIndex = 2;
            TipCharacter.Text = "角色配置名";
            TipCharacter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // BtnSave
            // 
            BtnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnSave.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            BtnSave.Location = new Point(420, 216);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(103, 45);
            BtnSave.TabIndex = 4;
            BtnSave.Text = "保存";
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
            // 
            // TextItem
            // 
            TextItem.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextItem.Font = new Font("等线", 14.25F);
            TextItem.Location = new Point(213, 144);
            TextItem.Name = "TextItem";
            TextItem.Size = new Size(201, 27);
            TextItem.TabIndex = 6;
            TextItem.WordWrap = false;
            // 
            // TipItem
            // 
            TipItem.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipItem.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipItem.Location = new Point(78, 144);
            TipItem.Name = "TipItem";
            TipItem.Size = new Size(129, 27);
            TipItem.TabIndex = 5;
            TipItem.Text = "技能配置名";
            TipItem.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextModule
            // 
            TextModule.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextModule.Font = new Font("等线", 14.25F);
            TextModule.Location = new Point(213, 226);
            TextModule.Name = "TextModule";
            TextModule.Size = new Size(201, 27);
            TextModule.TabIndex = 7;
            TextModule.WordWrap = false;
            // 
            // TipModule
            // 
            TipModule.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipModule.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipModule.Location = new Point(78, 225);
            TipModule.Name = "TipModule";
            TipModule.Size = new Size(129, 27);
            TipModule.TabIndex = 8;
            TipModule.Text = "模组名称";
            TipModule.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SetConfigName
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(535, 273);
            Controls.Add(TipModule);
            Controls.Add(TextModule);
            Controls.Add(TextItem);
            Controls.Add(TipItem);
            Controls.Add(BtnSave);
            Controls.Add(TextSkill);
            Controls.Add(TipCharacter);
            Controls.Add(TextCharacter);
            Controls.Add(TipSkill);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SetConfigName";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "模组名称设置";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label TipSkill;
        private TextBox TextCharacter;
        private TextBox TextSkill;
        private Label TipCharacter;
        private Button BtnSave;
        private TextBox TextItem;
        private Label TipItem;
        private TextBox TextModule;
        private Label TipModule;
    }
}
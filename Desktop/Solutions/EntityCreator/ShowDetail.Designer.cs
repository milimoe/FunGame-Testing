namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    partial class ShowDetail
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
            详细内容 = new RichTextBox();
            编辑 = new Button();
            加技能 = new Button();
            加物品 = new Button();
            删物品 = new Button();
            删技能 = new Button();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            详细内容.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            详细内容.Location = new Point(12, 12);
            详细内容.Name = "richTextBox1";
            详细内容.ReadOnly = true;
            详细内容.Size = new Size(584, 513);
            详细内容.TabIndex = 0;
            详细内容.Text = "";
            // 
            // 编辑
            // 
            编辑.Font = new Font("等线", 18F, FontStyle.Regular, GraphicsUnit.Point, 134);
            编辑.Location = new Point(602, 456);
            编辑.Name = "编辑";
            编辑.Size = new Size(89, 69);
            编辑.TabIndex = 1;
            编辑.Text = "编辑";
            编辑.UseVisualStyleBackColor = true;
            编辑.Click += 编辑_Click;
            // 
            // 加技能
            // 
            加技能.Font = new Font("等线", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            加技能.Location = new Point(602, 306);
            加技能.Name = "加技能";
            加技能.Size = new Size(89, 69);
            加技能.TabIndex = 2;
            加技能.Text = "加技能";
            加技能.UseVisualStyleBackColor = true;
            加技能.Click += 加技能_Click;
            // 
            // 加物品
            // 
            加物品.Font = new Font("等线", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            加物品.Location = new Point(602, 381);
            加物品.Name = "加物品";
            加物品.Size = new Size(89, 69);
            加物品.TabIndex = 3;
            加物品.Text = "加物品";
            加物品.UseVisualStyleBackColor = true;
            加物品.Click += 加物品_Click;
            // 
            // 删物品
            // 
            删物品.Font = new Font("等线", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            删物品.Location = new Point(602, 231);
            删物品.Name = "删物品";
            删物品.Size = new Size(89, 69);
            删物品.TabIndex = 5;
            删物品.Text = "删物品";
            删物品.UseVisualStyleBackColor = true;
            删物品.Click += 删物品_Click;
            // 
            // 删技能
            // 
            删技能.Font = new Font("等线", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            删技能.Location = new Point(602, 156);
            删技能.Name = "删技能";
            删技能.Size = new Size(89, 69);
            删技能.TabIndex = 4;
            删技能.Text = "删技能";
            删技能.UseVisualStyleBackColor = true;
            删技能.Click += 删技能_Click;
            // 
            // ShowDetail
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(697, 537);
            Controls.Add(删物品);
            Controls.Add(删技能);
            Controls.Add(加物品);
            Controls.Add(加技能);
            Controls.Add(编辑);
            Controls.Add(详细内容);
            Name = "ShowDetail";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ShowDetail";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox 详细内容;
        private Button 编辑;
        private Button 加技能;
        private Button 加物品;
        private Button 删物品;
        private Button 删技能;
    }
}
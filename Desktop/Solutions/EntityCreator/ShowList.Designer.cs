namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    partial class ShowList
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
            选择表 = new ListBox();
            SuspendLayout();
            // 
            // 选择表
            // 
            选择表.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            选择表.FormattingEnabled = true;
            选择表.ItemHeight = 17;
            选择表.Location = new Point(12, 12);
            选择表.Name = "选择表";
            选择表.Size = new Size(484, 429);
            选择表.TabIndex = 0;
            选择表.MouseDoubleClick += 选择表_MouseDoubleClick;
            // 
            // Showlist
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(508, 450);
            Controls.Add(选择表);
            Name = "ShowList";
            Text = "ShowList";
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
        }

        #endregion

        private ListBox 选择表;
    }
}
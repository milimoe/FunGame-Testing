namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    partial class CreateItem
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
            BtnCreate = new Button();
            TextCode = new TextBox();
            LabelCode = new Label();
            TipItemType = new Label();
            TextName = new TextBox();
            TipID = new Label();
            TextID = new TextBox();
            TipName = new Label();
            LabelWeaponType = new Label();
            CheckboxIsWeapon = new CheckBox();
            CheckboxIsEquip = new CheckBox();
            LabelEquip = new Label();
            CheckboxIsPurchasable = new CheckBox();
            TextPrice = new TextBox();
            LabelPrice = new Label();
            CheckboxIsSellable = new CheckBox();
            LabelNextSellTime = new Label();
            CheckboxIsTradable = new CheckBox();
            LabelNextTradeTime = new Label();
            DateTimePickerSell = new DateTimePicker();
            DateTimePickerTrade = new DateTimePicker();
            ComboWeapon = new ComboBox();
            ComboEquip = new ComboBox();
            ComboItemType = new ComboBox();
            SuspendLayout();
            // 
            // BtnCreate
            // 
            BtnCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCreate.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            BtnCreate.Location = new Point(628, 412);
            BtnCreate.Name = "BtnCreate";
            BtnCreate.Size = new Size(103, 45);
            BtnCreate.TabIndex = 14;
            BtnCreate.Text = "创建";
            BtnCreate.UseVisualStyleBackColor = true;
            BtnCreate.Click += BtnCreate_Click;
            // 
            // TextCode
            // 
            TextCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextCode.Font = new Font("等线", 14.25F);
            TextCode.Location = new Point(407, 422);
            TextCode.Name = "TextCode";
            TextCode.Size = new Size(215, 27);
            TextCode.TabIndex = 13;
            TextCode.WordWrap = false;
            // 
            // LabelCode
            // 
            LabelCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelCode.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelCode.Location = new Point(231, 422);
            LabelCode.Name = "LabelCode";
            LabelCode.Size = new Size(170, 27);
            LabelCode.TabIndex = 34;
            LabelCode.Text = "代号（存档标识）";
            LabelCode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TipItemType
            // 
            TipItemType.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipItemType.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipItemType.Location = new Point(214, 128);
            TipItemType.Name = "TipItemType";
            TipItemType.Size = new Size(129, 27);
            TipItemType.TabIndex = 41;
            TipItemType.Text = "物品类型";
            TipItemType.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextName
            // 
            TextName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextName.Font = new Font("等线", 14.25F);
            TextName.Location = new Point(349, 78);
            TextName.Name = "TextName";
            TextName.Size = new Size(201, 27);
            TextName.TabIndex = 1;
            TextName.WordWrap = false;
            // 
            // TipID
            // 
            TipID.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipID.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipID.Location = new Point(214, 28);
            TipID.Name = "TipID";
            TipID.Size = new Size(129, 27);
            TipID.TabIndex = 40;
            TipID.Text = "物品编号";
            TipID.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextID
            // 
            TextID.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextID.Font = new Font("等线", 14.25F);
            TextID.Location = new Point(349, 28);
            TextID.Name = "TextID";
            TextID.Size = new Size(201, 27);
            TextID.TabIndex = 0;
            TextID.WordWrap = false;
            // 
            // TipName
            // 
            TipName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TipName.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TipName.Location = new Point(214, 78);
            TipName.Name = "TipName";
            TipName.Size = new Size(129, 27);
            TipName.TabIndex = 38;
            TipName.Text = "物品名称";
            TipName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LabelWeaponType
            // 
            LabelWeaponType.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelWeaponType.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelWeaponType.Location = new Point(272, 192);
            LabelWeaponType.Name = "LabelWeaponType";
            LabelWeaponType.Size = new Size(129, 27);
            LabelWeaponType.TabIndex = 43;
            LabelWeaponType.Text = "武器类型";
            LabelWeaponType.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CheckboxIsWeapon
            // 
            CheckboxIsWeapon.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CheckboxIsWeapon.CheckAlign = ContentAlignment.MiddleRight;
            CheckboxIsWeapon.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            CheckboxIsWeapon.ImageAlign = ContentAlignment.MiddleRight;
            CheckboxIsWeapon.Location = new Point(121, 192);
            CheckboxIsWeapon.Name = "CheckboxIsWeapon";
            CheckboxIsWeapon.Size = new Size(145, 27);
            CheckboxIsWeapon.TabIndex = 3;
            CheckboxIsWeapon.Text = "是武器";
            CheckboxIsWeapon.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxIsWeapon.UseVisualStyleBackColor = true;
            CheckboxIsWeapon.CheckedChanged += CheckboxIsWeapon_CheckedChanged;
            // 
            // CheckboxIsEquip
            // 
            CheckboxIsEquip.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CheckboxIsEquip.CheckAlign = ContentAlignment.MiddleRight;
            CheckboxIsEquip.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            CheckboxIsEquip.ImageAlign = ContentAlignment.MiddleRight;
            CheckboxIsEquip.Location = new Point(121, 232);
            CheckboxIsEquip.Name = "CheckboxIsEquip";
            CheckboxIsEquip.Size = new Size(145, 27);
            CheckboxIsEquip.TabIndex = 5;
            CheckboxIsEquip.Text = "可被装备";
            CheckboxIsEquip.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxIsEquip.UseVisualStyleBackColor = true;
            CheckboxIsEquip.CheckedChanged += CheckboxIsEquip_CheckedChanged;
            // 
            // LabelEquip
            // 
            LabelEquip.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelEquip.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelEquip.Location = new Point(272, 232);
            LabelEquip.Name = "LabelEquip";
            LabelEquip.Size = new Size(129, 27);
            LabelEquip.TabIndex = 46;
            LabelEquip.Text = "装备类型";
            LabelEquip.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CheckboxIsPurchasable
            // 
            CheckboxIsPurchasable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CheckboxIsPurchasable.CheckAlign = ContentAlignment.MiddleRight;
            CheckboxIsPurchasable.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            CheckboxIsPurchasable.ImageAlign = ContentAlignment.MiddleRight;
            CheckboxIsPurchasable.Location = new Point(121, 272);
            CheckboxIsPurchasable.Name = "CheckboxIsPurchasable";
            CheckboxIsPurchasable.Size = new Size(145, 27);
            CheckboxIsPurchasable.TabIndex = 7;
            CheckboxIsPurchasable.Text = "可供购买";
            CheckboxIsPurchasable.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxIsPurchasable.UseVisualStyleBackColor = true;
            CheckboxIsPurchasable.CheckedChanged += CheckboxIsPurchasable_CheckedChanged;
            // 
            // TextPrice
            // 
            TextPrice.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TextPrice.Font = new Font("等线", 14.25F);
            TextPrice.Location = new Point(407, 272);
            TextPrice.Name = "TextPrice";
            TextPrice.Size = new Size(201, 27);
            TextPrice.TabIndex = 8;
            TextPrice.WordWrap = false;
            // 
            // LabelPrice
            // 
            LabelPrice.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelPrice.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelPrice.Location = new Point(272, 272);
            LabelPrice.Name = "LabelPrice";
            LabelPrice.Size = new Size(129, 27);
            LabelPrice.TabIndex = 49;
            LabelPrice.Text = "售价";
            LabelPrice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CheckboxIsSellable
            // 
            CheckboxIsSellable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CheckboxIsSellable.CheckAlign = ContentAlignment.MiddleRight;
            CheckboxIsSellable.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            CheckboxIsSellable.ImageAlign = ContentAlignment.MiddleRight;
            CheckboxIsSellable.Location = new Point(121, 312);
            CheckboxIsSellable.Name = "CheckboxIsSellable";
            CheckboxIsSellable.Size = new Size(145, 27);
            CheckboxIsSellable.TabIndex = 9;
            CheckboxIsSellable.Text = "允许出售";
            CheckboxIsSellable.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxIsSellable.UseVisualStyleBackColor = true;
            CheckboxIsSellable.CheckedChanged += CheckboxIsSellable_CheckedChanged;
            // 
            // LabelNextSellTime
            // 
            LabelNextSellTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelNextSellTime.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelNextSellTime.Location = new Point(272, 312);
            LabelNextSellTime.Name = "LabelNextSellTime";
            LabelNextSellTime.Size = new Size(160, 27);
            LabelNextSellTime.TabIndex = 52;
            LabelNextSellTime.Text = "下次可出售时间";
            LabelNextSellTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CheckboxIsTradable
            // 
            CheckboxIsTradable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CheckboxIsTradable.CheckAlign = ContentAlignment.MiddleRight;
            CheckboxIsTradable.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            CheckboxIsTradable.ImageAlign = ContentAlignment.MiddleRight;
            CheckboxIsTradable.Location = new Point(121, 352);
            CheckboxIsTradable.Name = "CheckboxIsTradable";
            CheckboxIsTradable.Size = new Size(145, 27);
            CheckboxIsTradable.TabIndex = 11;
            CheckboxIsTradable.Text = "允许交易";
            CheckboxIsTradable.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxIsTradable.UseVisualStyleBackColor = true;
            CheckboxIsTradable.CheckedChanged += CheckboxIsTradable_CheckedChanged;
            // 
            // LabelNextTradeTime
            // 
            LabelNextTradeTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LabelNextTradeTime.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            LabelNextTradeTime.Location = new Point(272, 352);
            LabelNextTradeTime.Name = "LabelNextTradeTime";
            LabelNextTradeTime.Size = new Size(160, 27);
            LabelNextTradeTime.TabIndex = 55;
            LabelNextTradeTime.Text = "下次可交易时间";
            LabelNextTradeTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DateTimePickerSell
            // 
            DateTimePickerSell.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            DateTimePickerSell.Location = new Point(438, 312);
            DateTimePickerSell.Name = "DateTimePickerSell";
            DateTimePickerSell.Size = new Size(170, 27);
            DateTimePickerSell.TabIndex = 10;
            // 
            // DateTimePickerTrade
            // 
            DateTimePickerTrade.Font = new Font("等线", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            DateTimePickerTrade.Location = new Point(438, 352);
            DateTimePickerTrade.Name = "DateTimePickerTrade";
            DateTimePickerTrade.Size = new Size(170, 27);
            DateTimePickerTrade.TabIndex = 12;
            // 
            // ComboWeapon
            // 
            ComboWeapon.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            ComboWeapon.FormattingEnabled = true;
            ComboWeapon.Items.AddRange(new object[] { "单手剑", "双手重剑", "弓", "手枪", "步枪", "双持短刀", "法器", "法杖", "长柄", "拳套", "暗器" });
            ComboWeapon.Location = new Point(407, 188);
            ComboWeapon.Name = "ComboWeapon";
            ComboWeapon.Size = new Size(201, 33);
            ComboWeapon.TabIndex = 4;
            // 
            // ComboEquip
            // 
            ComboEquip.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            ComboEquip.FormattingEnabled = true;
            ComboEquip.Items.AddRange(new object[] { "魔法卡包", "武器", "防具", "鞋子", "饰品" });
            ComboEquip.Location = new Point(407, 228);
            ComboEquip.Name = "ComboEquip";
            ComboEquip.Size = new Size(201, 33);
            ComboEquip.TabIndex = 6;
            // 
            // ComboItemType
            // 
            ComboItemType.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            ComboItemType.FormattingEnabled = true;
            ComboItemType.Items.AddRange(new object[] { "魔法卡包", "武器", "防具", "鞋子", "饰品", "消耗品", "魔法卡", "收藏品", "特殊物品", "任务物品", "礼包", "其他" });
            ComboItemType.Location = new Point(349, 125);
            ComboItemType.Name = "ComboItemType";
            ComboItemType.Size = new Size(201, 33);
            ComboItemType.TabIndex = 56;
            // 
            // CreateItem
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(743, 469);
            Controls.Add(ComboItemType);
            Controls.Add(ComboEquip);
            Controls.Add(ComboWeapon);
            Controls.Add(DateTimePickerTrade);
            Controls.Add(DateTimePickerSell);
            Controls.Add(CheckboxIsTradable);
            Controls.Add(LabelNextTradeTime);
            Controls.Add(CheckboxIsSellable);
            Controls.Add(LabelNextSellTime);
            Controls.Add(CheckboxIsPurchasable);
            Controls.Add(TextPrice);
            Controls.Add(LabelPrice);
            Controls.Add(CheckboxIsEquip);
            Controls.Add(LabelEquip);
            Controls.Add(CheckboxIsWeapon);
            Controls.Add(LabelWeaponType);
            Controls.Add(TipItemType);
            Controls.Add(TextName);
            Controls.Add(TipID);
            Controls.Add(TextID);
            Controls.Add(TipName);
            Controls.Add(TextCode);
            Controls.Add(LabelCode);
            Controls.Add(BtnCreate);
            Name = "CreateItem";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "物品创建器";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button BtnCreate;
        private TextBox TextCode;
        private Label LabelCode;
        private Label TipItemType;
        private TextBox TextName;
        private Label TipID;
        private TextBox TextID;
        private Label TipName;
        private Label LabelWeaponType;
        private CheckBox CheckboxIsWeapon;
        private CheckBox CheckboxIsEquip;
        private Label LabelEquip;
        private CheckBox CheckboxIsPurchasable;
        private TextBox TextPrice;
        private Label LabelPrice;
        private CheckBox CheckboxIsSellable;
        private Label LabelNextSellTime;
        private CheckBox CheckboxIsTradable;
        private Label LabelNextTradeTime;
        private DateTimePicker DateTimePickerSell;
        private DateTimePicker DateTimePickerTrade;
        private ComboBox ComboWeapon;
        private ComboBox ComboEquip;
        private ComboBox ComboItemType;
    }
}
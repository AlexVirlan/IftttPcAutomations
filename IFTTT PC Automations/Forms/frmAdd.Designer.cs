namespace IFTTT_PC_Automations.Forms
{
    partial class frmAdd
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
            components = new System.ComponentModel.Container();
            pnlAction = new Panel();
            lblAddTag = new Label();
            pnlRtbJsonBodyBg = new Panel();
            rtbJsonBody = new RichTextBox();
            lblSelectedEvent = new Label();
            chkActionEnabled = new CheckBox();
            txtAppletName = new TextBox();
            lblAppletBeautifyJson = new Label();
            lblAppletValidateJson = new Label();
            label9 = new Label();
            label10 = new Label();
            txtActionName = new TextBox();
            label3 = new Label();
            label2 = new Label();
            pnlEvent = new Panel();
            chkEventEnabled = new CheckBox();
            cmbEventType = new ComboBox();
            txtEventPropertyValue = new TextBox();
            lblEventPropertyValue = new Label();
            txtEventName = new TextBox();
            label1 = new Label();
            label4 = new Label();
            pnlBtns = new Panel();
            btnCancel = new Button();
            btnOk = new Button();
            lblInfo = new Label();
            toolTips = new ToolTip(components);
            tagsContextMenuStrip = new ContextMenuStrip(components);
            dateTimeToolStripMenuItem = new ToolStripMenuItem();
            dateFormatToolStripMenuItem = new ToolStripMenuItem();
            timeFormatToolStripMenuItem = new ToolStripMenuItem();
            datetimeFormatToolStripMenuItem = new ToolStripMenuItem();
            pCInformationToolStripMenuItem = new ToolStripMenuItem();
            userNameToolStripMenuItem = new ToolStripMenuItem();
            pCNameToolStripMenuItem = new ToolStripMenuItem();
            randomToolStripMenuItem = new ToolStripMenuItem();
            gUIDToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            to10ToolStripMenuItem = new ToolStripMenuItem();
            to50ToolStripMenuItem = new ToolStripMenuItem();
            to100ToolStripMenuItem = new ToolStripMenuItem();
            to1000ToolStripMenuItem = new ToolStripMenuItem();
            pnlAction.SuspendLayout();
            pnlRtbJsonBodyBg.SuspendLayout();
            pnlEvent.SuspendLayout();
            pnlBtns.SuspendLayout();
            tagsContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // pnlAction
            // 
            pnlAction.BorderStyle = BorderStyle.FixedSingle;
            pnlAction.Controls.Add(lblAddTag);
            pnlAction.Controls.Add(pnlRtbJsonBodyBg);
            pnlAction.Controls.Add(lblSelectedEvent);
            pnlAction.Controls.Add(chkActionEnabled);
            pnlAction.Controls.Add(txtAppletName);
            pnlAction.Controls.Add(lblAppletBeautifyJson);
            pnlAction.Controls.Add(lblAppletValidateJson);
            pnlAction.Controls.Add(label9);
            pnlAction.Controls.Add(label10);
            pnlAction.Controls.Add(txtActionName);
            pnlAction.Controls.Add(label3);
            pnlAction.Controls.Add(label2);
            pnlAction.Location = new Point(12, 12);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(264, 325);
            pnlAction.TabIndex = 0;
            pnlAction.Visible = false;
            // 
            // lblAddTag
            // 
            lblAddTag.AutoSize = true;
            lblAddTag.Cursor = Cursors.Hand;
            lblAddTag.Font = new Font("Segoe UI", 9F, FontStyle.Underline, GraphicsUnit.Point);
            lblAddTag.Location = new Point(96, 143);
            lblAddTag.Name = "lblAddTag";
            lblAddTag.Size = new Size(49, 15);
            lblAddTag.TabIndex = 13;
            lblAddTag.Text = "Add tag";
            toolTips.SetToolTip(lblAddTag, "Insert a new tag.\r\nTags are case-insensitive.");
            lblAddTag.Click += lblAddTag_Click;
            // 
            // pnlRtbJsonBodyBg
            // 
            pnlRtbJsonBodyBg.BackColor = Color.FromArgb(17, 17, 17);
            pnlRtbJsonBodyBg.BorderStyle = BorderStyle.FixedSingle;
            pnlRtbJsonBodyBg.Controls.Add(rtbJsonBody);
            pnlRtbJsonBodyBg.Location = new Point(7, 161);
            pnlRtbJsonBodyBg.Name = "pnlRtbJsonBodyBg";
            pnlRtbJsonBodyBg.Size = new Size(248, 131);
            pnlRtbJsonBodyBg.TabIndex = 12;
            // 
            // rtbJsonBody
            // 
            rtbJsonBody.BackColor = Color.FromArgb(17, 17, 17);
            rtbJsonBody.BorderStyle = BorderStyle.None;
            rtbJsonBody.Dock = DockStyle.Fill;
            rtbJsonBody.ForeColor = Color.White;
            rtbJsonBody.Location = new Point(0, 0);
            rtbJsonBody.Name = "rtbJsonBody";
            rtbJsonBody.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbJsonBody.Size = new Size(246, 129);
            rtbJsonBody.TabIndex = 7;
            rtbJsonBody.Text = "";
            rtbJsonBody.TextChanged += rtbJsonBody_TextChanged;
            // 
            // lblSelectedEvent
            // 
            lblSelectedEvent.Location = new Point(3, 18);
            lblSelectedEvent.Name = "lblSelectedEvent";
            lblSelectedEvent.Size = new Size(256, 17);
            lblSelectedEvent.TabIndex = 2;
            lblSelectedEvent.Text = "-";
            lblSelectedEvent.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkActionEnabled
            // 
            chkActionEnabled.AutoSize = true;
            chkActionEnabled.Checked = true;
            chkActionEnabled.CheckState = CheckState.Checked;
            chkActionEnabled.Location = new Point(7, 298);
            chkActionEnabled.Name = "chkActionEnabled";
            chkActionEnabled.Size = new Size(68, 19);
            chkActionEnabled.TabIndex = 9;
            chkActionEnabled.Text = "Enabled";
            toolTips.SetToolTip(chkActionEnabled, "Add the action as enabled or disabled.");
            chkActionEnabled.UseVisualStyleBackColor = true;
            // 
            // txtAppletName
            // 
            txtAppletName.BackColor = Color.FromArgb(17, 17, 17);
            txtAppletName.BorderStyle = BorderStyle.FixedSingle;
            txtAppletName.ForeColor = Color.White;
            txtAppletName.Location = new Point(7, 112);
            txtAppletName.MaxLength = 50;
            txtAppletName.Name = "txtAppletName";
            txtAppletName.Size = new Size(248, 23);
            txtAppletName.TabIndex = 7;
            txtAppletName.TextAlign = HorizontalAlignment.Center;
            txtAppletName.TextChanged += txtAppletName_TextChanged;
            // 
            // lblAppletBeautifyJson
            // 
            lblAppletBeautifyJson.AutoSize = true;
            lblAppletBeautifyJson.Cursor = Cursors.Hand;
            lblAppletBeautifyJson.Font = new Font("Segoe UI", 9F, FontStyle.Underline, GraphicsUnit.Point);
            lblAppletBeautifyJson.Location = new Point(208, 143);
            lblAppletBeautifyJson.Name = "lblAppletBeautifyJson";
            lblAppletBeautifyJson.Size = new Size(50, 15);
            lblAppletBeautifyJson.TabIndex = 11;
            lblAppletBeautifyJson.Text = "Beautify";
            lblAppletBeautifyJson.Click += lblAppletBeautifyJson_Click;
            // 
            // lblAppletValidateJson
            // 
            lblAppletValidateJson.AutoSize = true;
            lblAppletValidateJson.Cursor = Cursors.Hand;
            lblAppletValidateJson.Font = new Font("Segoe UI", 9F, FontStyle.Underline, GraphicsUnit.Point);
            lblAppletValidateJson.Location = new Point(152, 143);
            lblAppletValidateJson.Name = "lblAppletValidateJson";
            lblAppletValidateJson.Size = new Size(48, 15);
            lblAppletValidateJson.TabIndex = 10;
            lblAppletValidateJson.Text = "Validate";
            lblAppletValidateJson.Click += lblAppletValidateJson_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 95);
            label9.Name = "label9";
            label9.Size = new Size(110, 15);
            label9.TabIndex = 5;
            label9.Text = "Applet event name:";
            toolTips.SetToolTip(label9, "Required");
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 143);
            label10.Name = "label10";
            label10.Size = new Size(83, 15);
            label10.TabIndex = 6;
            label10.Text = "JSON payload:";
            // 
            // txtActionName
            // 
            txtActionName.BackColor = Color.FromArgb(17, 17, 17);
            txtActionName.BorderStyle = BorderStyle.FixedSingle;
            txtActionName.ForeColor = Color.White;
            txtActionName.Location = new Point(7, 62);
            txtActionName.MaxLength = 50;
            txtActionName.Name = "txtActionName";
            txtActionName.Size = new Size(248, 23);
            txtActionName.TabIndex = 6;
            txtActionName.TextAlign = HorizontalAlignment.Center;
            txtActionName.TextChanged += txtActionName_TextChanged;
            // 
            // label3
            // 
            label3.Location = new Point(3, 2);
            label3.Name = "label3";
            label3.Size = new Size(256, 15);
            label3.TabIndex = 2;
            label3.Text = "Selected event:";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 45);
            label2.Name = "label2";
            label2.Size = new Size(78, 15);
            label2.TabIndex = 2;
            label2.Text = "Action name:";
            toolTips.SetToolTip(label2, "Required");
            // 
            // pnlEvent
            // 
            pnlEvent.BorderStyle = BorderStyle.FixedSingle;
            pnlEvent.Controls.Add(chkEventEnabled);
            pnlEvent.Controls.Add(cmbEventType);
            pnlEvent.Controls.Add(txtEventPropertyValue);
            pnlEvent.Controls.Add(lblEventPropertyValue);
            pnlEvent.Controls.Add(txtEventName);
            pnlEvent.Controls.Add(label1);
            pnlEvent.Controls.Add(label4);
            pnlEvent.Location = new Point(12, 12);
            pnlEvent.Name = "pnlEvent";
            pnlEvent.Size = new Size(264, 200);
            pnlEvent.TabIndex = 0;
            pnlEvent.Visible = false;
            // 
            // chkEventEnabled
            // 
            chkEventEnabled.AutoSize = true;
            chkEventEnabled.Checked = true;
            chkEventEnabled.CheckState = CheckState.Checked;
            chkEventEnabled.Location = new Point(12, 163);
            chkEventEnabled.Name = "chkEventEnabled";
            chkEventEnabled.Size = new Size(68, 19);
            chkEventEnabled.TabIndex = 4;
            chkEventEnabled.Text = "Enabled";
            toolTips.SetToolTip(chkEventEnabled, "Add the event as enabled or disabled.");
            chkEventEnabled.UseVisualStyleBackColor = true;
            // 
            // cmbEventType
            // 
            cmbEventType.BackColor = Color.FromArgb(17, 17, 17);
            cmbEventType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEventType.ForeColor = Color.White;
            cmbEventType.FormattingEnabled = true;
            cmbEventType.Location = new Point(12, 75);
            cmbEventType.Name = "cmbEventType";
            cmbEventType.Size = new Size(238, 23);
            cmbEventType.TabIndex = 2;
            cmbEventType.SelectedIndexChanged += cmbEventType_SelectedIndexChanged;
            // 
            // txtEventPropertyValue
            // 
            txtEventPropertyValue.BackColor = Color.FromArgb(17, 17, 17);
            txtEventPropertyValue.BorderStyle = BorderStyle.FixedSingle;
            txtEventPropertyValue.Enabled = false;
            txtEventPropertyValue.ForeColor = Color.White;
            txtEventPropertyValue.Location = new Point(12, 124);
            txtEventPropertyValue.MaxLength = 50;
            txtEventPropertyValue.Name = "txtEventPropertyValue";
            txtEventPropertyValue.Size = new Size(238, 23);
            txtEventPropertyValue.TabIndex = 3;
            txtEventPropertyValue.TextAlign = HorizontalAlignment.Center;
            txtEventPropertyValue.TextChanged += txtEventPropertyValue_TextChanged;
            // 
            // lblEventPropertyValue
            // 
            lblEventPropertyValue.AutoSize = true;
            lblEventPropertyValue.Enabled = false;
            lblEventPropertyValue.Location = new Point(12, 107);
            lblEventPropertyValue.Name = "lblEventPropertyValue";
            lblEventPropertyValue.Size = new Size(118, 15);
            lblEventPropertyValue.TabIndex = 4;
            lblEventPropertyValue.Text = "Event property value:";
            toolTips.SetToolTip(lblEventPropertyValue, "Required only for some event types.");
            // 
            // txtEventName
            // 
            txtEventName.BackColor = Color.FromArgb(17, 17, 17);
            txtEventName.BorderStyle = BorderStyle.FixedSingle;
            txtEventName.ForeColor = Color.White;
            txtEventName.Location = new Point(12, 26);
            txtEventName.MaxLength = 50;
            txtEventName.Name = "txtEventName";
            txtEventName.Size = new Size(238, 23);
            txtEventName.TabIndex = 1;
            txtEventName.TextAlign = HorizontalAlignment.Center;
            txtEventName.TextChanged += txtAppletNameTest_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 58);
            label1.Name = "label1";
            label1.Size = new Size(65, 15);
            label1.TabIndex = 2;
            label1.Text = "Event type:";
            toolTips.SetToolTip(label1, "Required");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 9);
            label4.Name = "label4";
            label4.Size = new Size(72, 15);
            label4.TabIndex = 2;
            label4.Text = "Event name:";
            toolTips.SetToolTip(label4, "Required");
            // 
            // pnlBtns
            // 
            pnlBtns.BorderStyle = BorderStyle.FixedSingle;
            pnlBtns.Controls.Add(btnCancel);
            pnlBtns.Controls.Add(btnOk);
            pnlBtns.Controls.Add(lblInfo);
            pnlBtns.Location = new Point(12, 343);
            pnlBtns.Name = "pnlBtns";
            pnlBtns.Size = new Size(264, 49);
            pnlBtns.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(17, 17, 17);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.Gray;
            btnCancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 120, 120);
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 64, 64);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(7, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(60, 26);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.FromArgb(17, 17, 17);
            btnOk.Cursor = Cursors.Hand;
            btnOk.Enabled = false;
            btnOk.FlatAppearance.BorderColor = Color.Gray;
            btnOk.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 120, 120);
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 64, 64);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(195, 10);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(60, 26);
            btnOk.TabIndex = 5;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;
            // 
            // lblInfo
            // 
            lblInfo.ForeColor = Color.FromArgb(255, 128, 128);
            lblInfo.Location = new Point(72, 6);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(118, 34);
            lblInfo.TabIndex = 2;
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            toolTips.SetToolTip(lblInfo, "Click here for more info.");
            lblInfo.Visible = false;
            lblInfo.Click += lblInfo_Click;
            // 
            // toolTips
            // 
            toolTips.AutomaticDelay = 26;
            toolTips.AutoPopDelay = 12000;
            toolTips.InitialDelay = 260;
            toolTips.ReshowDelay = 100;
            toolTips.ToolTipIcon = ToolTipIcon.Info;
            toolTips.ToolTipTitle = "Info";
            // 
            // tagsContextMenuStrip
            // 
            tagsContextMenuStrip.Items.AddRange(new ToolStripItem[] { dateTimeToolStripMenuItem, pCInformationToolStripMenuItem, randomToolStripMenuItem });
            tagsContextMenuStrip.Name = "tagsContextMenuStrip";
            tagsContextMenuStrip.ShowImageMargin = false;
            tagsContextMenuStrip.Size = new Size(131, 70);
            tagsContextMenuStrip.MouseEnter += tagsContextMenuStrip_MouseEnter;
            tagsContextMenuStrip.MouseLeave += tagsContextMenuStrip_MouseLeave;
            // 
            // dateTimeToolStripMenuItem
            // 
            dateTimeToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            dateTimeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dateFormatToolStripMenuItem, timeFormatToolStripMenuItem, datetimeFormatToolStripMenuItem });
            dateTimeToolStripMenuItem.Name = "dateTimeToolStripMenuItem";
            dateTimeToolStripMenuItem.Size = new Size(130, 22);
            dateTimeToolStripMenuItem.Text = "Date && time";
            // 
            // dateFormatToolStripMenuItem
            // 
            dateFormatToolStripMenuItem.Name = "dateFormatToolStripMenuItem";
            dateFormatToolStripMenuItem.Size = new Size(127, 22);
            dateFormatToolStripMenuItem.Tag = "Date";
            dateFormatToolStripMenuItem.Text = "Date";
            dateFormatToolStripMenuItem.ToolTipText = "The current date.\r\nUsing the date format from settings.";
            dateFormatToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // timeFormatToolStripMenuItem
            // 
            timeFormatToolStripMenuItem.Name = "timeFormatToolStripMenuItem";
            timeFormatToolStripMenuItem.Size = new Size(127, 22);
            timeFormatToolStripMenuItem.Tag = "Time";
            timeFormatToolStripMenuItem.Text = "Time";
            timeFormatToolStripMenuItem.ToolTipText = "The current time.\r\nUsing the time format from settings.";
            timeFormatToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // datetimeFormatToolStripMenuItem
            // 
            datetimeFormatToolStripMenuItem.Name = "datetimeFormatToolStripMenuItem";
            datetimeFormatToolStripMenuItem.Size = new Size(127, 22);
            datetimeFormatToolStripMenuItem.Tag = "DateTime";
            datetimeFormatToolStripMenuItem.Text = "Date-time";
            datetimeFormatToolStripMenuItem.ToolTipText = "The current date-time.\r\nUsing the date-time format from settings.";
            datetimeFormatToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // pCInformationToolStripMenuItem
            // 
            pCInformationToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            pCInformationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { userNameToolStripMenuItem, pCNameToolStripMenuItem });
            pCInformationToolStripMenuItem.Name = "pCInformationToolStripMenuItem";
            pCInformationToolStripMenuItem.Size = new Size(130, 22);
            pCInformationToolStripMenuItem.Text = "PC information";
            // 
            // userNameToolStripMenuItem
            // 
            userNameToolStripMenuItem.Name = "userNameToolStripMenuItem";
            userNameToolStripMenuItem.Size = new Size(130, 22);
            userNameToolStripMenuItem.Tag = "UserName";
            userNameToolStripMenuItem.Text = "User name";
            userNameToolStripMenuItem.ToolTipText = "The username of the currently logged-in user.";
            userNameToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // pCNameToolStripMenuItem
            // 
            pCNameToolStripMenuItem.Name = "pCNameToolStripMenuItem";
            pCNameToolStripMenuItem.Size = new Size(130, 22);
            pCNameToolStripMenuItem.Tag = "PcName";
            pCNameToolStripMenuItem.Text = "PC name";
            pCNameToolStripMenuItem.ToolTipText = "The name of this PC.";
            pCNameToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // randomToolStripMenuItem
            // 
            randomToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            randomToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { gUIDToolStripMenuItem, toolStripSeparator2, to10ToolStripMenuItem, to50ToolStripMenuItem, to100ToolStripMenuItem, to1000ToolStripMenuItem });
            randomToolStripMenuItem.Name = "randomToolStripMenuItem";
            randomToolStripMenuItem.Size = new Size(130, 22);
            randomToolStripMenuItem.Text = "Random";
            // 
            // gUIDToolStripMenuItem
            // 
            gUIDToolStripMenuItem.Name = "gUIDToolStripMenuItem";
            gUIDToolStripMenuItem.Size = new Size(121, 22);
            gUIDToolStripMenuItem.Tag = "GUID";
            gUIDToolStripMenuItem.Text = "GUID";
            gUIDToolStripMenuItem.ToolTipText = "A random GUID (128-bit integer, 16 bytes).\r\nEg.: 788bd1a8-dfd7-4c14-b630-f3e9a58c2cfd";
            gUIDToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(118, 6);
            // 
            // to10ToolStripMenuItem
            // 
            to10ToolStripMenuItem.Name = "to10ToolStripMenuItem";
            to10ToolStripMenuItem.Size = new Size(121, 22);
            to10ToolStripMenuItem.Tag = "Rnd10";
            to10ToolStripMenuItem.Text = "0 to 10";
            to10ToolStripMenuItem.ToolTipText = "A number from 0 to 10 (both inclusive).";
            to10ToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // to50ToolStripMenuItem
            // 
            to50ToolStripMenuItem.Name = "to50ToolStripMenuItem";
            to50ToolStripMenuItem.Size = new Size(121, 22);
            to50ToolStripMenuItem.Tag = "Rnd50";
            to50ToolStripMenuItem.Text = "0 to 50";
            to50ToolStripMenuItem.ToolTipText = "A number from 0 to 50 (both inclusive).";
            to50ToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // to100ToolStripMenuItem
            // 
            to100ToolStripMenuItem.Name = "to100ToolStripMenuItem";
            to100ToolStripMenuItem.Size = new Size(121, 22);
            to100ToolStripMenuItem.Tag = "Rnd100";
            to100ToolStripMenuItem.Text = "0 to 100";
            to100ToolStripMenuItem.ToolTipText = "A number from 0 to 100 (both inclusive).";
            to100ToolStripMenuItem.Click += TagContextMenuClick;
            // 
            // to1000ToolStripMenuItem
            // 
            to1000ToolStripMenuItem.Name = "to1000ToolStripMenuItem";
            to1000ToolStripMenuItem.Size = new Size(121, 22);
            to1000ToolStripMenuItem.Tag = "Rnd1000";
            to1000ToolStripMenuItem.Text = "0 to 1000";
            to1000ToolStripMenuItem.ToolTipText = "A number from 0 to 1000 (both inclusive).";
            to1000ToolStripMenuItem.Click += rtbJsonBody_TextChanged;
            // 
            // frmAdd
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(26, 26, 26);
            ClientSize = new Size(288, 406);
            ControlBox = false;
            Controls.Add(pnlAction);
            Controls.Add(pnlBtns);
            Controls.Add(pnlEvent);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAdd";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "IFTTT PC Automations";
            Load += frmAdd_Load;
            pnlAction.ResumeLayout(false);
            pnlAction.PerformLayout();
            pnlRtbJsonBodyBg.ResumeLayout(false);
            pnlEvent.ResumeLayout(false);
            pnlEvent.PerformLayout();
            pnlBtns.ResumeLayout(false);
            tagsContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlAction;
        private Panel pnlEvent;
        private Panel pnlBtns;
        private Button btnCancel;
        private Button btnOk;
        private TextBox txtEventName;
        private Label label4;
        private TextBox txtEventPropertyValue;
        private Label lblEventPropertyValue;
        private ComboBox cmbEventType;
        private Label label1;
        private CheckBox chkEventEnabled;
        private Label lblInfo;
        private ToolTip toolTips;
        private TextBox txtActionName;
        private Label label2;
        private TextBox txtAppletName;
        private Label lblAppletBeautifyJson;
        private Label lblAppletValidateJson;
        private Label label9;
        private Label label10;
        private CheckBox chkActionEnabled;
        private Label lblSelectedEvent;
        private Label label3;
        private ContextMenuStrip tagsContextMenuStrip;
        private ToolStripMenuItem dateTimeToolStripMenuItem;
        private ToolStripMenuItem dateFormatToolStripMenuItem;
        private ToolStripMenuItem timeFormatToolStripMenuItem;
        private ToolStripMenuItem datetimeFormatToolStripMenuItem;
        private ToolStripMenuItem pCInformationToolStripMenuItem;
        private ToolStripMenuItem userNameToolStripMenuItem;
        private ToolStripMenuItem pCNameToolStripMenuItem;
        private ToolStripMenuItem randomToolStripMenuItem;
        private ToolStripMenuItem gUIDToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem to10ToolStripMenuItem;
        private ToolStripMenuItem to50ToolStripMenuItem;
        private ToolStripMenuItem to100ToolStripMenuItem;
        private ToolStripMenuItem to1000ToolStripMenuItem;
        private Panel pnlRtbJsonBodyBg;
        private RichTextBox rtbJsonBody;
        private Label lblAddTag;
    }
}
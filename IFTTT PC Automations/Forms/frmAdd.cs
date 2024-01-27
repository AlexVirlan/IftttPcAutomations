using IFTTT_PC_Automations.CustomHelpers;
using IFTTT_PC_Automations.Entities;
using Action = IFTTT_PC_Automations.Entities.Action;

namespace IFTTT_PC_Automations.Forms
{
    public partial class frmAdd : Form
    {
        #region Variables
        private InputType _inputType;
        private string? _selectedEventName;
        private string _NL = Environment.NewLine;
        private string _duplicateName = string.Empty;
        private int[] _eventTypesWithNoValue = { 2 }; // Shutdown
        private int[] _eventTypesWithNumericOnly = { 0, 1 }; // Battery below or equal to // Battery above or equal to
        public DynamicData DynamicData { get; private set; } = new DynamicData();
        #endregion

        public frmAdd(InputType inputType, string? selectedEventName = null)
        {
            InitializeComponent();
            _inputType = inputType;
            _selectedEventName = selectedEventName;
        }

        private void frmAdd_Load(object sender, EventArgs e)
        {
            foreach (EventType eventType in (EventType[])Enum.GetValues(typeof(EventType)))
            {
                cmbEventType.Items.Add(eventType.ToReadableString());
            }

            foreach (ToolStripMenuItem menuItem in tagsContextMenuStrip.Items) { ((ToolStripDropDownMenu)menuItem.DropDown).ShowImageMargin = false; }

            if (_inputType == InputType.Event)
            {
                pnlEvent.Visible = true;
                this.Size = new Size(304, 331);
                pnlBtns.Location = new Point(12, 230);
                this.Text = "Add event - IFTTT PC Automations";
                txtEventName.Focus();
            }
            else if (_inputType == InputType.Action)
            {
                pnlAction.Visible = true;
                this.Size = new Size(304, 445);
                pnlBtns.Location = new Point(12, 343);
                this.Text = "Add action - IFTTT PC Automations";
                lblSelectedEvent.Text = _selectedEventName;
                txtActionName.Focus();
            }
        }

        private void ValidityCheck()
        {
            SetInfoLabel(string.Empty);
            bool exists = false;
            switch (_inputType)
            {
                case InputType.Event:
                    exists = Settings.Events.ContainsKey(txtEventName.Text.Trim());
                    switch (cmbEventType.SelectedIndex)
                    {
                        case 0: // Battery below or equal to
                        case 1: // Battery above or equal to
                            btnOk.Enabled = (!txtEventName.Text.Trim().INOE() && cmbEventType.SelectedIndex > -1 && !exists && !txtEventPropertyValue.Text.Trim().INOE());
                            break;

                        case 2: // Shutdown
                            if (Settings.Events.Any(e => e.Value.EventType == EventType.Shutdown))
                            {
                                btnOk.Enabled = false;
                                SetInfoLabel("A 'Shutdown' event already exists.");
                                return;
                            }
                            btnOk.Enabled = (!txtEventName.Text.Trim().INOE() && cmbEventType.SelectedIndex > -1 && !exists);
                            break;
                    }
                    if (!txtEventPropertyValue.Text.Trim().INOE())
                    {
                        KeyValuePair<string, Event> duplicateTypeAndValue = Settings.Events.Where(e => e.Value.EventType == (EventType)cmbEventType.SelectedIndex)
                            .FirstOrDefault(e => e.Value.Value.Equals(txtEventPropertyValue.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (!duplicateTypeAndValue.IsNull())
                        {
                            _duplicateName = duplicateTypeAndValue.Key;
                            btnOk.Enabled = false;
                            SetInfoLabel("Another type-value pair already exists.");
                            return;
                        }
                    }
                    break;

                case InputType.Action:
                    if (!txtAppletName.Text.Trim().INOE())
                    {
                        KeyValuePair<string, Action> duplicateApplet =
                            Settings.Events[_selectedEventName].Actions.FirstOrDefault(a => a.Value.AppletEventName == txtAppletName.Text.Trim());
                        if (!duplicateApplet.IsNull())
                        {
                            _duplicateName = duplicateApplet.Key;
                            SetInfoLabel("This applet already exists for this event.");
                        }
                    }
                    exists = Settings.Events[_selectedEventName].Actions.ContainsKey(txtActionName.Text.Trim());
                    btnOk.Enabled = (!txtActionName.Text.Trim().INOE() && !txtAppletName.Text.Trim().INOE() && !exists);
                    break;
            }
            if (exists) { SetInfoLabel($"This {_inputType.ToString().ToLower()} name already exists."); }
        }

        private void SetInfoLabel(string text = "")
        {
            bool emptyText = text.INOE();
            lblInfo.Visible = !emptyText;
            lblInfo.Text = emptyText ? string.Empty : text;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            switch (_inputType)
            {
                case InputType.Event:
                    DynamicData.Name = txtEventName.Text.Trim();
                    DynamicData.Data = new Event(chkEventEnabled.Checked, (EventType)cmbEventType.SelectedIndex, txtEventPropertyValue.Text);
                    break;

                case InputType.Action:
                    DynamicData.Name = txtActionName.Text.Trim();
                    DynamicData.Data = new Action(chkActionEnabled.Checked, txtAppletName.Text.Trim(), rtbJsonBody.Text);
                    break;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtAppletNameTest_TextChanged(object sender, EventArgs e)
        {
            ValidityCheck();
        }

        private void cmbEventType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_eventTypesWithNoValue.Contains(cmbEventType.SelectedIndex))
            {
                txtEventPropertyValue.Text = "";
                lblEventPropertyValue.Enabled = txtEventPropertyValue.Enabled = false;
            }
            else
            {
                lblEventPropertyValue.Enabled = txtEventPropertyValue.Enabled = true;
            }
            //lblEventPropertyValue.Text = ((EventType)cmbEventType.SelectedIndex).ToReadableString() + ":";
            ValidityCheck();
        }

        private void txtEventPropertyValue_TextChanged(object sender, EventArgs e)
        {
            if (_eventTypesWithNumericOnly.Contains(cmbEventType.SelectedIndex))
            {
                txtEventPropertyValue.Text = new string(txtEventPropertyValue.Text.Where(char.IsDigit).ToArray());
                txtEventPropertyValue.SetCursorToEnd();
            }
            ValidityCheck();
        }

        private void lblAppletValidateJson_Click(object sender, EventArgs e)
        {
            if (rtbJsonBody.Text.Trim().INOE()) { return; }
            Validation valid = Helpers.IsJsonValid(rtbJsonBody.Text);
            MessageBox.Show(this, $"The payload is {(valid.Valid ? "valid!" : "NOT valid." + _NL.Repeat() + $"Details: {_NL + valid.Message}")}",
                "IFTTT PC Automations - validate json", MessageBoxButtons.OK,
                (valid.Valid ? MessageBoxIcon.Information : MessageBoxIcon.Warning));
        }

        private void lblAppletBeautifyJson_Click(object sender, EventArgs e)
        {
            if (rtbJsonBody.Text.Trim().INOE()) { return; }
            (Validation valid, string? jsonData) = Helpers.BeautifyJson(rtbJsonBody.Text);
            if (valid.Valid)
            {
                rtbJsonBody.Text = jsonData;
            }
            else
            {
                MessageBox.Show(this, "The payload is NOT valid." + _NL.Repeat() + $"Details: {_NL + valid.Message}",
                    "IFTTT PC Automations - beautify json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtActionName_TextChanged(object sender, EventArgs e)
        {
            ValidityCheck();
        }

        private void txtAppletName_TextChanged(object sender, EventArgs e)
        {
            ValidityCheck();
        }

        private void lblInfo_Click(object sender, EventArgs e)
        {
            switch (lblInfo.Text)
            {
                case string shutdown when shutdown.Equals("A 'Shutdown' event already exists.", StringComparison.OrdinalIgnoreCase):
                    MessageBox.Show(this, "You can only add one 'Shutdown' event, and you already have one. " +
                        "If you want to call multiple IFTTT applets at shutdown, select your shutdown event and add a new action.",
                        "IFTTT PC Automations - validation info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case string eventOrAction when eventOrAction.Equals($"This {_inputType.ToString().ToLower()} name already exists.", StringComparison.OrdinalIgnoreCase):
                    MessageBox.Show(this, $"{_inputType} names must be unique.",
                        "IFTTT PC Automations - validation info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case string duplicateEvent when duplicateEvent.Equals("Another type-value pair already exists.", StringComparison.OrdinalIgnoreCase):
                    MessageBox.Show(this, $"There is already another event named '{_duplicateName}' with the same type and value.",
                        "IFTTT PC Automations - validation info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case string duplicateApplet when duplicateApplet.Equals("This applet already exists for this event.", StringComparison.OrdinalIgnoreCase):
                    MessageBox.Show(this, $"This applet already exists for this event." + _NL.Repeat() +
                        "But in this case, this is only a warning. You can still add it, if you want to call it with a different payload for example.",
                        "IFTTT PC Automations - validation info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void TagContextMenuClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            if (Enum.TryParse(menuItem.Tag.ToStringSafely(), ignoreCase: true, out TagType tag))
            {
                string insertText = $"{{{{{tag}}}}}";
                int selectionIndex = rtbJsonBody.SelectionStart;
                rtbJsonBody.Text = rtbJsonBody.Text.Insert(selectionIndex, insertText);
                rtbJsonBody.SelectionStart = selectionIndex + insertText.Length;
            }
            else
            {
                MessageBox.Show(this, $"An error occurred while adding the tag." + _NL.Repeat() + "Please contact the developer for help.",
                    "IFTTT PC Automations - tag error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rtbJsonBody_TextChanged(object sender, EventArgs e)
        {
            if (Settings.ColorTags) { Helpers.ColorTagsInRTB(rtbJsonBody); }
        }

        private void lblAddTag_Click(object sender, EventArgs e)
        {
            tagsContextMenuStrip.Show(Cursor.Position);
        }

        private void tagsContextMenuStrip_MouseEnter(object sender, EventArgs e)
        {
            tagsContextMenuStrip.Opacity = 1;
        }

        private void tagsContextMenuStrip_MouseLeave(object sender, EventArgs e)
        {
            tagsContextMenuStrip.Opacity = 0.6;
        }
    }
}

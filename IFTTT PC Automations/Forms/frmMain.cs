using DeviceId;
using DeviceId.Components;
using DeviceId.Encoders;
using DeviceId.Formatters;
using IFTTT_PC_Automations.CustomHelpers;
using IFTTT_PC_Automations.Entities;
using IFTTT_PC_Automations.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualBasic.Logging;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Action = IFTTT_PC_Automations.Entities.Action;

namespace IFTTT_PC_Automations.Forms
{
    public partial class frmMain : Form
    {
        #region Variables
        private List<string>? _args = null;
        private int _WM_QUERYENDSESSION = 0x11;
        private bool _DEBUG_MODE = false;
        private bool _configured = false;
        private bool _showAssemblyVersion = false;
        private bool _settingsSaved = true;
        private bool _bypassJsonBodyTextChange = false;
        private string _NL = Environment.NewLine;
        private string _selectedEventName = string.Empty;
        private string _selectedActionName = string.Empty;
        private string _moreStatusInfo = string.Empty;
        private RichTextBox _rtbTag;
        #endregion

        public frmMain(List<string>? args = null)
        {
            InitializeComponent();
            _args = args;
            if (args is not null && args.Count > 0) { Variables.AddDebugLog($"Args: {string.Join(", ", args)}"); }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menuItem in tagsContextMenuStrip.Items) { ((ToolStripDropDownMenu)menuItem.DropDown).ShowImageMargin = false; }

            Variables.DeviceFingerprint = "@" + new DeviceIdBuilder()
                .AddUserName()
                .AddMachineName()
                //.AddOsVersion()
                .AddComponent("IFTTT PC Automations", new DeviceIdComponent("Alex Vîrlan"))
                .UseFormatter(new HashDeviceIdFormatter(() => SHA256.Create(), new Base64ByteArrayEncoder()))
                .ToString() + "@";

            if (File.Exists("App.set".CombineWithStartupPath()))
            {
                FunctionResponse fr = AppSettings.Load();
                if (fr.Error)
                {
                    MessageBox.Show(this, $"Error loading settings:{_NL + fr.Message + _NL.Repeat()}Exiting...",
                        "IFTTT PC Automations", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            else
            {
                FunctionResponse fr = AppSettings.Save();
                if (fr.Error)
                {
                    MessageBox.Show(this, $"Error saving settings:{_NL + fr.Message + _NL.Repeat()}Exiting...",
                        "IFTTT PC Automations", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }

            if (Settings.IftttWebhooksKey.INOE())
            {
                btnSaveSettings.Enabled = btnBack2Main.Enabled = false;
                lblTestFirstInfo.Visible = true;
                SetView(ViewType.Settings);
            }
            else { _configured = true; }

            SetAppTitle();
            LoadEvents();

            fswSettingsFile.Path = Application.StartupPath;
            fswAppErrors.Path = Variables.ErrorsPath + "\\";

            UpdateStatus(customStatus: $"Welcome {Environment.UserName}! :)", textColor: Color.DeepSkyBlue);

            Thread tTryConnection = new Thread(TryConnection);
            tTryConnection.Start();

            Thread rLoadTextEditors = new Thread(LoadTextEditors);
            rLoadTextEditors.Start();

            //PowerStatus p = SystemInformation.PowerStatus;
            //int a = (int)(p.BatteryLifePercent * 100);
        }

        private void TryConnection()
        {
            try
            {
                Action action = new Action("IftttPcAutomationsTryConnection");
                RequestResponse? testTask = Task.Run(async () => await Helpers.IFTTTPostAsync(action)).Result;
            }
            catch (Exception) { }
        }

        private void LoadEvents()
        {
            try
            {
                dgvEvents.Rows.Clear();
                ClearActionsAndApplet();
                foreach (KeyValuePair<string, Event> kvp in Settings.Events)
                {
                    dgvEvents.Rows.Add(kvp.Value.Enabled, kvp.Key, kvp.Value.EventType.ToReadableString(), kvp.Value.Value);
                }
                UpdateStatistics(UpdateType.All);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while loading the events. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void LoadTextEditors()
        {
            try
            {
                List<TextEditor> textEditors = Helpers.GetTextEditors(onlyExisting: true);
                textEditors.ForEach(te => cmbTextEditors.Items.Add(new ComboboxItem(te.Name, te.Path.ToStringSafely())));
                pnlTextEditorsBG.Visible = textEditors.Count > 0;
                if (cmbTextEditors.Items.Count > 0) { cmbTextEditors.SelectedIndex = 0; }
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while loading text editors. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void ReloadSettings()
        {
            Statistics totalStatsTemp = Settings.TotalStatistics;
            FunctionResponse fr = AppSettings.Load();
            if (fr.Error)
            {
                MessageBox.Show(this, $"Error loading settings:{_NL + fr.Message + _NL}",
                    "IFTTT PC Automations", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Settings.TotalStatistics = totalStatsTemp;
            LoadEvents();
        }

        private void UpdateStatistics(UpdateType updateType = UpdateType.All)
        {
            switch (updateType)
            {
                case UpdateType.Events:
                    lblEventsStats.Text = $"Events ({dgvEvents.CheckedRowsCount(columnName: "EventEnabled")} enabled, {dgvEvents.Rows.Count} in total):";
                    break;

                case UpdateType.Actions:
                    lblActionsStats.Text = $"Actions ({dgvActions.CheckedRowsCount(columnName: "ActionEnabled")} enabled, {dgvActions.Rows.Count} in total):";
                    break;

                case UpdateType.All:
                    lblEventsStats.Text = $"Events ({dgvEvents.CheckedRowsCount(columnName: "EventEnabled")} enabled, {dgvEvents.Rows.Count} in total):";
                    lblActionsStats.Text = $"Actions ({dgvActions.CheckedRowsCount(columnName: "ActionEnabled")} enabled, {dgvActions.Rows.Count} in total):";
                    break;
            }
        }

        private void SetView(ViewType viewType)
        {
            pnlMain.Visible = pnlSettings.Visible = pnlLogsStats.Visible = pnlAppErrors.Visible = false;
            switch (viewType)
            {
                case ViewType.Main:
                    pnlMain.Visible = true;
                    break;

                case ViewType.Settings:
                    LoadSettings();
                    pnlSettings.Visible = true;
                    break;

                case ViewType.LogsStats:
                    LoadLogsStats();
                    pnlLogsStats.Visible = true;
                    lblStatus.Hide();
                    Thread tLogsDirStats = new Thread(() => CalculateLogsSize(LogsType.Logs));
                    tLogsDirStats.Start();
                    break;

                case ViewType.AppErrors:
                    LoadAppErrors();
                    fswAppErrors.EnableRaisingEvents = true;
                    pnlAppErrors.Visible = true;
                    break;
            }
            Refresh();
        }

        private void CalculateLogsSize(LogsType logType)
        {
            try
            {
                string path = logType == LogsType.Logs ? Variables.LogsPath : Variables.ErrorsPath;
                if (!Directory.Exists(path))
                {
                    if (logType == LogsType.Logs)
                    { lblLogsStats.SetTextSafe($"Total logs: 0  |  Total size: 0 bytes"); }
                    else if (logType == LogsType.AppErrors)
                    { lblAppErrorsStats.SetTextSafe($"Total app errors: 0  |  Total size: 0 bytes"); }
                    return;
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FileInfo[] logFiles = directoryInfo.GetFiles("*.json", SearchOption.AllDirectories);
                long size = logFiles.Sum(file => file.Length);

                if (logType == LogsType.Logs)
                { lblLogsStats.SetTextSafe($"Total logs: {logFiles.Length}  |  Total size: {size.ToPrettySize(decimalPlaces: 2)}"); }
                else if (logType == LogsType.AppErrors)
                { lblAppErrorsStats.SetTextSafe($"Total app errors: {logFiles.Length}  |  Total size: {size.ToPrettySize(decimalPlaces: 2)}"); }
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while calculating files size. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void LoadSettings()
        {
            txtApiUrl.Text = Settings.IftttApiUrl;
            txtWebhooksKey.Text = Settings.IftttWebhooksKey;
            txtDateFormat.Text = Settings.DateFormat;
            txtTimeFormat.Text = Settings.TimeFormat;
            txtDateTimeFormat.Text = Settings.DateTimeFormat;
            chkRunOnStartup.Checked = Settings.RunOnStartup;
            chkMin2Tray.Checked = Settings.MinimizeToTray;
            chkNotifyOnEvents.Checked = Settings.NotifyOnEvents;
            chkDeleteConfirmationPrompts.Checked = Settings.DeleteConfirmationPrompts;
            chkColorTags.Checked = Settings.ColorTags;
            chkBeautifyIftttErrorResponses.Checked = Settings.BeautifyIftttErrorResponses;
        }

        private void LoadLogsStats()
        {
            lblStatTotalEvents.Text = Settings.TotalStatistics.EventsFired.ToString();
            lblStatTotalActions.Text = Settings.TotalStatistics.ActionsFired.ToString();
            lblStatTotalFailedActions.Text = Settings.TotalStatistics.ActionsFailed.ToString();
            lblStatSessionEvents.Text = Variables.SessionStatistics.EventsFired.ToString();
            lblStatSessionActions.Text = Variables.SessionStatistics.ActionsFired.ToString();
            lblStatSessionFailedActions.Text = Variables.SessionStatistics.ActionsFailed.ToString();
            txtLogsPath.Text = Variables.LogsPath + Path.DirectorySeparatorChar.ToString();

            LoadUnsavedLogs();
        }

        private void LoadUnsavedLogs()
        {
            try
            {
                dgvUnsavedLogs.Rows.Clear();
                dgvUnsavedLogsActions.Rows.Clear();
                txtUnsavedLogMessage.Text = txtUnsavedLogStackTrace.Text = string.Empty;
                foreach (UnsavedLog unsavedLog in Variables.UnsavedLogs)
                {
                    dgvUnsavedLogs.Rows.Add(unsavedLog.Log.DateTime.ToString("yyyy.MM.dd HH:mm:ss"), unsavedLog.Log.EventName, unsavedLog.Log.ActionsResults.Count);
                }
                lblStatUnsavedLogs.Text = $"Unsaved logs ({Variables.UnsavedLogs.Count}):";
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while loading the unsaved logs. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void LoadAppErrors()
        {
            try
            {
                txtAppErrorsPath.Text = Variables.ErrorsPath + Path.DirectorySeparatorChar.ToString();
                cmbAppErrorsYear.Items.Clear();
                cmbAppErrorsMonth.Items.Clear();
                cmbAppErrorsDay.Items.Clear();
                txtAppErrorsFile.Clear();

                Thread tAppErrorsDirStats = new Thread(() => CalculateLogsSize(LogsType.AppErrors));
                tAppErrorsDirStats.Start();

                if (!Directory.Exists(Variables.ErrorsPath)) { return; }
                foreach (string dir in Directory.GetDirectories(Variables.ErrorsPath))
                {
                    string dirName = new DirectoryInfo(dir).Name; // Path.GetDirectoryName(dir);
                    if (new Regex(@"^\d{4}$").Match(dirName).Success) { cmbAppErrorsYear.Items.Add(dirName); }
                }
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while loading the app errors. Please contact the developer.", textColor: Color.Tomato);
            }
        }

        private void SetAppTitle(bool showAssemblyVersion = false)
        {
            this.Text = "IFTTT PC Automations - " +
                $"{Helpers.GetAppVersion(fieldCount: (showAssemblyVersion ? 4 : 2), addVPrefix: true, addRuntimeMode: true)} - AvA.Soft";
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == _WM_QUERYENDSESSION)
            {
                MessageBox.Show("WARNING: this is a logoff, shutdown, or reboot");
            }
            base.WndProc(ref m);
        }

        private void trayShow_Click(object sender, EventArgs e)
        {
            notifyIcon_MouseDoubleClick(sender, null);
        }

        private void trayExit_Click(object sender, EventArgs e)
        {
            int unsavedLogsCount = Variables.UnsavedLogs.Count;
            if (unsavedLogsCount > 0 &&
                MessageBox.Show(this, $"There {(unsavedLogsCount == 1 ? "is one unsaved log" : $"are {unsavedLogsCount} unsaved logs")}." + _NL +
                $"Do you want to try to save {(unsavedLogsCount == 1 ? "it" : "them")} again?",
                "IFTTT PC Automations - unsaved logs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TrySaveLogs();
            }

            string exitMsg = _settingsSaved ? "Are you sure that you want to exit the app?" :
                "The events and/or actions were modified but not saved." + _NL.Repeat() + "Are you sure that you want to discard them and exit?";

            if (MessageBox.Show(this, exitMsg, "IFTTT PC Automations - exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_settingsSaved)
                {
                    SaveSettings();
                }
                else
                {
                    ReloadSettings();
                    SaveSettings();
                }
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (Settings.MinimizeToTray && WindowState == FormWindowState.Minimized) { Hide(); }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void btnTestApplet_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtAppletNameTest.Text.Trim().INOE())
                {
                    MessageBox.Show(this, $"The applet event name is required!", "IFTTT PC Automations - test applet",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                btnTestApplet.Enabled = false;
                Action action = new Action(txtAppletNameTest.Text.Trim(), rtbJsonBodyTest.Text);
                RequestResponse? testTask = Task.Run(async () => await Helpers.IFTTTPostAsync(action)).Result;
                if (testTask is null)
                {
                    MessageBox.Show(this, $"Could not perform the test... :(", "IFTTT PC Automations - test applet error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show(this,
                    $"Error: {testTask.Error.ToYN() + _NL}" +
                    $"Status code: {(int)testTask.StatusCode} ({testTask.StatusCode})" + _NL +
                    $"Response: {_NL + (testTask.Error ? Helpers.TryExtractMessage(testTask.Body) : testTask.Body)}",
                    "IFTTT PC Automations - test applet result", MessageBoxButtons.OK,
                    (testTask.Error ? MessageBoxIcon.Error : MessageBoxIcon.Information));

                if (chkTestAppletClearOnSuccess.Checked && !testTask.Error) { txtAppletNameTest.Text = rtbJsonBodyTest.Text = ""; }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error:{_NL + ex.Message}",
                    "IFTTT PC Automations - test applet error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestApplet.Enabled = true;
            }
        }

        private void txtAppletNameTest_TextChanged(object sender, EventArgs e)
        {
            btnTestApplet.Enabled = (txtAppletNameTest.Text.Trim().Length > 0);
            btnTestApplet.Text = $"Test {txtAppletNameTest.Text.Trim()}";
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (!_settingsSaved)
            {
                MessageBox.Show(this, $"The events and/or actions were modified but not saved." + _NL +
                    "Press the 'Save' button to save them, or the 'Discard' button to revert to the already saved ones." + _NL.Repeat() +
                    "Explanation: this warning is meant to prevent you from saving unwanted modified events and/or actions - " +
                    "since by saving the app settings you also save the events and actions.", "IFTTT PC Automations - settings not saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetView(ViewType.Settings);
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            string iftttApiUrlBackup = "", iftttWebhooksKeyBackup = "";
            try
            {
                if (txtApiUrl.Text.Trim().INOE())
                {
                    MessageBox.Show(this, $"The IFTTT API URL is required!", "IFTTT PC Automations - test connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtWebhooksKey.Text.Trim().INOE())
                {
                    MessageBox.Show(this, $"The IFTTT Webhooks Key is required!", "IFTTT PC Automations - test connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string apiUrl = txtApiUrl.Text.Trim();
                if (!apiUrl.EndsWith('/')) { apiUrl += "/"; }
                if (!Helpers.IsUrlValid(apiUrl))
                {
                    MessageBox.Show(this, $"The IFTTT API URL is not valid.", "IFTTT PC Automations - test connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnTestConnection.Enabled = false;
                iftttApiUrlBackup = Settings.IftttApiUrl;
                iftttWebhooksKeyBackup = Settings.IftttWebhooksKey;
                Settings.IftttApiUrl = apiUrl;
                Settings.IftttWebhooksKey = txtWebhooksKey.Text.Trim();
                RequestResponse? testTask = Task.Run(async () => await Helpers.IFTTTPostAsync(new Action("IFTTT-PC-Automations"))).Result;
                if (testTask is null)
                {
                    MessageBox.Show(this, $"Could not perform the test... :(", "IFTTT PC Automations - test connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show(this,
                    $"Error: {testTask.Error.ToYN() + _NL}" +
                    $"Status code: {(int)testTask.StatusCode} ({testTask.StatusCode})" + _NL +
                    $"Response: {_NL + (testTask.Error ? Helpers.TryExtractMessage(testTask.Body) : testTask.Body)}",
                    "IFTTT PC Automations - test connection result", MessageBoxButtons.OK,
                    (testTask.Error ? MessageBoxIcon.Error : MessageBoxIcon.Information));

                lblTestFirstInfo.Visible = testTask.Error;
                btnSaveSettings.Enabled = !testTask.Error;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error:{_NL + ex.Message}",
                    "IFTTT PC Automations - test connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Settings.IftttApiUrl = iftttApiUrlBackup;
                Settings.IftttWebhooksKey = iftttWebhooksKeyBackup;
                btnTestConnection.Enabled = true;
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            bool ctrl = Form.ModifierKeys == Keys.Control;
            if (txtApiUrl.Text.Trim().INOE())
            {
                MessageBox.Show(this, $"The IFTTT API URL is required!", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtWebhooksKey.Text.Trim().INOE())
            {
                MessageBox.Show(this, $"The IFTTT Webhooks Key is required!", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string apiUrl = txtApiUrl.Text.Trim();
            if (!apiUrl.EndsWith('/')) { apiUrl += "/"; }
            if (!Helpers.IsUrlValid(apiUrl))
            {
                MessageBox.Show(this, $"The IFTTT API URL is not valid.", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtDateFormat.Text.Trim().INOE())
            {
                MessageBox.Show(this, $"The date format is required!", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtTimeFormat.Text.Trim().INOE())
            {
                MessageBox.Show(this, $"The time format is required!", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtDateTimeFormat.Text.Trim().INOE())
            {
                MessageBox.Show(this, $"The date-time format is required!", "IFTTT PC Automations - save settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Settings.IftttApiUrl = apiUrl;
            Settings.IftttWebhooksKey = txtWebhooksKey.Text.Trim();
            Settings.DateFormat = txtDateFormat.Text.Trim();
            Settings.TimeFormat = txtTimeFormat.Text.Trim();
            Settings.DateTimeFormat = txtDateTimeFormat.Text.Trim();
            Settings.RunOnStartup = chkRunOnStartup.Checked;
            Settings.MinimizeToTray = chkMin2Tray.Checked;
            Settings.NotifyOnEvents = chkNotifyOnEvents.Checked;
            Settings.DeleteConfirmationPrompts = chkDeleteConfirmationPrompts.Checked;
            Settings.ColorTags = chkColorTags.Checked;
            Settings.BeautifyIftttErrorResponses = chkBeautifyIftttErrorResponses.Checked;
            FunctionResponse frSave = AppSettings.Save();
            if (frSave.Error)
            {
                MessageBox.Show(this, $"Error saving settings:{_NL + frSave.Message + _NL.Repeat()}Please try again or contact the developer.",
                    "IFTTT PC Automations", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _settingsSaved = true;

            Helpers.SetStartup(active: Settings.RunOnStartup, args: "minimized");

            if (!ctrl)
            {
                MessageBox.Show(this, $"Settings saved successfully!",
                    "IFTTT PC Automations - settings saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            btnBack2Main.Enabled = true;

            if (!_configured)
            {
                _configured = true;
                btnBack2Main_Click(sender, e);
                return;
            }

            if (ctrl) { btnBack2Main_Click(sender, e); }
        }

        private void lblShowWhKey_MouseDown(object sender, MouseEventArgs e)
        {
            txtWebhooksKey.PasswordChar = '\0';
        }

        private void lblShowWhKey_MouseUp(object sender, MouseEventArgs e)
        {
            txtWebhooksKey.PasswordChar = '#';
        }

        private void lblDefaultApiUrl_Click(object sender, EventArgs e)
        {
            txtApiUrl.Text = Variables.IftttDefaultApiUrl;
        }

        private void btnBack2Main_Click(object sender, EventArgs e)
        {
            SetView(ViewType.Main);
            btnSaveSettings.Enabled = true;
            lblTestFirstInfo.Visible = false;
        }

        private void ForceToTestConnection()
        {
            btnSaveSettings.Enabled = false;
            lblTestFirstInfo.Visible = true;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (_args is not null)
            {
                foreach (string arg in _args)
                {
                    switch (arg)
                    {
                        case string mnm when mnm.Contains("minimized", StringComparison.OrdinalIgnoreCase):
                            Hide();
                            break;

                        case string dbg when dbg.Contains("debug", StringComparison.OrdinalIgnoreCase):
                            _DEBUG_MODE = true;
                            MessageBox.Show(this, "Debug mode: ON", "IFTTT PC Automations - debug mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            lblCopyDebugLog.Visible = true;
                            break;
                    }
                }
            }
            this.Opacity = 1;
        }

        private void lblTestAppletValidateJson_Click(object sender, EventArgs e)
        {
            if (rtbJsonBodyTest.Text.Trim().INOE()) { return; }
            Validation valid = Helpers.IsJsonValid(rtbJsonBodyTest.Text);
            MessageBox.Show(this, $"The payload is {(valid.Valid ? "valid!" : "NOT valid." + _NL.Repeat() + $"Details: {_NL + valid.Message}")}",
                "IFTTT PC Automations - validate json", MessageBoxButtons.OK,
                (valid.Valid ? MessageBoxIcon.Information : MessageBoxIcon.Warning));
        }

        private void lblTestAppletBeautifyJson_Click(object sender, EventArgs e)
        {
            if (rtbJsonBodyTest.Text.Trim().INOE()) { return; }
            (Validation valid, string? jsonData) = Helpers.BeautifyJson(rtbJsonBodyTest.Text);
            if (valid.Valid)
            {
                rtbJsonBodyTest.Text = jsonData;
            }
            else
            {
                MessageBox.Show(this, "The payload is NOT valid." + _NL.Repeat() + $"Details: {_NL + valid.Message}",
                    "IFTTT PC Automations - beautify json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void frmMain_DoubleClick(object sender, EventArgs e)
        {
            SetAppTitle(_showAssemblyVersion.Invert());
        }

        private void btnAddEvent_Click(object sender, EventArgs e)
        {
            try
            {
                DynamicData? dynamicData = GetUserInput(InputType.Event);
                if (dynamicData is null || dynamicData.Data is null) { return; }

                Event newEvent = (Event)dynamicData.Data;
#pragma warning disable 8604
                Settings.Events.Add(dynamicData.Name, newEvent);
#pragma warning restore 8604
                dgvEvents.Rows.Add(newEvent.Enabled, dynamicData.Name, newEvent.EventType.ToReadableString(), newEvent.Value);

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Events);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while adding the event. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnAddAction_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvEvents.Rows.Count == 0)
                {
                    MessageBox.Show(this, $"Please add an event first.", "IFTTT PC Automations - add action",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (dgvEvents.SelectedRows.Count == 0)
                {
                    MessageBox.Show(this, $"Please select an event first.", "IFTTT PC Automations - add action",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DynamicData? dynamicData = GetUserInput(InputType.Action, _selectedEventName);
                if (dynamicData is null || dynamicData.Data is null) { return; }

                Action newAction = (Action)dynamicData.Data;
                Settings.Events[_selectedEventName].Actions.Add(dynamicData.Name, newAction);
                dgvActions.Rows.Add(newAction.Enabled, dynamicData.Name, newAction.AppletEventName);

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Actions);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while adding the action. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void ClearActionsAndApplet()
        {
            dgvActions.Rows.Clear();
            txtAppletName.Text = string.Empty; //rtbJsonBody.Text = "";
            ChangeRtbJsonBodyText(string.Empty);
        }

        private void txtEventPropertyValue_TextChanged(object sender, EventArgs e)
        {
            if (sender is not null && !((TextBox)sender).Modified) { return; }

            txtEventPropertyValue.Text = new string(txtEventPropertyValue.Text.Where(char.IsDigit).ToArray());
            txtEventPropertyValue.SetCursorToEnd();

            Settings.Events[_selectedEventName].Value = txtEventPropertyValue.Text;
            dgvEvents.SelectedRows[0].Cells["EventPropertyValue"].Value = txtEventPropertyValue.Text;
            SetSavedSettings(false);
        }

        private DynamicData? GetUserInput(InputType inputType, string? selectedEventName = null)
        {
            using (frmAdd frmAdd = new frmAdd(inputType, selectedEventName))
            {
                this.Opacity = 0.64;
                DialogResult dialogResult = frmAdd.ShowDialog(this);
                this.Opacity = 1;
                if (dialogResult == DialogResult.OK) { return frmAdd.DynamicData; }
                else { return null; }
            }
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
                ChangeRtbJsonBodyText(jsonData); //rtbJsonBody.Text = jsonData;
            }
            else
            {
                MessageBox.Show(this, "The payload is NOT valid." + _NL.Repeat() + $"Details: {_NL + valid.Message}",
                    "IFTTT PC Automations - beautify json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetInfoLabel(string? text = null)
        {
            if (text is null || text.INOE()) { lblInfo.Text = string.Empty; }
            lblInfo.Text = text;
        }

#pragma warning disable 8602, 8604
        private void fswSettingsFile_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name.INOE() || !e.Name.Equals("App.set", StringComparison.OrdinalIgnoreCase)) { return; }
            FunctionResponse fr = AppSettings.Save();
            if (fr.Error)
            {
                MessageBox.Show(this, $"I detected that the settings file (App.set) has been deleted. :(" + _NL.Repeat() +
                    $"I tried to save the current settings but I encountered an error: {_NL + fr.Message + _NL.Repeat()}" +
                    $"Please try to manually save again later or contact the developer for help.",
                    "Settings file deleted - IFTTT PC Automations", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void fswSettingsFile_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                if (e.OldName.INOE() || e.Name.INOE() || !e.OldName.Equals("App.set", StringComparison.OrdinalIgnoreCase)) { return; }
                File.Move(e.Name, e.OldName, overwrite: true); // check startup issue for path
            }
            catch (Exception) { }
        }
#pragma warning restore 8602, 8604

        private void dgvEvents_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ClearActionsAndApplet();
                if (dgvEvents.SelectedRows.Count == 0)
                {
                    //btnDeleteSelectedEvent.Enabled = btnDeleteAllEvents.Enabled = btnEnableDisableEvent.Enabled = false;
                    pnlEventsButtons.Enabled = false;
                    btnEnableDisableEvent.Text = "Enable/Disable";
                    lblEventPropertyValue.Text = "Event property value:";
                    txtEventPropertyValue.Enabled = false;
                    txtEventPropertyValue.Text = string.Empty;
                    return;
                }

                _selectedEventName = dgvEvents.SelectedRows[0].Cells["EventName"].Value.ToStringSafely();
                Event @event = Settings.Events[_selectedEventName];

                btnEnableDisableEvent.Text = @event.Enabled ? "Disable" : "Enable";

                txtEventPropertyValue.Text = @event.Value;
                foreach (KeyValuePair<string, Action> kvp in @event.Actions)
                {
                    dgvActions.Rows.Add(kvp.Value.Enabled, kvp.Key, kvp.Value.AppletEventName);
                }
                UpdateStatistics(UpdateType.Actions);

                //btnDeleteSelectedEvent.Enabled = btnDeleteAllEvents.Enabled = btnEnableDisableEvent.Enabled = true;
                pnlEventsButtons.Enabled = true;
                lblEventPropertyValue.Text = @event.EventType.ToReadableString() + ":";
                txtEventPropertyValue.Enabled = lblEventPropertyValue.Enabled = @event.EventType.IsEventValueAvailable();
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while reading event data. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void SetSavedSettings(bool saved)
        {
            _settingsSaved = saved;
            btnSave.Enabled = btnDiscard.Enabled = !saved;
            if (saved)
            {
                SetInfoLabel();
                pnlBG.BackColor = Color.FromArgb(26, 26, 26);
            }
            else
            {
                if (chkAutoSave.Checked)
                {
                    SaveSettings();
                    SetSavedSettings(true);
                }
                else
                {
                    SetInfoLabel("The data was modified. Remember to save before exiting.");
                    pnlBG.BackColor = Color.FromArgb(64, 26, 26);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            FunctionResponse frSave = AppSettings.Save();
            if (frSave.Error)
            {
                MessageBox.Show(this, $"Error saving settings:{_NL + frSave.Message + _NL.Repeat()}Please try again or contact the developer.",
                    "IFTTT PC Automations - save settings error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SetSavedSettings(!frSave.Error);
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure that you want to discard all modified events and/or actions?",
                "IFTTT PC Automations - discard modifications", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ReloadSettings();
                SetSavedSettings(true);
            }
        }

        private void SaveSettings()
        {
            FunctionResponse frSave = AppSettings.Save();
            if (frSave.Error)
            {
                MessageBox.Show(this, $"Error saving settings:{_NL + frSave.Message + _NL.Repeat()}Please try again or contact the developer.",
                    "IFTTT PC Automations - save settings error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvActions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count == 0 || dgvActions.SelectedRows.Count == 0)
            {
                //btnDeleteSelectedAction.Enabled = btnDeleteAllActions.Enabled = btnEnableDisableAction.Enabled = btnRunCustomActions.Enabled = false;
                pnlActionsButtons.Enabled = false;
                txtAppletName.Enabled = rtbJsonBody.Visible = pnlJsonPayloadActionsBg.Enabled = false;
                btnEnableDisableAction.Text = "Enable/Disable";
                return;
            }

            _selectedActionName = dgvActions.SelectedRows[0].Cells["ActionName"].Value.ToStringSafely();

            Action action = Settings.Events[_selectedEventName].Actions[_selectedActionName];
            txtAppletName.Text = action.AppletEventName;
            ChangeRtbJsonBodyText(action.JSONPayload); //rtbJsonBody.Text = action.JSONPayload;
            btnEnableDisableAction.Text = action.Enabled ? "Disable" : "Enable";

            //btnDeleteSelectedAction.Enabled = btnDeleteAllActions.Enabled = btnEnableDisableAction.Enabled = btnRunCustomActions.Enabled = true;
            pnlActionsButtons.Enabled = true;
            txtAppletName.Enabled = rtbJsonBody.Visible = pnlJsonPayloadActionsBg.Enabled = true;
        }

        private void txtAppletName_TextChanged(object sender, EventArgs e)
        {
            if (sender is not null && !((TextBox)sender).Modified) { return; }

            Settings.Events[_selectedEventName].Actions[_selectedActionName].AppletEventName = txtAppletName.Text;
            dgvActions.SelectedRows[0].Cells["AppletName"].Value = txtAppletName.Text;

            SetSavedSettings(false);
        }

        private void ChangeRtbJsonBodyText(string? str = null)
        {
            _bypassJsonBodyTextChange = true;
            if (str is null) { str = string.Empty; } // str ??= string.Empty;
            rtbJsonBody.Text = str;
            _bypassJsonBodyTextChange = false;
        }

        private void btnDeleteSelectedEvent_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.DeleteConfirmationPrompts &&
                    MessageBox.Show(this, "Are you sure that you want to delete the selected event?" + _NL.Repeat() + _selectedEventName,
                    "IFTTT PC Automations - delete event", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                { return; }

                dgvEvents.Rows.RemoveAt(dgvEvents.SelectedRows[0].Index);
                Settings.Events.Remove(_selectedEventName);

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Events);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while deleting the event. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnDeleteAllEvents_Click(object sender, EventArgs e)
        {
            if (Settings.DeleteConfirmationPrompts &&
                MessageBox.Show(this, "Are you sure that you want to delete all events?",
                "IFTTT PC Automations - delete all events", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            { return; }

            dgvEvents.Rows.Clear();
            Settings.Events.Clear();

            SetSavedSettings(false);
            UpdateStatistics(UpdateType.All);
        }

        private void btnEnableDisableEvent_Click(object sender, EventArgs e)
        {
            try
            {
                bool enabled = dgvEvents.SelectedRows[0].Cells["EventEnabled"].Value.ToBoolSafely();
                enabled.Invert();
                dgvEvents.SelectedRows[0].Cells["EventEnabled"].Value = enabled;
                Settings.Events[_selectedEventName].Enabled = enabled;

                btnEnableDisableEvent.Text = enabled ? "Disable" : "Enable";

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Events);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while modifying the event. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnDeleteSelectedAction_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.DeleteConfirmationPrompts &&
                    MessageBox.Show(this, "Are you sure that you want to delete the selected action?" + _NL.Repeat() + _selectedActionName,
                    "IFTTT PC Automations - delete action", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                { return; }

                dgvActions.Rows.RemoveAt(dgvActions.SelectedRows[0].Index);
                Settings.Events[_selectedEventName].Actions.Remove(_selectedActionName);

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Actions);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while deleting the action. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnDeleteAllActions_Click(object sender, EventArgs e)
        {
            if (Settings.DeleteConfirmationPrompts &&
                MessageBox.Show(this, "Are you sure that you want to delete all actions?",
                "IFTTT PC Automations - delete all actions", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            { return; }

            dgvActions.Rows.Clear();
            Settings.Events[_selectedEventName].Actions.Clear();

            SetSavedSettings(false);
            UpdateStatistics(UpdateType.Actions);
        }

        private void btnEnableDisableAction_Click(object sender, EventArgs e)
        {
            try
            {
                bool enabled = dgvActions.SelectedRows[0].Cells["ActionEnabled"].Value.ToBoolSafely();
                enabled.Invert();
                dgvActions.SelectedRows[0].Cells["ActionEnabled"].Value = enabled;
                Settings.Events[_selectedEventName].Actions[_selectedActionName].Enabled = enabled;

                btnEnableDisableAction.Text = enabled ? "Disable" : "Enable";

                SetSavedSettings(false);
                UpdateStatistics(UpdateType.Actions);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while modifying the action. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            pnlSettingsButtons.Enabled = !chkAutoSave.Checked;
            if (chkAutoSave.Checked && !_settingsSaved)
            {
                SaveSettings();
                SetSavedSettings(true);
            }
        }

        private void lblDefaultDateFormat_Click(object sender, EventArgs e)
        {
            txtDateFormat.Text = Variables.DefaultDateFormat;
        }

        private void lblDefaultTimeFormat_Click(object sender, EventArgs e)
        {
            txtTimeFormat.Text = Variables.DefaultTimeFormat;
        }

        private void lblDefaultDateTimeFormat_Click(object sender, EventArgs e)
        {
            txtDateTimeFormat.Text = Variables.DefaultDateTimeFormat;
        }

        private void txtApiUrl_TextChanged(object sender, EventArgs e)
        {
#pragma warning disable 8602
            //if (new StackFrame(2, true).GetMethod().Name.Equals("set_Text", StringComparison.OrdinalIgnoreCase)) { return; }
#pragma warning restore 8602
            if (sender is not null && !((TextBox)sender).Modified) { return; }
            lblApiUrl.ForeColor = txtApiUrl.Text.Trim().INOE() ? Color.Red : Color.White;
            ForceToTestConnection();
        }

        private void txtWebhooksKey_TextChanged(object sender, EventArgs e)
        {
#pragma warning disable 8602
            //if (new StackFrame(2, true).GetMethod().Name.Equals("set_Text", StringComparison.OrdinalIgnoreCase)) { return; }
#pragma warning restore 8602
            if (sender is not null && !((TextBox)sender).Modified) { return; }
            lblWebhooksKey.ForeColor = txtWebhooksKey.Text.Trim().INOE() ? Color.Red : Color.White;
            ForceToTestConnection();
        }

        private void txtDateFormat_TextChanged(object sender, EventArgs e)
        {
            lblDateFormat.ForeColor = txtDateFormat.Text.Trim().INOE() ? Color.Red : Color.White;
        }

        private void txtTimeFormat_TextChanged(object sender, EventArgs e)
        {
            lblTimeFormat.ForeColor = txtTimeFormat.Text.Trim().INOE() ? Color.Red : Color.White;
        }

        private void txtDateTimeFormat_TextChanged(object sender, EventArgs e)
        {
            lblDateTimeFormat.ForeColor = txtDateTimeFormat.Text.Trim().INOE() ? Color.Red : Color.White;
        }

        private void lblAddTag_Click(object sender, EventArgs e)
        {
            ShowTagsContextMenu(rtbJsonBody);
        }

        private void ShowTagsContextMenu(RichTextBox textBox)
        {
            _rtbTag = textBox;
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

        private void lblAddTagTest_Click(object sender, EventArgs e)
        {
            ShowTagsContextMenu(rtbJsonBodyTest);
        }

        private void InsertTag(TagType tag)
        {
            try
            {
                if (_rtbTag is null) { return; }
                string insertText = $"{{{{{tag}}}}}";
                int selectionIndex = _rtbTag.SelectionStart;
                _rtbTag.Text = _rtbTag.Text.Insert(selectionIndex, insertText);
                _rtbTag.SelectionStart = selectionIndex + insertText.Length;
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while inserting the tag. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void lblCopyToTest_Click(object sender, EventArgs e)
        {
            if (txtAppletName.Text.Trim().INOE()) { return; }
            txtAppletNameTest.Text = txtAppletName.Text;
            rtbJsonBodyTest.Text = rtbJsonBody.Text;
        }

        private void TagContextMenuClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            if (Enum.TryParse(menuItem.Tag.ToStringSafely(), ignoreCase: true, out TagType tag))
            {
                InsertTag(tag);
            }
            else
            {
                MessageBox.Show(this, $"An error occurred while adding the tag." + _NL.Repeat() + "Please contact the developer for help.",
                    "IFTTT PC Automations - tag error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomTagContextMenuClick(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                // tag sa fie cu sau fara spatii??
                // adauga dar tag aici (key din dictionar) si in helpers la process schimba cu valoarea

                if (_rtbTag is null) { return; }
                string insertText = $"{{{{{tag}}}}}";
                int selectionIndex = _rtbTag.SelectionStart;
                _rtbTag.Text = _rtbTag.Text.Insert(selectionIndex, insertText);
                _rtbTag.SelectionStart = selectionIndex + insertText.Length;


            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while . Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void rtbJsonBody_TextChanged(object sender, EventArgs e)
        {
            if (Settings.ColorTags) { Helpers.ColorTagsInRTB(rtbJsonBody); }

            if (_bypassJsonBodyTextChange) { return; }
            //if (sender is not null && !((RichTextBox)sender).Modified) { return; }

            Settings.Events[_selectedEventName].Actions[_selectedActionName].JSONPayload = rtbJsonBody.Text;
            SetSavedSettings(false);
        }

        private void rtbJsonBodyTest_TextChanged(object sender, EventArgs e)
        {
            if (Settings.ColorTags) { Helpers.ColorTagsInRTB(rtbJsonBodyTest); }
        }

        private void btnRunCustomActions_Click(object sender, EventArgs e)
        {
            runActionsContextMenuStrip.Show(btnRunCustomActions, new Point(0, btnRunCustomActions.Height));
        }

        private async void RunCustomActionContextMenuClick(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                if (Enum.TryParse(menuItem.Tag.ToStringSafely(), ignoreCase: true, out ActionsType executeType))
                {
                    // run in new bg thread + lock UI btn

                    Dictionary<string, RequestResponse>? results = await Helpers.ExecuteEventActions(_selectedEventName, executeType, _selectedActionName);
                    if (results is null)
                    {
                        UpdateStatus("The action could not be completed.", Color.Tomato);
                        return;
                    }
                    int failedActions = 0;
                    string info = $"Results for {_selectedEventName}:" + _NL.Repeat();
                    foreach (KeyValuePair<string, RequestResponse> kvpResult in results)
                    {
                        info += $"• {kvpResult.Key} ({(kvpResult.Value.Error ? $"error - {kvpResult.Value.StatusCode}" : "success")})";
                        if (kvpResult.Value.Error)
                        {
                            failedActions++;
                            info += _NL + Helpers.TryExtractMessage(kvpResult.Value.Body).RemoveLineBreaks();
                        }
                        info += _NL.Repeat();
                    }
                    UpdateStatus($"Ran actions: {results.Count} ({failedActions} failed)  |  Click here for more info.",
                        (failedActions > 0 ? Color.Tomato : Color.DeepSkyBlue), info);

                }
                else
                {
                    MessageBox.Show(this, $"An error occurred while running the actions." + _NL.Repeat() + "Please contact the developer for help.",
                        "IFTTT PC Automations - run actions error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while running action. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void tmrUpdateStatus_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus(string? customStatus = null, Color? textColor = null, string? moreInfo = null)
        {
            tmrUpdateStatus.Stop();

            if (customStatus.INOE())
            {
                string str = string.Empty;
                str += $"Total events fired: {Settings.TotalStatistics.EventsFired}  |  ";
                str += $"Total actions fired: {Settings.TotalStatistics.ActionsFired} ({Settings.TotalStatistics.ActionsFailed} failed)  |  ";
                str += $"Session events fired: {Variables.SessionStatistics.EventsFired}  |  ";
                str += $"Session actions fired: {Variables.SessionStatistics.ActionsFired} ({Variables.SessionStatistics.ActionsFailed} failed)";
                int unsavedLogs = Variables.UnsavedLogs.Count;
                if (unsavedLogs > 0) { str += $"  |  Unsaved logs: {unsavedLogs}"; }
                if (Variables.SessionAppErrorsCount > 0) { str += $"  |  Session errors: {Variables.SessionAppErrorsCount}"; }
                lblStatus.Text = str;
            }
            else { lblStatus.Text = customStatus; }

            if (textColor is null) { lblStatus.ForeColor = Color.White; }
            else { lblStatus.ForeColor = (Color)textColor; }

            if (moreInfo.INOE())
            {
                _moreStatusInfo = string.Empty;
                toolTips.SetToolTip(lblStatus, "Updates every 10 seconds.");
            }
            else
            {
                _moreStatusInfo = moreInfo;
                toolTips.SetToolTip(lblStatus, "Click for more info.");
            }

            tmrUpdateStatus.Start();
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
            if (!_moreStatusInfo.INOE())
            { MessageBox.Show(this, _moreStatusInfo, "IFTTT PC Automations - status info", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private void btnLogsStats_Click(object sender, EventArgs e)
        {
            SetView(ViewType.LogsStats);
        }

        private void btnLogsStatsBack_Click(object sender, EventArgs e)
        {
            SetView(ViewType.Main);
            lblStatus.Show();
        }

        private void btnCopyLogsPath_Click(object sender, EventArgs e)
        {
            SetClipboardText(txtLogsPath.Text);
        }

        private void SetClipboardText(string text)
        {
            if (text.INOE()) { return; }
            Clipboard.Clear();
            Clipboard.SetText(text);
        }

        private void btnOpenLogsPath_Click(object sender, EventArgs e)
        {
            OpenExplorerFolder(txtLogsPath.Text);
        }

        private void OpenExplorerFolder(string path)
        {
            if (path.INOE()) { return; }
            if (Directory.Exists(path)) { Process.Start("explorer.exe", path); }
        }

        private void dgvUnsavedLogs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUnsavedLogs.SelectedRows.Count == 0)
            {
                dgvUnsavedLogsActions.Rows.Clear();
                lblStatUnsavedLogsActions.Text = $"Ran actions (0):";
                return;
            }
            string eventName = dgvUnsavedLogs.SelectedRows[0].Cells["Event"].Value.ToStringSafely();
            UnsavedLog? unsavedLog = Variables.UnsavedLogs.FirstOrDefault(u => u.Log.EventName.Equals(eventName));
            if (unsavedLog is null) { return; }

            dgvUnsavedLogsActions.Rows.Clear();
            foreach (KeyValuePair<string, RequestResponse> kvpActionResult in unsavedLog.Log.ActionsResults)
            {
                dgvUnsavedLogsActions.Rows.Add(
                    kvpActionResult.Key,
                    $"{(kvpActionResult.Value.Error ? "Error" : "Success")}",
                    $"[{kvpActionResult.Value.StatusCode}] " +
                    (kvpActionResult.Value.Error ? Helpers.TryExtractMessage(kvpActionResult.Value.Body) : kvpActionResult.Value.Body.ToStringSafely()).RemoveLineBreaks());
            }
            txtUnsavedLogMessage.Text = unsavedLog.Message;
            txtUnsavedLogStackTrace.Text = unsavedLog.StackTrace;
            lblStatUnsavedLogsActions.Text = $"Ran actions ({dgvUnsavedLogsActions.Rows.Count}):";
        }

        private void btnUnsavedLogsSaveSelected_Click(object sender, EventArgs e)
        {
            if (dgvUnsavedLogs.Rows.Count == 0)
            {
                MessageBox.Show(this, "There are no unsaved logs.",
                    "IFTTT PC Automations - info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dgvUnsavedLogs.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Please select an unsaved log first.",
                    "IFTTT PC Automations - info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TrySaveLogs(dgvUnsavedLogs.SelectedRows[0].Cells["Event"].Value.ToStringSafely());
            //LoadUnsavedLogs();
        }

        private void btnUnsavedLogsSaveAll_Click(object sender, EventArgs e)
        {
            if (Variables.UnsavedLogs.Count == 0)
            {
                MessageBox.Show(this, "There are no unsaved logs.",
                    "IFTTT PC Automations - info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TrySaveLogs();
            //LoadUnsavedLogs();
        }

        private void btnUnsavedLogsDiscardAll_Click(object sender, EventArgs e)
        {
            if (Variables.UnsavedLogs.Count == 0)
            {
                MessageBox.Show(this, "There are no unsaved logs.",
                    "IFTTT PC Automations - info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Are you sure that you want to discard all the unsaved logs?",
                "IFTTT PC Automations - discard unsaved logs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Variables.UnsavedLogs.Clear();
                LoadUnsavedLogs();
            }
        }

        private void TrySaveLogs(string? specificLogByEventName = null)
        {
            try
            {
                List<UnsavedLog> tempUnsavedLogs = new List<UnsavedLog>();
                if (specificLogByEventName.INOE())
                {
                    tempUnsavedLogs = new List<UnsavedLog>(Variables.UnsavedLogs);
                    Variables.UnsavedLogs.Clear();
                }
                else
                {
                    UnsavedLog? unsavedLog = Variables.UnsavedLogs.FirstOrDefault(u => u.Log.EventName.Equals(specificLogByEventName));
                    if (unsavedLog is not null)
                    {
                        Variables.UnsavedLogs.Remove(unsavedLog);
                        tempUnsavedLogs.Add(unsavedLog);
                    }
                }

                int savedLogs = 0;
                int tmpLogs = tempUnsavedLogs.Count;
                tempUnsavedLogs.ForEach(ul =>
                {
                    if (Helpers.SaveLog(ul.Log)) { savedLogs++; }
                });

                if (savedLogs == 0)
                {
                    MessageBox.Show(this, $"{(tmpLogs == 1 ? "The log could not" : "No logs could")} be saved. :(" + _NL +
                        "Please contact the developer if this issue persists.",
                        "IFTTT PC Automations - unsaved logs result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string msg = specificLogByEventName.INOE() ?
                        $"{(savedLogs == tmpLogs ? "" : "Only ")}{savedLogs} out of {tmpLogs} {(savedLogs > 1 ? "logs were" : "log was")} saved successfully!" :
                        $"The selected log for the '{dgvUnsavedLogs.SelectedRows[0].Cells["Event"].Value.ToStringSafely()}' event was saved successfully!";
                    MessageBox.Show(this, msg, "IFTTT PC Automations - unsaved logs result", MessageBoxButtons.OK,
                        (savedLogs == tmpLogs ? MessageBoxIcon.Information : MessageBoxIcon.Warning));
                    LoadUnsavedLogs();
                    Thread tLogsDirStats = new Thread(() => CalculateLogsSize(LogsType.Logs));
                    tLogsDirStats.Start();
                }
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while saving the log. Check the 'App Errors' section.", textColor: Color.Tomato);
            }

            //List<UnsavedLog> tempUnsavedLogs = new List<UnsavedLog>(Variables.UnsavedLogs); // <<< ???
            //Variables.UnsavedLogs.Clear();
            //tempUnsavedLogs.ForEach(a => Helpers.SaveLog(a.Log));

            //int tmpLogs = tempUnsavedLogs.Count;
            //int savedLogs = tmpLogs - Variables.UnsavedLogs.Count;
            //if (savedLogs == 0)
            //{
            //    MessageBox.Show(this, "No logs could be saved. :(" + _NL + "Please contact the developer if this issue persists.",
            //        "IFTTT PC Automations - unsaved logs result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
            //else
            //{
            //    // if ???
            //    MessageBox.Show(this,
            //        $"{(savedLogs == tmpLogs ? "" : "Only ")}{savedLogs} out of {tmpLogs} {(tmpLogs > 1 ? "logs were" : "log was")} saved successfully!",
            //        "IFTTT PC Automations - unsaved logs result", MessageBoxButtons.OK,
            //        (savedLogs == tmpLogs ? MessageBoxIcon.Information : MessageBoxIcon.Warning));
            //}
        }

        private void lblBeautifyIftttErrorResponsesInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "If enabled, the IFTTT error responses will be converted from this JSON format: " + _NL +
                @"{""errors"":[{""message"":""First error.""},{""message"":""Second error.""}]}" + _NL.Repeat() +
                "Into a more human readable format like: " + _NL +
                "First error. Second error." + _NL.Repeat(3) +
                "Note: this will aftect the places where you can view the responses (like unsaved logs, test results), and also the 'Body' property of the logs.",
                "IFTTT PC Automations - info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnTsttttttttttttttttttt_Click(object sender, EventArgs e)
        {
            Variables.TESTING_BOOL.Invert();
            btnTsttttttttttttttttttt.Text = Variables.TESTING_BOOL.ToString();

            //SendNotification("Battery low", "Event fired");

            //Clipboard.Clear();
            //Clipboard.SetText(Variables.DebugLogs);

            //tagsContextMenuStrip.Items.Add("Custom", null, TagContextMenuClick);
            //var vvv = tagsContextMenuStrip.Items["Custom"] as ToolStripMenuItem;
            //var xxx = (ToolStripMenuItem)tagsContextMenuStrip.Items["Custom"];
            //vvv.DropDownItems.Add("cevaaaaaaaa");

            Settings.CustomTags.Add("CT1", "ct 1");
            Settings.CustomTags.Add("CT2", "ct 2");
            ProcessCustomTags();

            //ContextMenuStrip menu = new ContextMenuStrip();
            //ToolStripMenuItem item, submenu;

            //submenu = new ToolStripMenuItem();
            //((ToolStripDropDownMenu)submenu.DropDown).ShowImageMargin = false;
            //submenu.Text = "Custom";

            //item = new ToolStripMenuItem();
            //item.Text = "asd 1";
            ////((ToolStripDropDownMenu)item.DropDown).ShowImageMargin = false;
            //submenu.DropDownItems.Add(item);

            //item = new ToolStripMenuItem();
            //item.Text = "qwe 2";
            ////((ToolStripDropDownMenu)item.DropDown).ShowImageMargin = false;
            //submenu.DropDownItems.Add(item);

            //tagsContextMenuStrip.Items.Add(submenu);

            //foreach (ToolStripMenuItem menuItem in tagsContextMenuStrip.Items) { ((ToolStripDropDownMenu)menuItem.DropDown).ShowImageMargin = false; }


            try
            {
                //throw new Exception("ceva");
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
            }
        }

        private void ProcessCustomTags()
        {
            try
            {
                if (tagsContextMenuStrip.Items.ContainsKey("Custom")) { tagsContextMenuStrip.Items.RemoveByKey("Custom"); }
                if (Settings.CustomTags.Count == 0) { return; }
                ToolStripMenuItem item = new ToolStripMenuItem();
                ToolStripMenuItem submenu = new ToolStripMenuItem();
                ((ToolStripDropDownMenu)submenu.DropDown).ShowImageMargin = false;
                submenu.Text = "Custom";
                foreach (KeyValuePair<string, string> kvpCT in Settings.CustomTags)
                {
                    item = new ToolStripMenuItem();
                    item.Text = kvpCT.Key;
                    //item.Tag = kvpCT.Value;
                    item.Click += CustomTagContextMenuClick;
                    submenu.DropDownItems.Add(item);
                }
                tagsContextMenuStrip.Items.Add(submenu);
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while processing custom tags. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnDeleteAllLogs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure that you want to delete all the logs?" + _NL.Repeat() +
                "Please note that this will delete ALL the files and directories from the 'LOGS' directory.",
                "IFTTT PC Automations - delete all logs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteAllLogs(LogsType.Logs);
            }
        }

        private void DeleteAllLogs(LogsType logType)
        {
            try
            {
                string path = logType == LogsType.Logs ? Variables.LogsPath : Variables.ErrorsPath;
                if (!Directory.Exists(path)) { return; }
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (FileInfo file in directoryInfo.EnumerateFiles()) { file.Delete(); }
                foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories()) { dir.Delete(recursive: true); }
                Thread tLogsDirStats = new Thread(() => CalculateLogsSize(logType));
                tLogsDirStats.Start();
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while deleting the log. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void SendNotification(string title, string description = "")
        {
            try
            {
                if (!Settings.NotifyOnEvents) { return; }
                if (title.INOE() && description.INOE()) { return; }
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(description)
                    .Show();
            }
            catch (Exception ex)
            {
                Helpers.SaveAppError(ex);
                UpdateStatus(customStatus: "An error occurred while sending the notification. Check the 'App Errors' section.", textColor: Color.Tomato);
            }
        }

        private void btnAppErrors_Click(object sender, EventArgs e)
        {
            SetView(ViewType.AppErrors);
        }

        private void btnCopyAppErrorsPath_Click(object sender, EventArgs e)
        {
            SetClipboardText(txtAppErrorsPath.Text);
        }

        private void btnOpenAppErrorsPath_Click(object sender, EventArgs e)
        {
            OpenExplorerFolder(txtAppErrorsPath.Text);
        }

        private void btnDeleteAllAppErrors_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure that you want to delete all the app errors?" + _NL.Repeat() +
                "Please note that this will delete ALL the files and directories from the 'ERRORS' directory.",
                "IFTTT PC Automations - delete all app errors", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteAllLogs(LogsType.AppErrors);
                LoadAppErrors();
            }
        }

        private void lblCopyDebugLog_Click(object sender, EventArgs e)
        {
            SetClipboardText(Variables.DebugLogs);
        }

        private void btnAppErrorsBack_Click(object sender, EventArgs e)
        {
            fswAppErrors.EnableRaisingEvents = false;
            SetView(ViewType.Main);
        }

        private void cmbAppErrorsYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbAppErrorsMonth.Items.Clear();
            cmbAppErrorsDay.Items.Clear();
            txtAppErrorsFile.Clear();
            if (cmbAppErrorsYear.SelectedIndex < 0) { return; }

            string dirPath = Path.Combine(Variables.ErrorsPath, cmbAppErrorsYear.SelectedItem.ToStringSafely());
            if (!Directory.Exists(dirPath)) { return; }

            foreach (string dir in Directory.GetDirectories(dirPath))
            {
                string dirName = new DirectoryInfo(dir).Name; // Path.GetDirectoryName(dir);
                if (new Regex(@"^\d{2}$").Match(dirName).Success) { cmbAppErrorsMonth.Items.Add(dirName); }
            }
        }

        private void cmbAppErrorsMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbAppErrorsDay.Items.Clear();
            txtAppErrorsFile.Clear();
            if (cmbAppErrorsMonth.SelectedIndex < 0) { return; }

            string dirPath = Path.Combine(Variables.ErrorsPath, cmbAppErrorsYear.SelectedItem.ToStringSafely(), cmbAppErrorsMonth.SelectedItem.ToStringSafely());
            if (!Directory.Exists(dirPath)) { return; }

            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            FileInfo[] files = directoryInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.FullName);
                if (new Regex(@"^\d{2}$").Match(fileName).Success) { cmbAppErrorsDay.Items.Add(fileName); }
            }
        }

        private void cmbAppErrorsDay_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtAppErrorsFile.Clear();
            if (cmbAppErrorsDay.SelectedIndex < 0) { return; }

            string filePath = Path.Combine(Variables.ErrorsPath,
                cmbAppErrorsYear.SelectedItem.ToStringSafely(),
                cmbAppErrorsMonth.SelectedItem.ToStringSafely(),
                cmbAppErrorsDay.SelectedItem.ToStringSafely()) + ".json";
            if (!File.Exists(filePath)) { return; }
            txtAppErrorsFile.Text = File.ReadAllText(filePath);
        }

        private void btnOpenAppErrorFile_Click(object sender, EventArgs e)
        {
            ComboboxItem? textEditor = cmbTextEditors.SelectedItem as ComboboxItem;
            if (textEditor is null) { return; }
            string filePath = Path.Combine(Variables.ErrorsPath,
                cmbAppErrorsYear.SelectedItem.ToStringSafely(),
                cmbAppErrorsMonth.SelectedItem.ToStringSafely(),
                cmbAppErrorsDay.SelectedItem.ToStringSafely()) + ".json";
            if (!File.Exists(filePath))
            {
                MessageBox.Show(this, "The file does not exist.",
                    "IFTTT PC Automations - file missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(textEditor.Value, $"\"{filePath}\"");
        }

        private void fswAppErrors_Deleted(object sender, FileSystemEventArgs e)
        {
            ReloadAppErrors();
        }

        private void fswAppErrors_Renamed(object sender, RenamedEventArgs e)
        {
            ReloadAppErrors();
        }

        private void ReloadAppErrors()
        {
            MessageBox.Show(this, "The contents of the 'Errors' directory were modified externally or manually." + _NL +
                "I will reload / refresh the app errors.",
                "IFTTT PC Automations - reloading app errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            LoadAppErrors();
        }

        private void lblContactDev_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "If you have any issues or want to report some bugs, please use the GitHub issues page of this project.",
                "IFTTT PC Automations - contacting developer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Process.Start(Variables.AppWebsite);
            Process.Start(new ProcessStartInfo(Variables.AppWebsite) { UseShellExecute = true });
        }
    }
}
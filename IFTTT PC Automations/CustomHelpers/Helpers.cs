using IFTTT_PC_Automations.Entities;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Action = IFTTT_PC_Automations.Entities.Action;

namespace IFTTT_PC_Automations.CustomHelpers
{
    public static class Helpers
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async static Task<Dictionary<string, RequestResponse>?> ExecuteEventActions(string eventName, ActionsType actionsType, string? specificActionName = null)
        {
            if (eventName.INOE() || (actionsType == ActionsType.Specific && specificActionName.INOE())) { return null; }
            if (!Settings.Events.ContainsKey(eventName)) { return null; }

            Dictionary<string, Action> actions = new Dictionary<string, Action>();
            switch (actionsType)
            {
                //case ActionsType.Enabled:
                //    actions = Settings.Events[eventName].Actions.Where(a => a.Value.Enabled).ToDictionary(x => x.Key, x => x.Value);
                //    break;

                //case ActionsType.Disabled:
                //    actions = Settings.Events[eventName].Actions.Where(a => !a.Value.Enabled).ToDictionary(x => x.Key, x => x.Value);
                //    break;

                case ActionsType.Enabled:
                case ActionsType.Disabled:
                    bool enabled = actionsType == ActionsType.Enabled;
                    actions = Settings.Events[eventName].Actions.Where(a => a.Value.Enabled == enabled).ToDictionary(x => x.Key, x => x.Value);
                    break;

                case ActionsType.All:
                    actions = Settings.Events[eventName].Actions;
                    break;

                case ActionsType.Specific:
                    actions.Add(specificActionName, Settings.Events[eventName].Actions[specificActionName]);
                    break;
            }

            if (actions.Count == 0) { return new Dictionary<string, RequestResponse>(); }
            Settings.TotalStatistics.EventsFired++;
            Variables.SessionStatistics.EventsFired++;

            Dictionary<string, RequestResponse> responses = new Dictionary<string, RequestResponse>();
            foreach (KeyValuePair<string, Action> kvpAction in actions)
            {
                RequestResponse requestResponse = await IFTTTPostAsync(kvpAction.Value);
                responses.Add(kvpAction.Key, requestResponse);
                
                Settings.TotalStatistics.ActionsFired++;
                Variables.SessionStatistics.ActionsFired++;
                if (requestResponse.Error)
                {
                    Settings.TotalStatistics.ActionsFailed++;
                    Variables.SessionStatistics.ActionsFailed++;
                }
            }
            SaveLog(new Log(eventName, responses));

            return responses;
        }

        public async static Task<RequestResponse> IFTTTPostAsync(Action action)
        {
            if (action.AppletEventName.INOE()) { return new RequestResponse(error: true, body: "The 'AppletEventName' can't be empty."); }
            if (action.JSONPayload.INOE()) { action.JSONPayload = "{}"; }
            else { action.JSONPayload = ProcessTags(action.JSONPayload); }
            HttpResponseMessage response = await _httpClient.PostAsync(
                $"{Settings.IftttApiUrl + action.AppletEventName}/json/with/key/{Settings.IftttWebhooksKey}",
                new StringContent(action.JSONPayload, Encoding.UTF8, "application/json"));

            string responseString = await response.Content.ReadAsStringAsync();
            return new RequestResponse(
                appletName: action.AppletEventName,
                statusCode: response.StatusCode,
                body: responseString,
                error: (response.StatusCode != HttpStatusCode.OK));
        }

        public static bool SaveLog(Log log)
        {
            try
            {
                DateTime dtNow = DateTime.Now;
                string dsc = Path.DirectorySeparatorChar.ToString();
                string logFilePath = $"{Variables.LogsPath + dsc + dtNow.Year + dsc}{dtNow:MM}{dsc}";
                Directory.CreateDirectory(logFilePath);
                logFilePath += $"{dtNow:dd}.json";

                foreach(KeyValuePair<string, RequestResponse> kvpActionsResult in log.ActionsResults)
                {
                    if (kvpActionsResult.Value.Error)
                    { kvpActionsResult.Value.Body = Helpers.TryExtractMessage(kvpActionsResult.Value.Body); }
                }

                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine(log.ToJsonString(Formatting.None) + Environment.NewLine);
                }
                return true;
            }
            catch (Exception ex)
            {
                Variables.UnsavedLogs.Add(new UnsavedLog(log, ex.Message, ex.StackTrace));
                return false;
            }
        }

        public static void SaveAppError(Exception exception,
            [CallerMemberName] string cmn = "", [CallerFilePath] string cfp = "", [CallerLineNumber] int cln = 0)
        {
            try
            {
                DateTime dtNow = DateTime.Now;
                string dsc = Path.DirectorySeparatorChar.ToString();
                string errorFilePath = $"{Variables.ErrorsPath + dsc + dtNow.Year + dsc}{dtNow:MM}{dsc}";
                Directory.CreateDirectory(errorFilePath);
                errorFilePath += $"{dtNow:dd}.json";

                AppError appError = new AppError(exception, new Entities.MethodInfo(cmn, cfp, cln));
                using (StreamWriter sw = File.AppendText(errorFilePath))
                {
                    sw.WriteLine(appError.ToJsonString(Formatting.None) + Environment.NewLine);
                }
                Variables.SessionAppErrorsCount++;
            }
            catch (Exception ex)
            {

            }
        }

        public static string ProcessTags(string str)
        {
            if (str.INOE()) { return string.Empty; }

            foreach (TagType tag in (TagType[])Enum.GetValues(typeof(TagType)))
            {
                switch (tag)
                {
                    case TagType.Date:
                        str = str.Replace($"{{{{{tag}}}}}", DateTime.Now.ToString(Settings.DateFormat), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.Time:
                        str = str.Replace($"{{{{{tag}}}}}", DateTime.Now.ToString(Settings.TimeFormat), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.DateTime:
                        str = str.Replace($"{{{{{tag}}}}}", DateTime.Now.ToString(Settings.DateTimeFormat), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.UserName:
                        str = str.Replace($"{{{{{tag}}}}}", Environment.UserName, StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.PcName:
                        str = str.Replace($"{{{{{tag}}}}}", Environment.MachineName, StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.GUID:
                        str = str.Replace($"{{{{{tag}}}}}", Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.Rnd10:
                        str = str.Replace($"{{{{{tag}}}}}", GetRandomInt(min: 0, max: 10).ToString(), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.Rnd50:
                        str = str.Replace($"{{{{{tag}}}}}", GetRandomInt(min: 0, max: 50).ToString(), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.Rnd100:
                        str = str.Replace($"{{{{{tag}}}}}", GetRandomInt(min: 0, max: 100).ToString(), StringComparison.OrdinalIgnoreCase);
                        break;

                    case TagType.Rnd1000:
                        str = str.Replace($"{{{{{tag}}}}}", GetRandomInt(min: 0, max: 1000).ToString(), StringComparison.OrdinalIgnoreCase);
                        break;
                }
            }

            // for each custom tag

            return str;
        }

        public static void ColorTagsInRTB(RichTextBox rtb)
        {
            if (rtb.Text.Trim().INOE()) { return; }

            int selStart = rtb.SelectionStart;
            rtb.SelectAll();
            rtb.SelectionColor = Color.White;
            rtb.DeselectAll();
            rtb.SelectionStart = selStart;
            rtb.SelectionLength = 0;

            foreach (TagType tag in (TagType[])Enum.GetValues(typeof(TagType)))
            {
                rtb.HighlightText($"{{{{{tag}}}}}", caseSensitive: false, Color.DeepSkyBlue);
            }
        }

        public static int GetRandomInt(int min = 0, int max = 10)
        {
            if (min == max) { return min; }
            if (min > max) { (min, max) = (max, min); }
            return new Random().Next(min, max);
        }

        public static bool IsUrlValid(string url, UriKind uriKind = UriKind.Absolute)
        {
            return Uri.TryCreate(url, uriKind, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static Validation IsJsonValid(string jsonString)
        {
            try
            {
                JObject.Parse(jsonString);
                return new Validation(valid: true);
            }
            catch (Exception ex)
            {
                return new Validation(valid: false, message: ex.Message);
            }
        }

        public static (Validation validation, string? jsonData) BeautifyJson(string jsonString)
        {
            try
            {
                return (new Validation(valid: true), JToken.Parse(jsonString).ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                return (new Validation(valid: false, message: ex.Message), null);
            }
        }

        public static FunctionResponse SetStartup(bool active = true, string args = "")
        {
            RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk is null) { return new FunctionResponse(error: true, message: "The registry key is null."); }
            if (active) { rk.SetValue("IFTTT PC Automations", $@"""{Application.ExecutablePath}""" + (args.INOE() ? "" : $" {args}")); }
            else { rk.DeleteValue("IFTTT PC Automations", throwOnMissingValue: false); }
            return new FunctionResponse(error: false);
        }

        public static string GetAppVersion(int fieldCount = 4, bool addVPrefix = true, bool addRuntimeMode = true)
        {
            bool isDebugMode = false;
            if (fieldCount < 1) { fieldCount = 1; }
            if (fieldCount > 4) { fieldCount = 4; }
#if DEBUG
            isDebugMode = true;
#endif
            return (addVPrefix ? "v." : "") + Assembly.GetExecutingAssembly().GetName().Version?.ToString(fieldCount) +
                (addRuntimeMode ? (isDebugMode ? "-D" : "-R") : "");
        }

        public static string TryExtractMessage(string? iftttResponse)
        {
            try
            {
                if (!Settings.BeautifyIftttErrorResponses) { return iftttResponse; }
                if (iftttResponse.INOE()) { return iftttResponse; }
                IftttErrorResponse? iftttErrorResponse = JsonConvert.DeserializeObject<IftttErrorResponse>(iftttResponse);
                if (iftttErrorResponse is null) { return iftttResponse; }
                return string.Join(" ", iftttErrorResponse.Errors.Select(s => s.Message));
            }
            catch (Exception)
            {
                return iftttResponse;
            }
        }

        public static List<TextEditor> GetTextEditors(bool onlyExisting = true)
        {
            List<TextEditor> textEditors = new List<TextEditor>();

            #region VS Code
            string vsCodeReg = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\vscode\shell\open\command", "", "").ToStringSafely();
            MatchCollection vscMatches = Regex.Matches(vsCodeReg, @"""(.*Code\.exe)""");
            string vscPath = string.Empty;
            if (vscMatches.Count > 0) { vscPath = vscMatches[0].Groups[1].Value;  }
            textEditors.Add(new TextEditor("Visual Studio Code", File.Exists(vscPath), vscPath));
            #endregion

            #region Sublime
            string sublimeReg = Registry.GetValue(@"HKEY_CLASSES_ROOT\*\shell\Open with Sublime Text\command", "", "").ToStringSafely();
            if (!string.IsNullOrEmpty(sublimeReg) && sublimeReg.Contains(".exe"))
            { sublimeReg = sublimeReg.Remove(sublimeReg.LastIndexOf(".exe")) + ".exe"; }
            textEditors.Add(new TextEditor("Sublime Text", File.Exists(sublimeReg), sublimeReg));
            #endregion

            #region Notepad++
            RegistryKey localRegKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            string nppReg = localRegKey.OpenSubKey(@"SOFTWARE\Notepad++")?.GetValue("").ToStringSafely() + "\\notepad++.exe";
            textEditors.Add(new TextEditor("Notepad++", File.Exists(nppReg), nppReg.ToStringSafely()));
            #endregion

            #region Atom
            string atomReg = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Classes\Applications\atom.exe\shell\open\command", "", "").ToStringSafely();
            if (string.IsNullOrEmpty(atomReg))
            { atomReg = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Classes\atom\shell\open\command", "", "").ToStringSafely(); }
            MatchCollection atomMatches = Regex.Matches(atomReg, @"""(.*atom\.exe)""");
            string atomPath = string.Empty;
            if (atomMatches.Count > 0)
            {
                atomPath = atomMatches[0].Groups[1].Value;
                int atomLindex = atomPath.LastIndexOf(@"\atom\", StringComparison.OrdinalIgnoreCase);
                if (atomLindex > -1) { atomPath = atomPath.Remove(atomLindex) + @"\atom\atom.exe"; }
            }
            textEditors.Add(new TextEditor("Atom", File.Exists(atomPath), atomPath));
            #endregion

            #region Notepad
            bool notepadExists = File.Exists($"{Environment.SystemDirectory}\\notepad.exe");
            textEditors.Add(new TextEditor("Microsoft Notepad", notepadExists, $"{Environment.SystemDirectory}\\notepad.exe"));
            #endregion

            if (onlyExisting) { textEditors = textEditors.Where(te => te.Exists).ToList(); }
            return textEditors;
        }

        public static string RemoveEventTypeName(string? eventName)
        {
            if (eventName.INOE()) { return ""; }
            //return eventName.Remove(eventName.LastIndexOf("(")).TrimEnd();
            return Regex.Replace(eventName, @"\s\([^)]+\)$", "");
        }
    }
}

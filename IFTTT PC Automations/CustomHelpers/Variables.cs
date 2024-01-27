using IFTTT_PC_Automations.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IFTTT_PC_Automations.CustomHelpers
{
    public static class Variables
    {
        public static string DeviceFingerprint = "";
        public static string IftttDefaultApiUrl = "https://maker.ifttt.com/trigger/";
        public static string DefaultDateFormat = "yyyy.MM.dd";
        public static string DefaultTimeFormat = "HH:mm:ss";
        public static string DefaultDateTimeFormat = "yyyy.MM.dd HH:mm:ss";

        public static string LogsPath = $"{Application.StartupPath}Logs";
        public static string ErrorsPath = $"{Application.StartupPath}Errors";

        public static string AppWebsite = "https://github.com/AlexVirlan/IftttPcAutomations/issues";

        public static List<UnsavedLog> UnsavedLogs = new List<UnsavedLog>();
        public static Statistics SessionStatistics = new Statistics();
        public static int SessionAppErrorsCount = 0;

        public static bool TESTING_BOOL = true;
        public static string DebugLogs = "";

        public static void AddDebugLog(string text, bool logCallingMethod = false,
            [CallerMemberName] string cmn = "", [CallerFilePath] string cfp = "", [CallerLineNumber] int cln = 0)
        {
            DebugLogs += text + Environment.NewLine;
            if (logCallingMethod)
            {
                DebugLogs += $"Method: {cmn}, line: {cln}, file: {Path.GetFileName(cfp)}" + Environment.NewLine;
            }
            DebugLogs += Environment.NewLine;
        }
    }
}

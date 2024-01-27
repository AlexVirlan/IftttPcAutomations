using IFTTT_PC_Automations.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFTTT_PC_Automations.CustomHelpers
{
    class Settings
    {
        public static string IftttApiUrl = "https://maker.ifttt.com/trigger/";
        public static string IftttWebhooksKey = ""; // 
        public static string DateFormat = "yyyy.MM.dd";
        public static string TimeFormat = "HH:mm:ss";
        public static string DateTimeFormat = "yyyy.MM.dd HH:mm:ss";

        public static bool RunOnStartup = true;
        public static bool MinimizeToTray = true;
        public static bool NotifyOnEvents = true;
        public static bool DeleteConfirmationPrompts = true;
        public static bool ColorTags = true;
        public static bool BeautifyIftttErrorResponses = true;

        public static Statistics TotalStatistics = new Statistics();
        public static Dictionary<string, string> CustomTags = new Dictionary<string, string>();
        public static Dictionary<string, Event> Events = new Dictionary<string, Event>();
    }

    [Serializable]
    class AppSettings : Settings
    {
        #region Properties
        public string _IftttApiUrl { get { return IftttApiUrl; } set { IftttApiUrl = value; } }
        public string _IftttWebhooksKey { get { return IftttWebhooksKey; } set { IftttWebhooksKey = value; } }
        public string _DateFormat { get { return DateFormat; } set { DateFormat = value; } }
        public string _TimeFormat { get { return TimeFormat; } set { TimeFormat = value; } }
        public string _DateTimeFormat { get { return DateTimeFormat; } set { DateTimeFormat = value; } }

        public bool _RunOnStartup { get { return RunOnStartup; } set { RunOnStartup = value; } }
        public bool _MinimizeToTray { get { return MinimizeToTray; } set { MinimizeToTray = value; } }
        public bool _NotifyOnEvents { get { return NotifyOnEvents; } set { NotifyOnEvents = value; } }
        public bool _DeleteConfirmationPrompts { get { return DeleteConfirmationPrompts; } set { DeleteConfirmationPrompts = value; } }
        public bool _ColorTags { get { return ColorTags; } set { ColorTags = value; } }
        public bool _BeautifyIftttRrrorResponses { get { return BeautifyIftttErrorResponses; } set { BeautifyIftttErrorResponses = value; } }

        public Statistics _TotalStatistics { get { return TotalStatistics; } set { TotalStatistics = value; } }
        public Dictionary<string, string> _CustomTags { get { return CustomTags; } set { CustomTags = value; } }
        public Dictionary<string, Event> _Events { get { return Events; } set { Events = value; } }
        #endregion

        #region Methods (Save & Load)
        public static FunctionResponse Save(string fileName = "App.set")
        {
            try
            {
                string settingsData = JsonConvert.SerializeObject(new AppSettings(), Formatting.None);
                settingsData = StringCipher.Encrypt(settingsData, Variables.DeviceFingerprint);
                File.WriteAllText(fileName.CombineWithStartupPath(), settingsData);
                return new FunctionResponse(error: false, message: "Settings saved successfully.");
            }
            catch (Exception ex)
            {
                return new FunctionResponse(ex);
            }
        }

        public static FunctionResponse Load(string fileName = "App.set")
        {
            string fileFullPath = fileName.CombineWithStartupPath();
            if (!File.Exists(fileFullPath))
            {
                return new FunctionResponse(error: true, message: $"The settings file ({fileName}) is missing.");
            }
            try
            {
                string settingsData = File.ReadAllText(fileFullPath);
                settingsData = StringCipher.Decrypt(settingsData, Variables.DeviceFingerprint);
                JsonConvert.DeserializeObject<AppSettings>(settingsData,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                return new FunctionResponse(error: false, message: "Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                return new FunctionResponse(ex);
            }
        }
        #endregion
    }
}

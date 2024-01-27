using IFTTT_PC_Automations.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Action = IFTTT_PC_Automations.Entities.Action;

namespace IFTTT_PC_Automations.CustomHelpers
{
    public static class Extensions
    {
        #region String
        public static bool INOE(this string? str) => string.IsNullOrEmpty(str);

        public static string ToStringSafely(this string? str, string prefix = "", string suffix = "")
        {
            if (str is null) { return string.Empty; }
            return prefix + str + suffix;
        }

        public static string? Repeat(this string? str, int times = 2, StringRepeatType repeatType = StringRepeatType.SBInsert)
        {
            if (str.INOE()) { return str; }
            if (times < 2) { times = 2; }
            switch (repeatType)
            {
                case StringRepeatType.Replace:
                    return new string('X', times).Replace("X", str);

                case StringRepeatType.Concat:
                    return string.Concat(Enumerable.Repeat(str, times));

                default:
                case StringRepeatType.SBInsert:
                    return new StringBuilder(str.Length * times).Insert(0, str, times).ToString();

                case StringRepeatType.SBAppendJoin:
                    return new StringBuilder(str.Length * times).AppendJoin(str, new string[times + 1]).ToString();
            }
        }

        public static string CombineWithStartupPath(this string fileName)
        {
            if (fileName.INOE()) { return fileName; }
            string file = Path.GetFileName(fileName);
            string fullPath = Path.Combine(Application.StartupPath, file);
            if (File.Exists(fullPath)) { return fullPath; }
            else { return fileName; }
        }

        public static string RemoveLineBreaks(this string str)
        {
            return Regex.Replace(str, @"\r\n?|\n", string.Empty);
        }
        #endregion

        #region Bool
        public static bool Invert(ref this bool boolRef) => boolRef = !boolRef;

        public static string ToYN(this bool? @bool)
        {
            if (@bool is null || @bool == false) { return "No"; }
            else { return "Yes"; }
        }

        public static string ToYN(this bool @bool) => @bool == true ? "Yes" : "No";
        #endregion

        #region Object
        public static string ToStringSafely(this object? @object)
        {
            if (@object is null) { return string.Empty; }
            string? stringifiedObject = @object.ToString();
            return (stringifiedObject is null ? string.Empty : stringifiedObject);
        }

        public static bool ToBoolSafely(this object? @object)
        {
            if (@object is null) { return false; }
            bool.TryParse(@object.ToStringSafely(), out bool boolResult);
            return boolResult;
        }
        #endregion

        #region Long
        public static string ToPrettySize(this long value, int decimalPlaces = 2)
        {
            string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            if (decimalPlaces < 0) { decimalPlaces = 0; }
            if (value == 0) { return string.Format($"{{0:n{decimalPlaces}}} bytes", 0); }
            if (value < 0) { return "-" + ToPrettySize(-value, decimalPlaces); }
            //if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            //return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            return string.Format($"{{0:n{decimalPlaces}}} {{1}}", adjustedSize, SizeSuffixes[mag]);
        }
        #endregion

        #region EventType
        public static string ToReadableString(this EventType eventType)
        {
            switch (eventType)
            {
                case EventType.BatteryBoE: return "Battery below or equal to";
                case EventType.BatteryAoE: return "Battery above or equal to";
                case EventType.Shutdown: return "Shutdown";
                default: return "Event property value";
            }
        }

        public static bool IsEventValueAvailable(this EventType eventType)
        {
            switch (eventType)
            {
                case EventType.BatteryBoE:
                case EventType.BatteryAoE: return true;
                case EventType.Shutdown: return false;
                default: return true;
            }
        }
        #endregion

        #region Controls
        public static bool SetTextSafe(this Control control, string text)
        {
            try
            {
                //if (control is not TextBox || control is not Label || control is not Button) { return false; }
                if (control.InvokeRequired) { control.Invoke(new MethodInvoker(delegate { control.Text = text; })); }
                else { control.Text = text; }
                return true;
            }
            catch (Exception) { return false; }
        }

        delegate bool SetTextCallback(Control control, string text);
        public static bool SetTextSafeV1(this Control control, string text)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    SetTextCallback setTextCallback = new SetTextCallback(SetTextSafeV1);
                    control.FindForm()?.Invoke(setTextCallback, new object[] { control, text });
                    //control.Invoke(setTextCallback, new object[] { control, text });
                }
                else
                {
                    control.Text = text;
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool RunOnUIThread(this Form form, System.Action action)
        {
            try
            {
                if (form.InvokeRequired) { form.Invoke(action); }
                else { action.Invoke(); }
                return true;
            }
            catch (Exception) { return false; }
        }

        public static void SetCursorToEnd(this TextBox textBox)
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
        }

        public static int CheckedRowsCount(this DataGridView dataGridView, string columnName)
        {
            int count = 0;
            //if (dataGridView.Rows.Count == 0) { return 0; }
            if (!dataGridView.Columns.Contains(columnName)) { return 0; }
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[columnName].Value.ToStringSafely().Equals("true", StringComparison.OrdinalIgnoreCase)) { count++; }
            }
            return count;
        }

        public static void HighlightText(this RichTextBox rtb, string text, bool caseSensitive = false, Color? color = null)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            if (color is null) { color = Color.Red; }
            int selStart = rtb.SelectionStart, startIndex = 0, index;
            StringComparison stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            while ((index = rtb.Text.IndexOf(text, startIndex, stringComparison)) != -1)
            {
                rtb.Select(index, text.Length);
                rtb.SelectionColor = (Color)color;
                startIndex = index + text.Length;
            }
            rtb.SelectionStart = selStart;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = rtb.ForeColor;
        }
        #endregion

        #region Others
        public static bool IsNull(this KeyValuePair<string, Event> keyValuePair) => keyValuePair.Key is null;
        public static bool IsNull(this KeyValuePair<string, Action> keyValuePair) => keyValuePair.Key is null;
        #endregion
    }
}

using IFTTT_PC_Automations.CustomHelpers;
using IFTTT_PC_Automations.Forms;
using System;
using System.Reflection;

namespace IFTTT_PC_Automations
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(MyAssemblyLoadEventHandler);

            Mutex mutex = new Mutex(true, "IFTTT PC Automations", out bool newRun);
            if (newRun == false) { return; }

            List<string>? args = Environment.GetCommandLineArgs().Skip(1).ToList();

            ApplicationConfiguration.Initialize();
            Application.Run(new frmMain(args));
        }

        static void MyAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            Variables.AddDebugLog($"Loaded assembly: {args.LoadedAssembly.FullName}, Location={args.LoadedAssembly.Location}");
        }

        static Program()
        {

            //MessageBox.Show("IPA - PROGRAM");
            //MessageBox.Show("Application.StartupPath:" + Environment.NewLine + Application.StartupPath + Environment.NewLine.Repeat() +
            //    "Environment.CurrentDirectory:" + Environment.NewLine + Environment.CurrentDirectory);

            ////Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath) ?? Application.ExecutablePath + Path.DirectorySeparatorChar.ToString();

            //MessageBox.Show("After changing 'Environment.CurrentDirectory':" + Environment.NewLine.Repeat() +
            //    "Application.StartupPath:" + Environment.NewLine + Application.StartupPath + Environment.NewLine.Repeat() +
            //    "Environment.CurrentDirectory:" + Environment.NewLine + Environment.CurrentDirectory);


            try { Environment.CurrentDirectory = Application.StartupPath; }
            catch (Exception) { }
            
            string missingDlls = "";
            string[] requiredDlls = { "Newtonsoft.Json.dll", "DeviceId.dll", "BouncyCastle.Crypto.dll" };
            foreach (string dll in requiredDlls)
            {
                if (!File.Exists(dll.CombineWithStartupPath())) { missingDlls += $"• {dll + Environment.NewLine}"; }
            }
            if (!missingDlls.INOE())
            {
                MessageBox.Show("The following dlls are missing:" + Environment.NewLine + missingDlls + Environment.NewLine + "Exting...",
                    "IFTTT PC Automations - missing dlls", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
    }
}
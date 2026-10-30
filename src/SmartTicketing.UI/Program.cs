using System;
using System.Windows.Forms;

namespace SmartTicketing.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            if (args.Length > 0 && args[0] == "--export-screenshots")
            {
                string outDir = args.Length > 1 ? args[1] : "Screenshots";
                ScreenshotExporter.ExportAllScreens(outDir);
                return;
            }

            Application.Run(new MainForm());
        }
    }
}

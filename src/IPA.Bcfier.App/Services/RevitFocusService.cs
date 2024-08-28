using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IPA.Bcfier.App.Services
{
    /// <summary>
    /// This class is used to set the focus to the Revit desktop application, to ensure that after
    /// an action is called the Revit UI thread processes the new data. Taken from: https://github.com/opf/openproject-revit-add-in/blob/master/src/OpenProject.Browser/WebViewIntegration/RevitMainWindowHandler.cs
    /// </summary>
    public static class RevitFocusService
    {
        public static void SetFocusToRevit()
        {
            var revitProcess = Process
              .GetProcesses()
              .FirstOrDefault(p => p.ProcessName == "Revit");
            if (revitProcess != null)
            {
                try
                {
                    SetForegroundWindow(revitProcess.MainWindowHandle);
                }
                catch
                {
                    // We're ignoring errors here
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}

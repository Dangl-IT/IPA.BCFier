using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IPA.Bcfier.Ipc;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace IPA.Bcfier.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class ListenToIpaBcfierAppPipeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            EnsureDependentAssembliesAreLoaded();

            if (_bcfierAppProcess != null && !_bcfierAppProcess.HasExited)
            {
                return Result.Succeeded;
            }

            OpenIpaBcfierApp(() =>
            {
                _bcfierAppProcess = null;
            }, commandData.Application.ActiveUIDocument.Document.PathName);

            var ipcHandler = new IpcHandler(thisAppName: "Revit", otherAppName: "BcfierApp");
            ipcHandler.InitializeAsync().ConfigureAwait(true).GetAwaiter().GetResult();

            var taskQueueHandler = new RevitTaskQueueHandler();
            var commandListener = new IpcBcfierCommandListener(ipcHandler, taskQueueHandler);
            commandListener.Listen();
            commandData.Application.Idling += taskQueueHandler.OnIdling;

            return Result.Succeeded;
        }

        private static Process? _bcfierAppProcess;

        private static void OpenIpaBcfierApp(Action onExited, string revitProjectPath)
        {
            if (_bcfierAppProcess != null && !_bcfierAppProcess.HasExited)
            {
                return;
            }

            var ipaBcfierExecutablePath = GetIpaBcfierAppExecutablePath();
            if (!File.Exists(ipaBcfierExecutablePath))
            {
                throw new SystemException("IPA.BCFier.App executable not found.");
            }

            var arguments = "--revit-integration";
            if (!string.IsNullOrWhiteSpace(revitProjectPath))
            {
                arguments += $" --revit-project-path \"{revitProjectPath}\"";
            }

            _bcfierAppProcess = Process.Start(ipaBcfierExecutablePath, arguments);
            _bcfierAppProcess.Exited += (sender, args) =>
            {
                onExited();
                _bcfierAppProcess = null;
            };
        }

        private static string GetIpaBcfierAppExecutablePath()
        {
            var currentAssemblyPathUri = Assembly.GetExecutingAssembly().CodeBase;
            var currentAssemblyPath = Uri.UnescapeDataString(new Uri(currentAssemblyPathUri).AbsolutePath).Replace("/", "\\");
            var currentFolder = Path.GetDirectoryName(currentAssemblyPath) ?? string.Empty;

            return Path.Combine(currentFolder, "ipa-bcfier-app", "IPA.Bcfier.exe");
        }

        private void EnsureDependentAssembliesAreLoaded()
        {
            // I didn't find out why this is required, but apparently Revit
            // does something to resolve assemblies, and this seems to fail
            // when the assemblies are not directly loaded due to execution in the
            // initial command but only later, like in our case when an event from
            // the browser is triggering some action
            typeof(IPA.Bcfier.Models.Bcf.BcfComment).ToString();
            typeof(Dangl.BCF.APIObjects.V21.Auth_GET).ToString();
            typeof(IPA.Bcfier.Ipc.IpcHandler).ToString();
            typeof(DecimalMath.DecimalEx).ToString();
        }
    }
}

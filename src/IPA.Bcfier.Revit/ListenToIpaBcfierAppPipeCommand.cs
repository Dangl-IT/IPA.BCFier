using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IPA.Bcfier.Ipc;
using Newtonsoft.Json;
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

#if !DEBUG_BUILD
            var appCorrelationId = Guid.NewGuid();
            OpenIpaBcfierApp(() =>
            {
                _bcfierAppProcess = null;
            }, commandData.Application.ActiveUIDocument.Document.PathName,
               appCorrelationId);
#else
            // If we're in a debug build, we want to use a static Guid for the app id, so that we can launch the UI manually
            // in debug mode and apply changes quickly
            var appCorrelationId = new Guid("3136262a-5f6b-41b0-ba89-6a8c4212b699");
#endif

            var ipcHandler = new IpcHandler(thisAppName: "Revit", otherAppName: "BcfierApp", appCorrelationId);
            ipcHandler.InitializeAsync().ConfigureAwait(true).GetAwaiter().GetResult();

            var taskQueueHandler = new RevitTaskQueueHandler();
            var commandListener = new IpcBcfierCommandListener(ipcHandler, taskQueueHandler, appCorrelationId);
            commandListener.Listen();
            commandData.Application.Idling += taskQueueHandler.OnIdling;
            commandData.Application.ApplicationClosing += (s, e) =>
            {
                if (_bcfierAppProcess != null)
                {
                    ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                    {
                        Command = IpcMessageCommand.CadClosing,
                        Data = appCorrelationId.ToString()
                    })).ConfigureAwait(true).GetAwaiter().GetResult();
                }
            };

            return Result.Succeeded;
        }

        private static Process? _bcfierAppProcess;

        private static void OpenIpaBcfierApp(Action onExited,
            string revitProjectPath,
            Guid appCorrelationId)
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

            var arguments = $"--revit-integration --app-correlation-id=\"{appCorrelationId}\"";
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

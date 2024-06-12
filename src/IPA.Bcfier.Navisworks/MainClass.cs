using Autodesk.Navisworks.Api.Plugins;
using System.Diagnostics;
using System.Reflection;
using IPA.Bcfier.Ipc;
using Newtonsoft.Json;

namespace IPA.Bcfier.Navisworks
{
    [PluginAttribute("IPA.BCFier", "Dangl It GmbH", DisplayName = "IPA.BCFier")]
    [RibbonTab("IPA.BCFier", DisplayName = "IPA.BCFier")]
    [AddInPlugin(AddInLocation.AddIn)]
    public class MainClass : AddInPlugin
    {
        private static Process? _bcfierAppProcess;

        public override int Execute(params string[] parameters)
        {
            EnsureDependentAssembliesAreLoaded();

            if (_bcfierAppProcess != null && !_bcfierAppProcess.HasExited)
            {
                return 0;
            }

            var taskQueueHandler = new NavisworksTaskQueueHandler();
#if !DEBUG_BUILD
            var appCorrelationId = Guid.NewGuid();
            OpenIpaBcfierApp(() =>
            {
                _bcfierAppProcess = null;
            }, appCorrelationId);
#else
            // If we're in a debug build, we want to use a static Guid for the app id, so that we can launch the UI manually
            // in debug mode and apply changes quickly
            var appCorrelationId = new Guid("65ef2104-64ca-4390-bae3-3de4901a53dc");
#endif

            var ipcHandler = new IpcHandler(thisAppName: "Navisworks", otherAppName: "BcfierAppNavisworks", appCorrelationId);
            ipcHandler.InitializeAsync().ConfigureAwait(true).GetAwaiter().GetResult();

            var commandListener = new IpcNavisworksCommandListener(ipcHandler, taskQueueHandler, appCorrelationId);
            commandListener.Listen();
            Autodesk.Navisworks.Api.Application.Idle += taskQueueHandler.OnIdling;
            Autodesk.Navisworks.Api.Application.Gui.Closing += (s, e) =>
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

            return 0;
        }

        private static void OpenIpaBcfierApp(Action onExited,
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

            var arguments = $"--navisworks-integration --app-correlation-id=\"{appCorrelationId}\"";

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

            return Path.Combine(currentFolder, "..", "ipa-bcfier-app", "IPA.Bcfier.exe");
        }

        private void EnsureDependentAssembliesAreLoaded()
        {
            // I didn't find out why this is required, but apparently Revit (and probably also Navisworks)
            // does something to resolve assemblies, and this seems to fail
            // when the assemblies are not directly loaded due to execution in the
            // initial command but only later, like in our case when an event from
            // the browser is triggering some action
            typeof(IPA.Bcfier.Models.Bcf.BcfComment).ToString();
            typeof(IPA.Bcfier.Models.Clashes.NavisworksClashSelection).ToString();
            typeof(Dangl.BCF.APIObjects.V21.Auth_GET).ToString();
            typeof(IPA.Bcfier.Ipc.IpcHandler).ToString();
            typeof(DecimalMath.DecimalEx).ToString();
        }
    }
}

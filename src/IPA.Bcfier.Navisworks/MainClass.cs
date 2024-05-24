using Autodesk.Navisworks.Api.Plugins;
using System.Diagnostics;
using System.Reflection;
using IPA.Bcfier.Ipc;

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
            var appCorrelationId = Guid.NewGuid().ToString();
            OpenIpaBcfierApp(() =>
            {
                _bcfierAppProcess = null;
            }, appCorrelationId);

            var ipcHandler = new IpcHandler(thisAppName: "Navisworks", otherAppName: "BcfierAppNavisworks");
            ipcHandler.InitializeAsync().ConfigureAwait(true).GetAwaiter().GetResult();

            var commandListener = new IpcNavisworksCommandListener(ipcHandler, taskQueueHandler, appCorrelationId);
            commandListener.Listen();
            Autodesk.Navisworks.Api.Application.Idle += taskQueueHandler.OnIdling;

            return 0;
        }

        private static void OpenIpaBcfierApp(Action onExited,
            string appCorrelationId)
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

            var arguments = $"--navisworks-integration --app-correlation-id \"{appCorrelationId}\"";

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

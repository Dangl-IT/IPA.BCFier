using IPA.Bcfier.App.Configuration;
using IPA.Bcfier.Ipc;

namespace IPA.Bcfier.App.Services
{
    // We're using this service so that there's always one listener at the start of the application active
    public class IpcHandlerLifetimeService
    {
        private IpcHandler? _ipcHandler;

        public IpcHandler IpcHandler => _ipcHandler!;

        public async Task StartAsync(RevitParameters revitParameters, AppParameters appParameters)
        {
            _ipcHandler = GetIpcHandler(revitParameters, appParameters);
            await _ipcHandler.InitializeAsync();
        }

        public void Stop()
        {
            _ipcHandler?.Dispose();
        }

        private IpcHandler GetIpcHandler(RevitParameters revitParameters, AppParameters appParameters)
        {
            if (revitParameters.IsConnectedToRevit)
            {
                return new IpcHandler(thisAppName: "BcfierApp", otherAppName: "Revit", appParameters.ApplicationId);
            }

            // We're assuming it's Navisworks then, since we don't have another possibility at the moment
            return new IpcHandler(thisAppName: "BcfierAppNavisworks", otherAppName: "Navisworks", appParameters.ApplicationId);
        }
    }
}

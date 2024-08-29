using IPA.Bcfier.Ipc;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Models.Ipc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IPA.Bcfier.Revit
{
    public class IpcBcfierCommandListener
    {
        private readonly IpcHandler _ipcHandler;
        private readonly RevitTaskQueueHandler _revitTaskQueueHandler;
        private readonly Guid _appCorrelationId;
        private bool _isRunning = true;

        public IpcBcfierCommandListener(IpcHandler ipcHandler,
            RevitTaskQueueHandler revitTaskQueueHandler,
            Guid appCorrelationId)
        {
            _ipcHandler = ipcHandler;
            _revitTaskQueueHandler = revitTaskQueueHandler;
            _appCorrelationId = appCorrelationId;
        }

        public void Listen()
        {
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                    {
                        var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                        switch (ipcMessage.Command)
                        {
                            case IpcMessageCommand.AppClosed:
                                if (ipcMessage.Data == _appCorrelationId.ToString())
                                {
                                    _isRunning = false;
                                }
                                break;

                            case IpcMessageCommand.CreateViewpoint:
                                _revitTaskQueueHandler.CreateRevitViewpointCallbacks.Enqueue(async (data) =>
                                {
                                    await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                    {
                                        CorrelationId = ipcMessage.CorrelationId,
                                        Command = IpcMessageCommand.ViewpointCreated,
                                        Data = data
                                    }));
                                });
                                break;

                            case IpcMessageCommand.ShowViewpoint:
                                var messageData = JsonConvert.DeserializeObject<ViewpointDisplayIpcModel>(ipcMessage.Data!)!;
                                _revitTaskQueueHandler.ShowViewpointQueueItems.Enqueue(new Models.ShowViewpointQueueItem
                                {
                                    Callback = async () =>
                                    {
                                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                        {
                                            CorrelationId = ipcMessage.CorrelationId,
                                            Command = IpcMessageCommand.ViewpointShown
                                        }));
                                    },
                                    Viewpoint = messageData.BcfViewpoint,
                                    ViewpointOriginatesFromRevit = messageData.ViewpointOriginatesFromRevit
                                });
                                break;

                            default:
                                // TODO
                                throw new NotImplementedException();
                        }
                    }

                    if (_revitTaskQueueHandler.CadErrorMessages.TryDequeue(out var errorMessage))
                    {
                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                        {
                            Command = IpcMessageCommand.PluginErrorEncountered,
                            Data = errorMessage
                        }));
                    }

                    await Task.Delay(100);
                }

                _revitTaskQueueHandler.UnregisterEventHandler();
                _ipcHandler.Dispose();
            });
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}

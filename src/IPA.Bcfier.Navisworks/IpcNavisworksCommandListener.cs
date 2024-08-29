using IPA.Bcfier.Ipc;
using Newtonsoft.Json;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Models;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Models.Ipc;

namespace IPA.Bcfier.Navisworks
{
    public class IpcNavisworksCommandListener
    {
        private readonly IpcHandler _ipcHandler;
        private readonly NavisworksTaskQueueHandler _navisworksTaskHandler;
        private readonly Guid _appCorrelationId;
        private bool _isRunning = true;

        public IpcNavisworksCommandListener(IpcHandler ipcHandler,
            NavisworksTaskQueueHandler navisworksTaskHandler,
            Guid appCorrelationId)
        {
            _ipcHandler = ipcHandler;
            _navisworksTaskHandler = navisworksTaskHandler;
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
                                _navisworksTaskHandler.CreateNavisworksViewpointCallbacks.Enqueue(async (data) =>
                                {
                                    await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                    {
                                        CorrelationId = ipcMessage.CorrelationId,
                                        Command = IpcMessageCommand.ViewpointCreated,
                                        Data = data
                                    }));
                                });
                                break;

                            case IpcMessageCommand.NavisworksClashIssuesCancellation:
                                var clashIdToCancel = JsonConvert.DeserializeObject<Guid>(ipcMessage.Data!)!;
                                _navisworksTaskHandler.NavisworksClashCancellationQueue.Enqueue(clashIdToCancel);
                                break;

                            case IpcMessageCommand.CreateNavisworksClashDetectionIssues:
                                var data = JsonConvert.DeserializeObject<NavisworksClashCreationData>(ipcMessage.Data!)!;
                                _navisworksTaskHandler.CreateNavisworksClashIssuesCallbacks.Enqueue(new CreateClashIssuesQueueItem
                                {
                                    ClashCreationData = data,
                                    Callback = async (data) =>
                                    {
                                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                        {
                                            CorrelationId = ipcMessage.CorrelationId,
                                            Command = IpcMessageCommand.NavisworksClashDetectionIssuesCreated,
                                            Data = data
                                        }));
                                    },
                                    CallbackReportTotalCount = totalCount =>
                                    {
                                        Task.Run(async () =>
                                        {
                                            await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                            {
                                                CorrelationId = ipcMessage.CorrelationId,
                                                Command = IpcMessageCommand.NavisworksClashIssuesTotalCount,
                                                Data = totalCount.ToString()
                                            }));
                                        });
                                    },
                                    CallbackReportCurrentCount = currentCount =>
                                    {
                                        Task.Run(async () =>
                                        {
                                            await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                            {
                                                CorrelationId = ipcMessage.CorrelationId,
                                                Command = IpcMessageCommand.NavisworksClashIssuesCurrentCount,
                                                Data = currentCount.ToString()
                                            }));
                                        });
                                    },
                                });
                                break;

                            case IpcMessageCommand.ShowViewpoint:
                                _navisworksTaskHandler.ShowViewpointQueueItems.Enqueue(new ShowViewpointQueueItem
                                {
                                    Callback = async () =>
                                    {
                                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                        {
                                            CorrelationId = ipcMessage.CorrelationId,
                                            Command = IpcMessageCommand.ViewpointShown
                                        }));
                                    },
                                    Viewpoint = JsonConvert.DeserializeObject<ViewpointDisplayIpcModel>(ipcMessage.Data!)!.BcfViewpoint
                                });
                                break;

                            case IpcMessageCommand.GetNavisworksAvailableClashes:
                                _navisworksTaskHandler.GetAvailableNavisworksClashes.Enqueue(async (availableClashes) =>
                                    {
                                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                        {
                                            CorrelationId = ipcMessage.CorrelationId,
                                            Command = IpcMessageCommand.NavisworksAvailableClashes,
                                            Data = availableClashes
                                        }));
                                    }
                                );
                                break;

                            default:
                                // TODO
                                throw new NotImplementedException();
                        }
                    }

                    if (_navisworksTaskHandler.CadErrorMessages.TryDequeue(out var errorMessage))
                    {
                        await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                        {
                            Command = IpcMessageCommand.PluginErrorEncountered,
                            Data = errorMessage
                        }));
                    }

                    await Task.Delay(100);
                }

                _navisworksTaskHandler.UnregisterEventHandler();
                _ipcHandler.Dispose();
            });
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}

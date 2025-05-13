using IPA.Bcfier.Ipc;
using Newtonsoft.Json;
using IPA.Bcfier.Navisworks.Models;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Models.Ipc;
using IPA.Bcfier.Models.Viewpoints;
using Autodesk.Navisworks.Api;

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

                            case IpcMessageCommand.GetElementNamesList:
                                var ifcGuidNamePairList = new List<IfcGuidNamePair>();
                                var ifcGuids = JsonConvert.DeserializeObject<List<string>>(ipcMessage.Data!)!;
                                foreach (var ifcGuid in ifcGuids)
                                {
                                    var name = ifcGuid;

                                    var searchGetElementNamesList = new Search();
                                    searchGetElementNamesList.SearchConditions.Add(SearchCondition.HasPropertyByDisplayName("IfcGUID", "IfcGUID").EqualValue(new VariantData(ifcGuid)));

                                    var resultsGetElementNamesList = searchGetElementNamesList.FindAll(Application.ActiveDocument, false);
                                    if (!resultsGetElementNamesList.IsEmpty)
                                    {
                                        name = resultsGetElementNamesList.First.DisplayName;
                                    }

                                    ifcGuidNamePairList.Add(new IfcGuidNamePair
                                    {
                                        IfcGuid = ifcGuid,
                                        Name = name
                                    });
                                }

                                await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                {
                                    CorrelationId = ipcMessage.CorrelationId,
                                    Command = IpcMessageCommand.ReturnElementNamesList,
                                    Data = JsonConvert.SerializeObject(ifcGuidNamePairList)
                                }));
                                break;

                            case IpcMessageCommand.SelectElement:
                                var isSuccess = false;

                                var searchSelectElement = new Search();
                                searchSelectElement.SearchConditions.Add(SearchCondition.HasPropertyByDisplayName("IfcGUID", "IfcGUID").EqualValue(new VariantData(ipcMessage.Data)));

                                var resultsSelectElement = searchSelectElement.FindAll(Application.ActiveDocument, false);
                                if (!resultsSelectElement.IsEmpty)
                                {
                                    Application.ActiveDocument.CurrentSelection.Clear();
                                    Application.ActiveDocument.CurrentSelection.Add(resultsSelectElement.First);
                                    isSuccess = true;
                                }

                                await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                {
                                    CorrelationId = ipcMessage.CorrelationId,
                                    Command = IpcMessageCommand.SelectElementResult,
                                    Data = isSuccess.ToString()
                                }));
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

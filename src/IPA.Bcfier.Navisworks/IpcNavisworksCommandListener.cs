using IPA.Bcfier.Ipc;
using Newtonsoft.Json;
using IPA.Bcfier.Navisworks.Models;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Models.Ipc;
using IPA.Bcfier.Models.Viewpoints;
using Autodesk.Navisworks.Api;
using System.Runtime.Remoting.Messaging;
using IPA.Bcfier.Navisworks.Services;

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
                                var elementIds = JsonConvert.DeserializeObject<List<IfcGuidNamePair>>(ipcMessage.Data!)!;
                                foreach (var elementId in elementIds)
                                {
                                    var name = elementId.IfcGuid;

                                    var searchGetElementNamesList = new Search();
                                    searchGetElementNamesList.SearchConditions.Add(SearchCondition.HasPropertyByDisplayName("IfcGUID", "IfcGUID").EqualValue(new VariantData(elementId.IfcGuid)));

                                    var resultsGetElementNamesList = searchGetElementNamesList.FindAll(Application.ActiveDocument, false);
                                    if (!resultsGetElementNamesList.IsEmpty)
                                    {
                                        name = resultsGetElementNamesList.First.DisplayName;
                                    }

                                    ifcGuidNamePairList.Add(new IfcGuidNamePair
                                    {
                                        IfcGuid = elementId.IfcGuid,
                                        RevitId = elementId.RevitId,
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
                                await HandleSelectElementAsync(ipcMessage);
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

        private async Task HandleSelectElementAsync(IpcMessage ipcMessage)
        {
            var isSuccess = false;

            var searchSelectElement = new Search();
            var elementId = JsonConvert.DeserializeObject<IfcGuidNamePair>(ipcMessage.Data!);
            searchSelectElement.SearchConditions.Add(SearchCondition.HasPropertyByDisplayName("IfcGUID", "IfcGUID").EqualValue(new VariantData(elementId.IfcGuid)));

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
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}

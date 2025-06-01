using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using IPA.Bcfier.Ipc;
using IPA.Bcfier.Models.Ipc;
using IPA.Bcfier.Models.Projects;
using IPA.Bcfier.Models.Viewpoints;
using Newtonsoft.Json;

namespace IPA.Bcfier.Revit
{
    public class IpcBcfierCommandListener
    {
        private readonly IpcHandler _ipcHandler;
        private readonly RevitTaskQueueHandler _revitTaskQueueHandler;
        private readonly Guid _appCorrelationId;
        private bool _isRunning = true;
        private readonly ExternalCommandData _commandData;

        public IpcBcfierCommandListener(IpcHandler ipcHandler,
            RevitTaskQueueHandler revitTaskQueueHandler,
            Guid appCorrelationId,
            ExternalCommandData commandData)
        {
            _ipcHandler = ipcHandler;
            _revitTaskQueueHandler = revitTaskQueueHandler;
            _appCorrelationId = appCorrelationId;
            _commandData = commandData;
        }

        public void Listen()
        {
            Task.Run(async () =>
            {
                await SendRevitProjectDataToUiAsync();

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

                            case IpcMessageCommand.RefreshProjectData:
                                await SendRevitProjectDataToUiAsync();
                                break;

                            case IpcMessageCommand.GetElementNamesList:
                                await HandleGetElementNamesListAsync(ipcMessage);
                                break;

                            case IpcMessageCommand.SelectElement:
                                await HandleSelectElementAsync(ipcMessage);
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

        private async Task HandleGetElementNamesListAsync(IpcMessage ipcMessage)
        {
            var ifcGuidNamePairList = new List<IfcGuidNamePair>();
            var elementIds = JsonConvert.DeserializeObject<List<IfcGuidNamePair>>(ipcMessage.Data!)!;
            var uiDocument = _commandData.Application.ActiveUIDocument;
            var collectorGetElementNamesList = new FilteredElementCollector(uiDocument.Document).WhereElementIsNotElementType().ToList();
            foreach (var elementId in elementIds)
            {
                var name = elementId.IfcGuid;

                foreach (var element in collectorGetElementNamesList)
                {
                    var ifcGuidParam = element.LookupParameter("IfcGUID");
                    if (ifcGuidParam != null && ifcGuidParam.AsString() == elementId.IfcGuid)
                    {
                        name = element.Name;
                        break;
                    }
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
        }

        private async Task HandleSelectElementAsync(IpcMessage ipcMessage)
        {
            var isSuccess = false;
            var uiDocument = _commandData.Application.ActiveUIDocument;

            var elementId = JsonConvert.DeserializeObject<IfcGuidNamePair>(ipcMessage.Data!)!;

            if (!string.IsNullOrWhiteSpace(elementId?.RevitId) && long.TryParse(elementId!.RevitId, out _))
            {
                var element = uiDocument.Document.GetElement(new ElementId(long.Parse(elementId.RevitId)));
                if (element != null)
                {
                    _revitTaskQueueHandler.ElementSelectionInstructionsQueue.Enqueue(new Models.ElementSelectionInstructions
                    {
                        ElementId = element.Id,
                    });
                    isSuccess = true;
                }
            }
            else if (elementId != null)
            {
                var collectorSelectElement = new FilteredElementCollector(uiDocument.Document).WhereElementIsNotElementType().ToList();
                foreach (var element in collectorSelectElement)
                {
                    var ifcGuidParam = element.LookupParameter("IfcGUID");
                    if (ifcGuidParam != null && ifcGuidParam.AsString() == elementId.IfcGuid)
                    {
                        _revitTaskQueueHandler.ElementSelectionInstructionsQueue.Enqueue(new Models.ElementSelectionInstructions
                        {
                            ElementId = element.Id,
                        });
                        isSuccess = true;
                        break;
                    }
                }
            }

            await _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = ipcMessage.CorrelationId,
                Command = IpcMessageCommand.SelectElementResult,
                Data = isSuccess.ToString()
            }));
        }

        private Task SendRevitProjectDataToUiAsync()
        {
            return _ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                Command = IpcMessageCommand.RevitProjectChanged,
                Data = JsonConvert.SerializeObject(new ProjectData
                {
                    ProjectNumber = _commandData.Application.ActiveUIDocument.Document.ProjectInformation.Number,
                    FilePath = _commandData.Application.ActiveUIDocument.Document.PathName
                })
            }));
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}

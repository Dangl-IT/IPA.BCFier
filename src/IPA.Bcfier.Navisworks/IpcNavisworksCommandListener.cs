﻿using IPA.Bcfier.Ipc;
using Newtonsoft.Json;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Models;

namespace IPA.Bcfier.Navisworks
{
    public class IpcNavisworksCommandListener
    {
        private readonly IpcHandler _ipcHandler;
        private readonly NavisworksTaskQueueHandler _navisworksTaskHandler;
        private bool _isRunning = true;

        public IpcNavisworksCommandListener(IpcHandler ipcHandler,
            NavisworksTaskQueueHandler navisworksTaskHandler)
        {
            _ipcHandler = ipcHandler;
            _navisworksTaskHandler = navisworksTaskHandler;
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
                                _isRunning = false;
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
                                    Viewpoint = JsonConvert.DeserializeObject<BcfViewpoint>(ipcMessage.Data!)
                                });
                                break;

                            default:
                                // TODO
                                throw new NotImplementedException();
                        }
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

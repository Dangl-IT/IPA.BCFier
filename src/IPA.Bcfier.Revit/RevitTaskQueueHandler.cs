using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Revit.Models;
using IPA.Bcfier.Revit.Services;
using IPA.Bcfier.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;

namespace IPA.Bcfier.Revit
{
    public class RevitTaskQueueHandler
    {
        public Queue<Func<string, Task>> OpenBcfFileCallbacks { get; } = new Queue<Func<string, Task>>();
        public Queue<Func<string, Task>> CreateRevitViewpointCallbacks { get; } = new Queue<Func<string, Task>>();
        public Queue<ShowViewpointQueueItem> ShowViewpointQueueItems { get; } = new Queue<ShowViewpointQueueItem>();
        private Queue<ViewContinuationInstructions> AfterViewCreationCallbackQueue { get; } = new Queue<ViewContinuationInstructions>();
        public ConcurrentQueue<string> CadErrorMessages { get; } = new ConcurrentQueue<string>();
        private bool shouldUnregister = false;

        public void OnIdling(object sender, IdlingEventArgs args)
        {
            var uiApplication = sender as UIApplication;
            if (uiApplication == null)
            {
                return;
            }

            if (shouldUnregister)
            {
                uiApplication.Idling -= OnIdling;
            }

            if (OpenBcfFileCallbacks.Count > 0)
            {
                var callback = OpenBcfFileCallbacks.Dequeue();
                HandleOpenBcfFileCallback(callback);
            }

            if (CreateRevitViewpointCallbacks.Count > 0)
            {
                var uiDocument = uiApplication.ActiveUIDocument;
                var callback = CreateRevitViewpointCallbacks.Dequeue();
                HandleCreateRevitViewpointCallback(callback, uiDocument);
            }

            if (ShowViewpointQueueItems.Count > 0)
            {
                var uiDocument = uiApplication.ActiveUIDocument;
                var showViewpointQueueItem = ShowViewpointQueueItems.Dequeue();
                HandleShowRevitViewpointCallback(showViewpointQueueItem.Callback, showViewpointQueueItem.Viewpoint, uiDocument);
            }

            if (AfterViewCreationCallbackQueue.Count > 0)
            {
                var uiDocument = uiApplication.ActiveUIDocument;
                HandlAfterViewCreationCallbackQueueItems(uiDocument);
            }
        }

        private void HandlAfterViewCreationCallbackQueueItems(UIDocument uiDocument)
        {
            // This is pretty complicated. The signal flow is like this:
            // 1. User clicks on a button in the web view
            // 2. We send that data to the Revit API, which puts the request on a queue
            // 3. During the Revit Application.Idling event, we process the queue
            // 4. A viewpoint display request is processed, and a view is created and set as active view
            // The active view can only be set in an asynchronous way from the Application.Idling
            // event in the Revit API, so we need to wait until the new view is loaded
            // 5. Once the view is loaded, we check this other queue here and apply the callback,
            //    which sets e.g. the selected components
            // 6. After that, we can inform the frontend
            try
            {
                var queueLength = AfterViewCreationCallbackQueue.Count;
                for (var i = 0; i < queueLength; i++)
                {
                    var item = AfterViewCreationCallbackQueue.Dequeue();
                    if (item?.ViewId == uiDocument.ActiveView.Id)
                    {
                        item.ViewContinuation?.Invoke();
                        Task.Run(async () =>
                        {
                            if (item != null && item.Callback != null)
                            {
                                await item.Callback();
                            }
                        });
                    }
                    else if (item != null)
                    {
                        AfterViewCreationCallbackQueue.Enqueue(item);
                    }
                }
            }
            catch (Exception e)
            {
                CadErrorMessages.Enqueue($"Error during viewpoint display (after view init): {Environment.NewLine}{e}");
            }
        }

        public void UnregisterEventHandler()
        {
            shouldUnregister = true;
        }

        private void HandleOpenBcfFileCallback(Func<string, Task> callback)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "BCF Files (*.bcf, *.bcfzip)|*.bcf;*.bcfzip"
            };

            if (!openFileDialog.ShowDialog() ?? false || openFileDialog.FileName == null)
            {
                return;
            }

            var bcfFilePath = openFileDialog.FileName;
            Task.Run(async () =>
            {
                var bcfFileName = Path.GetFileName(bcfFilePath);
                using var bcfFileStream = File.OpenRead(bcfFilePath);
                var bcfResult = await new BcfImportService().ImportBcfFileAsync(bcfFileStream, bcfFileName ?? "issue.bcf");
                var contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                };

                await callback(JsonConvert.SerializeObject(bcfResult, serializerSettings));
            });
        }

        private void HandleCreateRevitViewpointCallback(Func<string, Task> callback, UIDocument uiDocument)
        {
            try
            {
                var viewpointService = new RevitViewpointCreationService(uiDocument);
                var viewpoint = viewpointService.GenerateViewpoint();
                var contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                };
                Task.Run(async () =>
                {
                    if (viewpoint == null)
                    {
                        await callback("{}");
                    }
                    else
                    {
                        await callback(JsonConvert.SerializeObject(viewpoint, serializerSettings));
                    }
                });
            }
            catch (Exception e)
            {
                CadErrorMessages.Enqueue($"Error during viewpoint creation: {Environment.NewLine}{e}");
            }
        }

        private void HandleShowRevitViewpointCallback(Func<Task>? callback, BcfViewpoint? viewpoint, UIDocument uiDocument)
        {
            if (callback == null || viewpoint == null)
            {
                return;
            }

            try
            {
                var viewpointService = new RevitViewpointDisplayService(uiDocument);
                var afterViewInitCallback = viewpointService.DisplayViewpoint(viewpoint);
                if (afterViewInitCallback?.ViewId == null)
                {
                    Task.Run(async () =>
                    {
                        await callback();
                    });
                    return;
                }

                afterViewInitCallback.Callback = callback;
                AfterViewCreationCallbackQueue.Enqueue(afterViewInitCallback);
            }
            catch (Exception e)
            {
                CadErrorMessages.Enqueue($"Error during viewpoint display: {Environment.NewLine}{e}");
            }
        }
    }
}

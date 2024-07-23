using Newtonsoft.Json;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Models;
using Autodesk.Navisworks.Api;
using IPA.Bcfier.Navisworks.Services;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using IPA.Bcfier.Models.Clashes;

namespace IPA.Bcfier.Navisworks
{
    public class NavisworksTaskQueueHandler
    {
        public Queue<Func<string, Task>> CreateNavisworksViewpointCallbacks { get; } = new Queue<Func<string, Task>>();
        public Queue<CreateClashIssuesQueueItem> CreateNavisworksClashIssuesCallbacks { get; } = new Queue<CreateClashIssuesQueueItem>();
        public Queue<ShowViewpointQueueItem> ShowViewpointQueueItems { get; } = new Queue<ShowViewpointQueueItem>();
        public Queue<Func<string, Task>> GetAvailableNavisworksClashes { get; } = new Queue<Func<string, Task>>();
        private bool shouldUnregister = false;
        public ConcurrentQueue<string> CadErrorMessages { get; } = new ConcurrentQueue<string>();

        public Queue<Guid> NavisworksClashCancellationQueue { get; } = new Queue<Guid>();

        private Dictionary<Guid, CancellationTokenSource> _navisworksClashCancellationTokenSourcesByCorrelationId = new Dictionary<Guid, CancellationTokenSource>();

        public void OnIdling(object sender, EventArgs args)
        {
            if (shouldUnregister)
            {
                Application.Idle -= OnIdling;
            }

            if (CreateNavisworksViewpointCallbacks.Count > 0)
            {
                var uiDocument = Application.ActiveDocument;
                var callback = CreateNavisworksViewpointCallbacks.Dequeue();
                HandleCreateNavisworksViewpointCallback(callback, uiDocument);
            }

            if (CreateNavisworksClashIssuesCallbacks.Count > 0)
            {
                var uiDocument = Application.ActiveDocument;
                var queueItem = CreateNavisworksClashIssuesCallbacks.Dequeue();

                var cancellationTokenSource = new CancellationTokenSource();
                if (!_navisworksClashCancellationTokenSourcesByCorrelationId.ContainsKey(queueItem.ClashCreationData.ClashId))
                {
                    _navisworksClashCancellationTokenSourcesByCorrelationId.Add(queueItem.ClashCreationData.ClashId, cancellationTokenSource);
                }
                
                HandleCreateNavisworksClashIssuesCallback(queueItem.Callback,
                    uiDocument,
                    queueItem.ClashCreationData,
                    queueItem.CallbackReportTotalCount,
                    queueItem.CallbackReportCurrentCount,
                    () => CheckForNavisworksClashCancellation(),
                    cancellationTokenSource.Token);
            }

            CheckForNavisworksClashCancellation();

            if (ShowViewpointQueueItems.Count > 0)
            {
                var uiDocument = Application.ActiveDocument;
                var showViewpointQueueItem = ShowViewpointQueueItems.Dequeue();
                HandleShowNavisworksViewpointCallback(showViewpointQueueItem.Callback, showViewpointQueueItem.Viewpoint, uiDocument);
            }

            if (GetAvailableNavisworksClashes.Count > 0)
            {
                var uiDocument = Application.ActiveDocument;
                var callback = GetAvailableNavisworksClashes.Dequeue();
                HandleGetAvailableNavisworksClashes(callback, uiDocument);
            }
        }

        /// <summary>
        /// We're using a separate method for this, because when we're in the process of actually generating the clashes,
        /// this all happens in the UI thread, so the OnIdling event won't be triggered until the clashes are generated.
        /// By moving this to a separate method, we can pass it as a function down to the service where the clashes are
        /// generated, so after every generated clash we can check if we should cancel / stop the generation process.
        /// </summary>
        private void CheckForNavisworksClashCancellation()
        {
            if (NavisworksClashCancellationQueue.Count > 0)
            {
                var correlationId = NavisworksClashCancellationQueue.Dequeue();
                if (_navisworksClashCancellationTokenSourcesByCorrelationId.TryGetValue(correlationId, out var clashCancellationTokenSource))
                {
                    clashCancellationTokenSource.Cancel();
                    _navisworksClashCancellationTokenSourcesByCorrelationId.Remove(correlationId);
                }
            }
        }

        public void UnregisterEventHandler()
        {
            shouldUnregister = true;
        }

        private void HandleGetAvailableNavisworksClashes(Func<string, Task> callback, Document uiDocument)
        {
            var viewpointService = new NavisworksViewpointCreationService(uiDocument);
            var clashes = viewpointService.GetAvailableClashesForExport();
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
                if (clashes == null)
                {
                    await callback("[]");
                }
                else
                {
                    await callback(JsonConvert.SerializeObject(clashes, serializerSettings));
                }
            });
        }

        private void HandleCreateNavisworksViewpointCallback(Func<string, Task> callback, Document uiDocument)
        {
            try
            {
                var viewpointService = new NavisworksViewpointCreationService(uiDocument);
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

        private void HandleCreateNavisworksClashIssuesCallback(Func<string, Task> callback,
            Document uiDocument,
            NavisworksClashCreationData clashCreationData,
            Action<int> reportTotalCount,
            Action<int> reportCurrentCount,
            Action checkForNavisworksClashCancellation,
            CancellationToken cancellationToken)
        {
            try
            {
                var viewpointService = new NavisworksViewpointCreationService(uiDocument);
                var clashIssues = viewpointService.CreateClashIssues(clashCreationData, reportTotalCount, reportCurrentCount, checkForNavisworksClashCancellation, cancellationToken);
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
                    if (_navisworksClashCancellationTokenSourcesByCorrelationId.ContainsKey(clashCreationData.ClashId))
                    {
                        _navisworksClashCancellationTokenSourcesByCorrelationId.Remove(clashCreationData.ClashId);
                    }

                    if (clashIssues == null)
                    {
                        await callback("[]");
                    }
                    else
                    {
                        await callback(JsonConvert.SerializeObject(clashIssues, serializerSettings));
                    }
                });
            }
            catch (Exception e)
            {
                CadErrorMessages.Enqueue($"Error during clash issues creation: {Environment.NewLine}{e}");
            }
        }

        private void HandleShowNavisworksViewpointCallback(Func<Task>? callback, BcfViewpoint? viewpoint, Document uiDocument)
        {
            if (callback == null || viewpoint == null)
            {
                return;
            }

            try
            {
                var viewpointService = new NavisworksViewpointDisplayService(uiDocument);
                viewpointService.DisplayViewpoint(viewpoint);
                Task.Run(async () => await callback());
            }
            catch (Exception e)
            {
                CadErrorMessages.Enqueue($"Error during viewpoint rendering: {Environment.NewLine}{e}");
            }
        }
    }
}

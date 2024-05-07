using Newtonsoft.Json;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Models;
using Autodesk.Navisworks.Api;
using IPA.Bcfier.Navisworks.Services;
using Newtonsoft.Json.Serialization;

namespace IPA.Bcfier.Navisworks
{
    public class NavisworksTaskQueueHandler
    {
        public Queue<Func<string, Task>> CreateNavisworksViewpointCallbacks { get; } = new Queue<Func<string, Task>>();
        public Queue<ShowViewpointQueueItem> ShowViewpointQueueItems { get; } = new Queue<ShowViewpointQueueItem>();
        private bool shouldUnregister = false;

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

            if (ShowViewpointQueueItems.Count > 0)
            {
                var uiDocument = Application.ActiveDocument;
                var showViewpointQueueItem = ShowViewpointQueueItems.Dequeue();
                HandleShowNavisworksViewpointCallback(showViewpointQueueItem.Callback, showViewpointQueueItem.Viewpoint, uiDocument);
            }
        }

        public void UnregisterEventHandler()
        {
            shouldUnregister = true;
        }

        private void HandleCreateNavisworksViewpointCallback(Func<string, Task> callback, Document uiDocument)
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

        private void HandleShowNavisworksViewpointCallback(Func<Task>? callback, BcfViewpoint? viewpoint, Document uiDocument)
        {
            if (callback == null || viewpoint == null)
            {
                return;
            }

            var viewpointService = new NavisworksViewpointDisplayService(uiDocument);
            viewpointService.DisplayViewpoint(viewpoint);
            Task.Run(async () => await callback());
        }
    }
}

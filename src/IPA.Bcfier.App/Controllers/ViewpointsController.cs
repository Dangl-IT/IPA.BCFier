using Dangl.Data.Shared;
using ElectronNET.API;
using IPA.Bcfier.App.Configuration;
using IPA.Bcfier.App.Hubs;
using IPA.Bcfier.App.Services;
using IPA.Bcfier.Ipc;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Models.Clashes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/viewpoints")]
    public class ViewpointsController : ControllerBase
    {
        private readonly RevitParameters _revitParameters;
        private readonly NavisworksParameters _navisworksParameters;
        private readonly AppParameters _appParameters;
        private readonly IServiceProvider _serviceProvider;

        public ViewpointsController(RevitParameters revitParameters,
            NavisworksParameters navisworksParameters,
            AppParameters appParameters,
            IServiceProvider serviceProvider)
        {
            _revitParameters = revitParameters;
            _navisworksParameters = navisworksParameters;
            _appParameters = appParameters;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("visualization")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ShowViewpointAsync([FromBody] BcfViewpoint viewpoint)
        {
            using var ipcHandler = GetIpcHandler();
            await ipcHandler.InitializeAsync();

            var correlationId = Guid.NewGuid();
            await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = correlationId,
                Command = IpcMessageCommand.ShowViewpoint,
                Data = JsonConvert.SerializeObject(viewpoint)
            }));

            if (_revitParameters.IsConnectedToRevit)
            {
                // We want to put revit into focus after the message was sent so the UI thread is
                // active and our request is processed
                RevitFocusService.SetFocusToRevit();
            }

            var hasReceived = false;
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(120) && !hasReceived)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.CorrelationId == correlationId)
                    {
                        hasReceived = true;
                        return NoContent();
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                        await Task.Delay(100);
                    }
                }
            }

            return BadRequest();
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(BcfViewpoint), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateViewpointAsync()
        {
            using var ipcHandler = GetIpcHandler();
            await ipcHandler.InitializeAsync();

            var correlationId = Guid.NewGuid();
            await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = correlationId,
                Command = IpcMessageCommand.CreateViewpoint,
                Data = null
            }));

            if (_revitParameters.IsConnectedToRevit)
            {
                // We want to put revit into focus after the message was sent so the UI thread is
                // active and our request is processed
                RevitFocusService.SetFocusToRevit();
            }

            var hasReceived = false;
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(120) && !hasReceived)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.CorrelationId == correlationId)
                    {
                        hasReceived = true;
                        var bcfViewpoint = JsonConvert.DeserializeObject<BcfViewpoint>(ipcMessage.Data!.ToString())!;
                        return Ok(bcfViewpoint);
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                        await Task.Delay(100);
                    }
                }
            }

            return BadRequest();
        }

        [HttpGet("navisworks-clashes")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<NavisworksClashSelection>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailableNavisworksClashesAsync()
        {
            if (!_navisworksParameters.IsConnectedToNavisworks)
            {
                return BadRequest(new ApiError("The app is currently not connected to Navisworks"));
            }

            using var ipcHandler = GetIpcHandler();
            await ipcHandler.InitializeAsync();

            var correlationId = Guid.NewGuid();
            await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = correlationId,
                Command = IpcMessageCommand.GetNavisworksAvailableClashes,
                Data = null
            }));

            var hasReceived = false;
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(120) && !hasReceived)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.CorrelationId == correlationId)
                    {
                        hasReceived = true;
                        var bcfViewpoint = JsonConvert.DeserializeObject<List<NavisworksClashSelection>>(ipcMessage.Data!)!;
                        return Ok(bcfViewpoint);
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                        await Task.Delay(100);
                    }
                }
            }

            return BadRequest();
        }

        [HttpDelete("navisworks-clashes/{clashId}")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CancelNavisworksClashDetectionAsync(Guid clashId)
        {
            if (!_navisworksParameters.IsConnectedToNavisworks)
            {
                return BadRequest(new ApiError("The app is currently not connected to Navisworks"));
            }

            using var ipcHandler = GetIpcHandler();
            await ipcHandler.InitializeAsync();

            var correlationId = Guid.NewGuid();
            await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = correlationId,
                Command = IpcMessageCommand.NavisworksClashIssuesCancellation,
                Data = JsonConvert.SerializeObject(clashId)
            }));

            return NoContent();
        }

        [HttpPost("navisworks-clashes")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<BcfTopic>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateNavisworksClashDetectionResultIssuesAsync([FromBody] NavisworksClashCreationData model)
        {
            if (!_navisworksParameters.IsConnectedToNavisworks)
            {
                return BadRequest(new ApiError("The app is currently not connected to Navisworks"));
            }

            if (model == null || model.ClashId == Guid.Empty)
            {
                return BadRequest(new ApiError("The model is invalid"));
            }

            using var ipcHandler = GetIpcHandler();
            await ipcHandler.InitializeAsync();

            var correlationId = Guid.NewGuid();
            await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
            {
                CorrelationId = correlationId,
                Command = IpcMessageCommand.CreateNavisworksClashDetectionIssues,
                Data = JsonConvert.SerializeObject(model)
            }));

            using (var scope = _serviceProvider.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BcfierHub>>();
                await hubContext.Clients.All.SendAsync("NavisworksClashIssuesCorrelationId", correlationId, model.ClashId);
            }

            var hasReceived = false;
            var start = DateTime.UtcNow;
            // We're waiting up to 20 minutes for the results here - could take a while for large
            // clash results TODO FOR MANY CLASHES, WE NEED TO EXTEND THIS DELAY HERE AND MAKE IT
            // CANCELLABLE BY THE USER FOR LOTS OF TESTS, 120s is too little, which just results in
            // the user never receiving the data
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(1200) && !hasReceived)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.CorrelationId == correlationId)
                    {
                        if (ipcMessage.Command == IpcMessageCommand.NavisworksClashIssuesTotalCount)
                        {
                            var totalCount = JsonConvert.DeserializeObject<int>(ipcMessage.Data!);
                            using var scope = _serviceProvider.CreateScope();
                            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BcfierHub>>();
                            await hubContext.Clients.All.SendAsync("NavisworksClashIssuesTotalCount", totalCount);
                        }
                        else if (ipcMessage.Command == IpcMessageCommand.NavisworksClashIssuesCurrentCount)
                        {
                            var currentCount = JsonConvert.DeserializeObject<int>(ipcMessage.Data!);
                            using var scope = _serviceProvider.CreateScope();
                            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BcfierHub>>();
                            await hubContext.Clients.All.SendAsync("NavisworksClashIssuesCurrentCount", currentCount);
                        }
                        else
                        {
                            hasReceived = true;
                            var bcfViewpoint = JsonConvert.DeserializeObject<List<BcfTopic>>(ipcMessage.Data!)!;
                            return Ok(bcfViewpoint);
                        }
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                        await Task.Delay(100);
                    }
                }
            }

            return BadRequest();
        }

        private IpcHandler GetIpcHandler()
        {
            if (_revitParameters.IsConnectedToRevit)
            {
                return new IpcHandler(thisAppName: "BcfierApp", otherAppName: "Revit", _appParameters.ApplicationId);
            }

            // We're assuming it's Navisworks then, since we don't have another possibility at the moment
            return new IpcHandler(thisAppName: "BcfierAppNavisworks", otherAppName: "Navisworks", _appParameters.ApplicationId);
        }
    }
}

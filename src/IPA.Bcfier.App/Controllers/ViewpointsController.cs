using Dangl.Data.Shared;
using IPA.Bcfier.App.Configuration;
using IPA.Bcfier.Ipc;
using IPA.Bcfier.Models.Bcf;
using Microsoft.AspNetCore.Mvc;
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

        public ViewpointsController(RevitParameters revitParameters,
            NavisworksParameters navisworksParameters)
        {
            _revitParameters = revitParameters;
            _navisworksParameters = navisworksParameters;
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

            var hasReceived = false;
            var start = DateTime.Now;
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

            var hasReceived = false;
            var start = DateTime.Now;
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
                    }
                }
            }

            return BadRequest();
        }

        private IpcHandler GetIpcHandler()
        {
            if (_revitParameters.IsConnectedToRevit)
            {
                return new IpcHandler(thisAppName: "BcfierApp", otherAppName: "Revit");

            }

            // We're assuming it's Navisworks then, since we don't have another possibility
            // at the moment
            return new IpcHandler(thisAppName: "BcfierAppNavisworks", otherAppName: "Navisworks");
        }
    }
}

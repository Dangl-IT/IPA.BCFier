using IPA.Bcfier.App.Hubs;
using IPA.Bcfier.Ipc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace IPA.Bcfier.App.Services
{
    public class PluginErrorListenerService : IHostedService
    {
        private bool _isListening;
        private readonly IServiceProvider _serviceProvider;

        public PluginErrorListenerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _isListening = true;
            Task.Run(() => ListenAsync());
            return Task.CompletedTask;
        }

        private async Task ListenAsync()
        {
            while (_isListening)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.Command == IpcMessageCommand.PluginErrorEncountered)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BcfierHub>>();
                        await hubContext.Clients.All.SendAsync("InternalError", ipcMessage.Data);
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                    }
                }


                await Task.Delay(500);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isListening = true;
            return Task.CompletedTask;
        }
    }
}

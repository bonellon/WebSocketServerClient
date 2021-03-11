using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoiceModTest.Host.services;

namespace VoiceModTest.Host
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            var serviceProvider = Startup.ConfigureServices(services);

            using var applicationScope = serviceProvider.CreateScope();

            var logger = applicationScope.ServiceProvider.GetService<ILogger<Program>>();

            //get port from args
            var port = 8181;
            var available = CheckIfPortAvailable(port);

            if (available)
            {
                var serverUri = $"ws://0.0.0.0:{port}";
                logger.LogInformation($"Initialising new Server at URI: {serverUri}");
                new MessagingServer(applicationScope.ServiceProvider.GetService<ILogger<MessagingServer>>(), serverUri);
            }

            var client = new MessageClient();
            var cancellationTokenSource = new CancellationTokenSource(5000);
            await client.Connect(cancellationTokenSource.Token)
                .ContinueWith((t) => cancellationTokenSource.Dispose());

            if (client.GetClientState() == WebSocketState.Open)
            {

                cancellationTokenSource = new CancellationTokenSource(5000);
                await client.Disconnect(cancellationTokenSource.Token)
                    .ContinueWith((t) => cancellationTokenSource.Dispose()); ;
            }
            Console.WriteLine("WebSocket CLOSED");
        }

        private static bool CheckIfPortAvailable(int port)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var endpoint = properties.GetActiveTcpListeners();
            var ports = endpoint.Select(p => p.Port).ToList();
            return !ports.Contains(port);
        }
    }
}

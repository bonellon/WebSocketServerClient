using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoiceModTest.Contracts;
using VoiceModTest.Host.services;

namespace VoiceModTest.Host
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;
        private readonly IMessageServer _messageServer;
        private readonly IMessageClient _messageClient;

        public Runner(ILogger<Runner> logger, IMessageServer messageServer, IMessageClient messageClient)
        {
            _logger = logger;
            _messageServer = messageServer;
            _messageClient = messageClient;
        }

        public async Task RunAsync(int port)
        {
            var available = CheckIfPortAvailable(port);
            var serverUri = $"ws://127.0.0.1:{port}";

            if (available)
            {
                _logger.LogInformation($"Initialising new Server at URI: {serverUri}");
                _messageServer.Start(serverUri);
            }

            var cancellationTokenSource = new CancellationTokenSource(5000);
            await _messageClient.Connect(serverUri, cancellationTokenSource.Token)
                .ContinueWith((t) => cancellationTokenSource.Dispose());

            if (_messageClient.GetClientState() == WebSocketState.Open)
            {
                //allow 5000ms before cancelling a disconnection request. 
                cancellationTokenSource = new CancellationTokenSource(5000);
                await _messageClient.Disconnect(cancellationTokenSource.Token)
                    .ContinueWith((t) => cancellationTokenSource.Dispose()); ;
            }

            if (available)
            {
                _messageServer.Shutdown();
            }

            Console.WriteLine("Session closed. Goodbye!");
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

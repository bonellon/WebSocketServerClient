using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using VoiceModTest.Host.services;

namespace VoiceModTest.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //get port from args
            var port = 8181;
            var available = CheckIfPortAvailable(port);

            if (available)
            {
                new MessagingServer($"ws://0.0.0.0:{port}");
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

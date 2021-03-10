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
            await client.Connect(new CancellationToken());

            if (client.GetClientState() == WebSocketState.Open)
            {
                await client.Disconnect(new CancellationToken());
            }
            Console.WriteLine("WebSocket CLOSED");
        }

        private static bool CheckIfPortAvailable(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] end_point = properties.GetActiveTcpListeners();
            var ports = end_point.Select(p => p.Port).ToList<int>();
            if (!ports.Contains(port))
            {
                return true;
            }
            return false;
        }
    }
}

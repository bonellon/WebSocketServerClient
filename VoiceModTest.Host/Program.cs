using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Fleck;

namespace VoiceModTest.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //get port from args
            var port = 8181;
            var available = CheckIfPortAvailable(port);

            if (available)
            {
                var server = new WebSocketServer($"ws://0.0.0.0:{port}");
                server.Start(socket =>
                {
                    socket.OnOpen = () => Console.WriteLine("Open!");
                    socket.OnClose = () => Console.WriteLine("Close!");
                    socket.OnMessage = message => socket.Send(message);
                });
            }

            var wscli = new ClientWebSocket();
            var tokSrc = new CancellationTokenSource();
            var task = wscli.ConnectAsync(new Uri($"ws://localhost:{port}"), tokSrc.Token);
            task.Wait(); task.Dispose();
            Console.WriteLine($"WebSocket to ws://localhost:{port} OPEN!");
            Console.WriteLine("SubProtocol: " + wscli.SubProtocol ?? "");

            Console.WriteLine(@"Type ""exit<Enter>"" to quit... ");
            for (var inp = Console.ReadLine(); inp != "exit"; inp = Console.ReadLine())
            {
                var inputBytes = Encoding.UTF8.GetBytes(inp);
                task = wscli.SendAsync(
                            new ArraySegment<byte>(inputBytes),
                            WebSocketMessageType.Text,
                            true,
                            tokSrc.Token
                        );
                task.Wait(); task.Dispose();

                //wait for end of message
                var buffer = new byte[2000];
                var result = wscli.ReceiveAsync(buffer, tokSrc.Token).Result;


                Console.WriteLine("**** sent msg");
                var ans = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine($"Server replied: {result.ToString()}");
            }

            if (wscli.State == WebSocketState.Open)
            {
                task = wscli.CloseAsync(WebSocketCloseStatus.NormalClosure, "", tokSrc.Token);
                task.Wait(); task.Dispose();
            }
            tokSrc.Dispose();
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

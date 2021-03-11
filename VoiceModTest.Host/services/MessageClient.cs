using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceModTest.Host.services
{
    public class MessageClient
    {
        private readonly string Username;
        private readonly string Port;
        private readonly ClientWebSocket _webSocket;

        public MessageClient()
        {

            Console.WriteLine($"Enter Username: ");
            Username = Console.ReadLine();

            _webSocket = new ClientWebSocket();
        }

        public WebSocketState GetClientState()
        {
            return _webSocket.State;
        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                await _webSocket.ConnectAsync(new Uri("ws://localhost:8181"), cancellationToken);

                var receiveMsgTask = Task.Run(async () => await ReceiveMessage());
                var sendMsgTask = Task.Run(async () => await PublishMessage());
                Console.WriteLine("Connected!");


                await Task.WhenAny(receiveMsgTask, sendMsgTask);
                Console.WriteLine("Completed.");
            }
            catch (TaskCanceledException t)
            {
                Console.WriteLine("Task cancelled", t.Message);
            }
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnect", cancellationToken);
        }

        public async Task PublishMessage()
        {
            try
            {
                Console.WriteLine(@"Type ""exit<Enter>"" to quit... ");
                for (var message = Console.ReadLine(); message != "exit"; message = Console.ReadLine())
                {

                    //Remove user input line - replace with received message. 
                    int x = Console.CursorLeft;
                    int y = Console.CursorTop == 0 ? 0 : Console.CursorTop - 1;
                    Console.SetCursorPosition(x, y);

                    var publishedMessage = $"{Username}: {message}";
                    var inputBytes = Encoding.UTF8.GetBytes(publishedMessage);

                    using (var cancellationTokenSource = new CancellationTokenSource(1000))
                    {
                        await _webSocket.SendAsync(new ArraySegment<byte>(inputBytes),
                                    WebSocketMessageType.Text,
                                    true,
                                    cancellationTokenSource.Token);
                    }
                }
            }
            catch(TaskCanceledException t)
            {
                Console.WriteLine("Task Cancelled..", t.Message);
            }
        }

        public async Task ReceiveMessage()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    while (_webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                            result = await _webSocket.ReceiveAsync(messageBuffer, CancellationToken.None);
                            ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var msgString = Encoding.UTF8.GetString(ms.ToArray());
                            Console.WriteLine(msgString);
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                        ms.SetLength(0);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }
    }
}

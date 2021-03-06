using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoiceModTest.Contracts;

namespace VoiceModTest.Host.services
{
    /// <summary>
    /// The WebSocketClient implementation
    /// </summary>
    public class MessageClient : IMessageClient
    {
        private readonly string Username;
        private readonly ClientWebSocket _webSocket;

        public MessageClient()
        {

            Console.WriteLine($"Enter Username (or enter anonymously!): ");
            Username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(Username))
                Username = "anon";

            _webSocket = new ClientWebSocket();
        }

        /// <summary>
        /// Get The Current Client State
        /// </summary>
        /// <returns><see cref=”WebSocketState”/></returns>
        public WebSocketState GetClientState()
        {
            return _webSocket.State;
        }

        /// <summary>
        /// Connect to the web socket server at the specified uri
        /// </summary>
        /// <param name="uri">The server's URI</param>
        /// <param name="cancellationToken">Request CancellationToken</param>
        /// <returns></returns>
        public async Task Connect(string uri, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                await _webSocket.ConnectAsync(new Uri($"{uri}/{Username}"), cancellationToken);

                var receiveMsgTask = Task.Run(async () => await ReceiveMessage());
                var sendMsgTask = Task.Run(async () => await BroadcastMessage());
                Console.WriteLine("Connected!");


                await Task.WhenAny(receiveMsgTask, sendMsgTask);
                Console.WriteLine("Completed.");
            }
            catch (TaskCanceledException t)
            {
                Console.WriteLine("Task cancelled", t.Message);
            }
        }

        /// <summary>
        /// Disconnect from the web socket server
        /// </summary>
        /// <param name="cancellationToken">Request CancellationToken</param>
        /// <returns></returns>
        public async Task Disconnect(CancellationToken cancellationToken)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnect", cancellationToken);
        }

        /// <summary>
        /// Broadcast message to everyone currently connected to the same server. 
        /// Waits for the user to enter a message before publishing to the server.
        /// This is run indefinitely until the client or server closes the connection.
        /// </summary>
        /// <returns></returns>
        public async Task BroadcastMessage()
        {
            try
            {
                Console.WriteLine("Type \"exit<Enter>\" to terminate the session");
                for (var message = Console.ReadLine(); message != "exit"; message = Console.ReadLine())
                {
                    //Remove user input line - replace with received message. 
                    int x = Console.CursorLeft;
                    int y = Console.CursorTop == 0 ? 0 : Console.CursorTop - 1;
                    Console.SetCursorPosition(x, y);

                    var publishedMessage = $"{Username}: {message}";
                    var inputBytes = Encoding.UTF8.GetBytes(publishedMessage);

                    using var cancellationTokenSource = new CancellationTokenSource(1000);
                    await _webSocket.SendAsync(new ArraySegment<byte>(inputBytes),
                                WebSocketMessageType.Text,
                                true,
                                cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException t)
            {
                Console.WriteLine("Task Cancelled", t.Message);
            }
        }

        /// <summary>
        /// Web socket implementation to receive any messages that are sent by other clients to the server.
        /// This is run indefinitely until the client or server closes the connection.
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveMessage()
        {
            try
            {
                using var ms = new MemoryStream();
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
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}.");
                throw;
            }
        }
    }
}

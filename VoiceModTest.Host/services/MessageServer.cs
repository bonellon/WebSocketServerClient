using Fleck;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using VoiceModTest.Contracts;

namespace VoiceModTest.Host.services
{
    /// <summary>
    /// The WebSocketServer implementation that handles all requests
    /// </summary>
    public class MessageServer : IMessageServer
    {
        private List<IWebSocketConnection> _webConnections = new List<IWebSocketConnection>();
        private readonly ILogger<MessageServer> _logger;
        private WebSocketServer _server;

        public MessageServer(ILogger<MessageServer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Start the WebSocketServer at the specified URI
        /// </summary>
        /// <param name="uri">The server uri</param>
        public void Start(string uri)
        {
            _server = new WebSocketServer(uri);

            _server.Start(socket =>
            {
                socket.OnOpen = () => AddClientConnection(socket);

                socket.OnClose = () => DeleteClientConnection(socket);

                socket.OnMessage = message =>
                {
                    _logger.LogInformation($"Received message from: {socket.ConnectionInfo.Id}");
                    _webConnections.ForEach(s => s.Send(message));
                };

                socket.OnBinary = bytes =>
                {
                    _logger.LogInformation($"Received binary message from: {socket.ConnectionInfo.Id}");
                    BroadcastMessage($"{socket.ConnectionInfo.Id} : {Encoding.UTF8.GetString(bytes)}");
                };

                socket.OnError = exception =>
                {
                    _logger.LogError($"Unhandled Socket Exception: {socket.ConnectionInfo.Id}", exception.Message);
                };
            });


            _logger.LogInformation("Server running. Awaiting connections...");
        }

        /// <summary>
        /// Shutdown the server. Connected clients will be disposed correctly
        /// </summary>
        public void Shutdown()
        {
            _logger.LogInformation("Server shutting down, closing outstanding connections...");
            _webConnections.ForEach(s => s.Close());
            _logger.LogInformation("Done. Server shutdown complete");
            _server.Dispose();
        }

        /// <summary>
        /// Add a new websocket client.
        /// This method adds the new client to the list of ongoing connections and broadcasts a message to inform all clients of a new login.
        /// </summary>
        /// <param name="socket">The new socket connection</param>
        private void AddClientConnection(IWebSocketConnection socket)
        {
            _webConnections.Add(socket);

            var username = GetUsername(socket.ConnectionInfo.Path);
            var message = $"New client connection: {username} : {socket.ConnectionInfo.Id}, there are {_webConnections.Count} active connections";
            _logger.LogInformation(message);
            BroadcastMessage(message);
        }

        /// <summary>
        /// Discconect a websocket client.
        /// This method removes the client from the list of ongoing connections and broadcasts a message to inform all clients of a new logout.
        /// </summary>
        /// <param name="socket"></param>
        private void DeleteClientConnection(IWebSocketConnection socket)
        {

            _webConnections.Remove(socket);
            var username = GetUsername(socket.ConnectionInfo.Path);
            var message = $"Client disconnected: {username} : {socket.ConnectionInfo.Id}, there are {_webConnections.Count} active connections";
            _logger.LogInformation(message);
            BroadcastMessage(message);
        }

        private void BroadcastMessage(string message)
        {
            _webConnections.ForEach(s => s.Send(message));
        }

        private string GetUsername(string path)
        {
            var username = path.Split('/');
            return username.Length == 2 ? username[1] : "anon"; 
        }
    }
}

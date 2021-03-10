using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceModTest.Host.services
{
    public class MessagingServer
    {
        private List<IWebSocketConnection> _webConnections;
        private FleckLog _logger;
        private WebSocketServer _server;

        //On first connection ask for the name and map the name -> ConnectionGuid in server
        public MessagingServer(string serverURI)
        {
            _webConnections = new List<IWebSocketConnection>();
            _logger = new FleckLog();
            _server = new WebSocketServer(serverURI);
            Start();
        }

        public void Start()
        {
            _server.Start(socket =>
            {
                socket.OnOpen = () => AddClientConnection(socket);

                socket.OnClose = () => DeleteClientConnection(socket);

                socket.OnMessage = message =>
                {
                    _webConnections.ForEach(s => s.Send(message));
                };

                socket.OnBinary = bytes =>
                {
                    Console.WriteLine($"Received binary message from: {socket.ConnectionInfo.Id}");
                    _webConnections.ForEach(s => s.Send($"{socket.ConnectionInfo.Id} : {Encoding.UTF8.GetString(bytes)}"));
                };

                socket.OnError = exception =>
                    Console.WriteLine($"OnError {exception.Message}");

                //socket.OnPing = bytes =>
                  //  socket.Send("Pong");

               // socket.OnPong = bytes =>
                 //   socket.Send("Ping");
            });


            Console.WriteLine("Server established. Awaiting connections...");
        }

        public void AddClientConnection(IWebSocketConnection socket)
        {
            Console.WriteLine($"New client connection: {socket.ConnectionInfo.Id}");
            _webConnections.Add(socket);
        }

        //??
        public void DeleteClientConnection(IWebSocketConnection socket)
        {
            Console.WriteLine($"Client disconnected: {socket.ConnectionInfo.Id}");
            _webConnections.Remove(socket);
        }

        public void Shutdown()
        {
            _server.Dispose();
        }
    }
}

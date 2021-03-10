using System;

using Fleck;

namespace VoiceModTest.Contracts
{
    public interface ISocketServer
    {

        Action OpenConnection(IWebSocketConnection socket);
        Action CloseConnection(IWebSocketConnection socket);
    }
}

using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceModTest.Contracts
{
    /// <summary>
    /// The Client interface providing a number of methods required to connect to a Fleck Server
    /// </summary>
    public interface IMessageClient
    {
        WebSocketState GetClientState();
        Task Connect(string uri, CancellationToken cancellationToken);
        Task Disconnect(CancellationToken cancellationToken);
        Task BroadcastMessage();
        Task ReceiveMessage();
    }
}

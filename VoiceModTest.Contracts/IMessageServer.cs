namespace VoiceModTest.Contracts
{
    /// <summary>
    /// The Server interface containing the required methods to host a Fleck Server
    /// </summary>
    public interface IMessageServer
    {
        void Start(string uri);
        void Shutdown();
    }
}

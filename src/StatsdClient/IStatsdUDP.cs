namespace StatsdClient
{
    public interface IStatsdUDP
    {
        int MaxUDPPacketSize { get; }

        void Send(string command);
    }
}
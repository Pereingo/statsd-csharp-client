namespace StatsdClient
{
    public interface IStatsdUDP
    {
        void Send(string command);
    }
}
using System.Threading.Tasks;

namespace StatsdClient
{
    public interface IStatsdUDP
    {
        void Send(string command);
#if !NET451
        Task SendAsync(string command);
#endif
    }
}
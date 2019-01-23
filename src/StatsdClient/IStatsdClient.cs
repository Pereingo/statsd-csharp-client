using System;
using System.Threading.Tasks;

namespace StatsdClient
{
    public interface IStatsdClient : IDisposable
    {
        //void Send(string command);
        Task SendAsync(string command);
    }
}

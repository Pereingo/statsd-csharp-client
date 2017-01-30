using System;

namespace StatsdClient
{
    public interface IStatsdClient : IDisposable
    {
        void Send(string command);
    }
}

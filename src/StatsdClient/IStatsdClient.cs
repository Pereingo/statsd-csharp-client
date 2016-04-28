using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient
{
    public interface IStatsdClient : IDisposable
    {
        void Send(string command);
    }
}

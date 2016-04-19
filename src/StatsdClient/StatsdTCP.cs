using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdTCPClient : IDisposable, IStatsdClient
    {
        public IPEndPoint IPEndpoint { get; private set; }

        private readonly Socket _clientSocket;
        private readonly string _name;
        private readonly int _port;

        public StatsdTCPClient(string name, int port = 8125)
        {
            _name = name;
            _port = port;

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var ipAddress = GetIpv4Address(name);
            IPEndpoint = new IPEndPoint(ipAddress, _port);
        }

        private IPAddress GetIpv4Address(string name)
        {
            IPAddress ipAddress;
            var isValidIpAddress = IPAddress.TryParse(name, out ipAddress);

            if (!isValidIpAddress)
            {
                ipAddress = GetIpFromHostname();
            }

            return ipAddress;
        }

        private IPAddress GetIpFromHostname()
        {
            var addressList = Dns.GetHostEntry(_name).AddressList;
            var ipv4Addresses = addressList.Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);

            return ipv4Addresses.First();
        }

        public void Send(string command)
        {
            Send(Encoding.ASCII.GetBytes(command));
        }

        private void Send(byte[] encodedCommand)
        {
            _clientSocket.Connect(IPEndpoint.Address, _port);
            _clientSocket.SendTo(encodedCommand, encodedCommand.Length, SocketFlags.None, IPEndpoint);
            _clientSocket.Disconnect(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StatsdTCP() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

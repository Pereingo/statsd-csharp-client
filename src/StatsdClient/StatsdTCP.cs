using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdTCPClient : Address, IStatsdClient
    {
        public IPEndPoint IPEndpoint { get; private set; }
        private readonly Socket _clientSocket;

        public StatsdTCPClient(string name, int port = 8125)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndpoint = new IPEndPoint(GetIpv4Address(name), port);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        public void Send(string command)
        {
            Send(Encoding.ASCII.GetBytes(command));
        }

        private void Send(byte[] encodedCommand)
        {
            try
            {
                _clientSocket.Connect(IPEndpoint);
                _clientSocket.SendTo(encodedCommand, encodedCommand.Length, SocketFlags.None, IPEndpoint);
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            finally
            {
                _clientSocket.Disconnect(false);
            }
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

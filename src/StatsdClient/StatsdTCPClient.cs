using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdTCPClient : IStatsdClient
    {
        private IPEndPoint IpEndpoint { get; }
        private readonly Socket _clientSocket;

        public StatsdTCPClient(string name, int port = 8125)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IpEndpoint = new IPEndPoint(AddressResolution.GetIpv4Address(name), port);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
                _clientSocket.Connect(IpEndpoint);
                _clientSocket.SendTo(encodedCommand, encodedCommand.Length, SocketFlags.None, IpEndpoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                CloseSocket(_clientSocket);
            }
        }

        private static void CloseSocket(Socket socket)
        {
#if NETFULL
            socket.Close();
#else
            socket.Dispose();
#endif
        }

        #region IDisposable Support
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_clientSocket != null)
                {
                    try
                    {
                        CloseSocket(_clientSocket);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            _disposed = true;
        }

        ~StatsdTCPClient() {
          Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

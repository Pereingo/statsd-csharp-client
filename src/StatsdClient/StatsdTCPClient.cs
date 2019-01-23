using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StatsdClient
{
    public class StatsdTCPClient : IStatsdClient
    {
        private readonly Task<IPEndPoint> _ipEndpoint;
        private readonly Socket _clientSocket;

        public StatsdTCPClient(string name, int port = 8125)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _ipEndpoint = AddressResolution.GetIpv4EndPoint(name, port);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Send(string command) => SendAsync(Encoding.ASCII.GetBytes(command)).GetAwaiter().GetResult();

        public Task SendAsync(string command) => SendAsync(Encoding.ASCII.GetBytes(command));

        private async Task SendAsync(byte[] encodedCommand)
        {
            try
            {
                var ipEndpoint = await _ipEndpoint.ConfigureAwait(false);

                await _clientSocket.ConnectAsync(ipEndpoint).ConfigureAwait(false);
                await _clientSocket.SendToAsync(new ArraySegment<byte>(encodedCommand), SocketFlags.None, ipEndpoint).ConfigureAwait(false);
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
#if NETFRAMEWORK
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

        ~StatsdTCPClient()
        {
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

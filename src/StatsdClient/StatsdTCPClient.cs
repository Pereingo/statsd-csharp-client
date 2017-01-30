using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdTCPClient : Address, IStatsdClient
    {
        private IPEndPoint IpEndpoint { get; }
        private readonly Socket _clientSocket;

        public StatsdTCPClient(string name, int port = 8125)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IpEndpoint = new IPEndPoint(GetIpv4Address(name), port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
#if NETFULL
                _clientSocket.Close();
#else
                _clientSocket.Dispose();
#endif
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
                    if (_clientSocket != null)
                    {
                        try
                        {
#if NETFULL
							_clientSocket.Close();
#else
							_clientSocket.Dispose();
#endif
						}
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                disposedValue = true;
            }
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

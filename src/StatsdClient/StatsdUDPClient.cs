using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StatsdClient
{
    public class StatsdUDPClient : IStatsdClient
    {
        private readonly Task<IPEndPoint> _ipEndpoint;
        private readonly int _maxUdpPacketSizeBytes;
        private readonly Socket _clientSocket;

        /// <summary>
        /// Creates a new StatsdUDP class for lower level access to statsd.
        /// </summary>
        /// <param name="name">Hostname or IP (v4) address of the statsd server.</param>
        /// <param name="port">Port of the statsd server. Default is 8125.</param>
        /// <param name="maxUdpPacketSizeBytes">Max packet size, in bytes. This is useful to tweak if your MTU size is different than normal. Set to 0 for no limit. Default is MetricsConfig.DefaultStatsdMaxUDPPacketSize.</param>
        public StatsdUDPClient(string name, int port = 8125, int maxUdpPacketSizeBytes = MetricsConfig.DefaultStatsdMaxUDPPacketSize)
        {
            _maxUdpPacketSizeBytes = maxUdpPacketSizeBytes;

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _ipEndpoint = AddressResolution.GetIpv4EndPoint(name, port);
        }

        public void Send(string command) => SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(command))).GetAwaiter().GetResult();

        public Task SendAsync(string command) => SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(command)));

        private async Task SendAsync(ArraySegment<byte> encodedCommand)
        {
            if (_maxUdpPacketSizeBytes > 0 && encodedCommand.Count > _maxUdpPacketSizeBytes)
            {
                // If the command is too big to send, linear search backwards from the maximum
                // packet size to see if we can find a newline delimiting two stats. If we can,
                // split the message across the newline and try sending both componenets individually
                for (var i = _maxUdpPacketSizeBytes; i > 0; i--)
                {
                    if (encodedCommand.Array[encodedCommand.Offset + i] != '\n')
                    {
                        continue;
                    }

                    await SendAsync(new ArraySegment<byte>(encodedCommand.Array, encodedCommand.Offset, i)).ConfigureAwait(false);

                    var remainingCharacters = encodedCommand.Count - i - 1;
                    if (remainingCharacters > 0)
                    {
                        await SendAsync(new ArraySegment<byte>(encodedCommand.Array, encodedCommand.Offset + i + 1, remainingCharacters)).ConfigureAwait(false);
                    }

                    return; // We're done here if we were able to split the message.
                    // At this point we found an oversized message but we weren't able to find a
                    // newline to split upon. We'll still send it to the UDP socket, which upon sending an oversized message
                    // will fail silently if the user is running in release mode or report a SocketException if the user is
                    // running in debug mode.
                    // Since we're conservative with our MAX_UDP_PACKET_SIZE, the oversized message might even
                    // be sent without issue.
                }
            }

            await _clientSocket.SendToAsync(encodedCommand, SocketFlags.None, await _ipEndpoint.ConfigureAwait(false)).ConfigureAwait(false);
        }

        //reference : https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~StatsdUDPClient()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_clientSocket != null)
                {
                    try
                    {
#if NETFRAMEWORK
                        _clientSocket.Close();
#else
                        _clientSocket.Dispose();
#endif
                    }
                    catch (Exception)
                    {
                        //Swallow since we are not using a logger, should we add LibLog and start logging??
                    }

                }
            }

            _disposed = true;
        }
    }
}
#if NET45
namespace System.Net.Sockets
{
    public static class SocketTaskExtensions
    {
        public static Task<Int32> SendToAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return Task.Factory.FromAsync((arg1, arg2, arg3, callback, state) => socket.BeginSendTo(arg1.Array, arg1.Offset, arg1.Count, arg2, arg3, callback, state),
                socket.EndSendTo, buffer, socketFlags, remoteEP, null);
        }
    }
}
#endif

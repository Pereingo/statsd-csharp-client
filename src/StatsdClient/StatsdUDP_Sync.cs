using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial class StatsdUDP
    {
        public IPEndPoint IPEndpoint
        {
            get
            {
                if(_ipEndpoint == null) throw new Exception("Plaese call InitializeAsync() before using StatsdUDP");
                return _ipEndpoint;
            }
            private set { _ipEndpoint = value; }
        }

        private readonly int _maxUdpPacketSizeBytes;
        private readonly Socket _udpSocket;
        private readonly string _name;
        private readonly int _port;

        /// <summary>
        /// Creates a new StatsdUDP class for lower level access to statsd.
        /// </summary>
        /// <param name="name">Hostname or IP (v4) address of the statsd server.</param>
        /// <param name="port">Port of the statsd server. Default is 8125.</param>
        /// <param name="maxUdpPacketSizeBytes">Max packet size, in bytes. This is useful to tweak if your MTU size is different than normal. Set to 0 for no limit. Default is MetricsConfig.DefaultStatsdMaxUDPPacketSize.</param>
        public StatsdUDP(
            string name, 
            int port = 8125, 
            int maxUdpPacketSizeBytes = MetricsConfig.DefaultStatsdMaxUDPPacketSize)
        {
            _name = name;
            _port = port;
            _maxUdpPacketSizeBytes = maxUdpPacketSizeBytes;

            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

#if NET451
            IPAddress ipAddress = GetIpv4Address(_name);
            IPEndpoint = new IPEndPoint(ipAddress, _port);
#endif
        }

#if NET451

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
            var hostEntry = Dns.GetHostEntry(_name);
            var addressList = hostEntry.AddressList;
            var ipv4Addresses = addressList.Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);

            return ipv4Addresses.First();
        }
        
        #endif        

        public void Send(string command)
        {
            Send(Encoding.ASCII.GetBytes(command));
        }

        private void Send(byte[] encodedCommand)
        {
            if (_maxUdpPacketSizeBytes > 0 && encodedCommand.Length > _maxUdpPacketSizeBytes)
            {
                // If the command is too big to send, linear search backwards from the maximum
                // packet size to see if we can find a newline delimiting two stats. If we can,
                // split the message across the newline and try sending both componenets individually
                var newline = Encoding.ASCII.GetBytes("\n")[0];
                for (var i = _maxUdpPacketSizeBytes; i > 0; i--)
                {
                    if (encodedCommand[i] != newline)
                    {
                        continue;
                    }

                    var encodedCommandFirst = new byte[i];
                    Array.Copy(encodedCommand, encodedCommandFirst, encodedCommandFirst.Length); // encodedCommand[0..i-1]
                    Send(encodedCommandFirst);

                    var remainingCharacters = encodedCommand.Length - i - 1;
                    if (remainingCharacters > 0)
                    {
                        var encodedCommandSecond = new byte[remainingCharacters];
                        Array.Copy(encodedCommand, i + 1, encodedCommandSecond, 0, encodedCommandSecond.Length); // encodedCommand[i+1..end]
                        Send(encodedCommandSecond);
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

            _udpSocket.SendTo(encodedCommand, SocketFlags.None, IPEndpoint);
        }

        //reference : https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
        private bool _disposed;
        private IPEndPoint _ipEndpoint;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~StatsdUDP() 
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
                if (_udpSocket != null)
                {
                    try
                    {
                        _udpSocket.Dispose();
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
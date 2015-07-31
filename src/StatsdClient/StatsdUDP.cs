using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdUDP : IDisposable, IStatsdUDP
    {
        public int MaxUDPPacketSize { get; private set; } // In bytes; default is MetricsConfig.DefaultStatsdMaxUDPPacketSize.
                                                          // Set to zero for no limit.
        public IPEndPoint IPEndpoint { get; private set; }
        private Socket UDPSocket { get; set; }
        private string Name { get; set; }
        private int Port { get; set; }
        private bool _disposed;

        public StatsdUDP(string name, int port, int maxUdpPacketSize = MetricsConfig.DefaultStatsdMaxUDPPacketSize)
        {
            Name = name;
            Port = port;
            MaxUDPPacketSize = maxUdpPacketSize;

            UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var ipAddress = GetIpv4Address(name);

            IPEndpoint = new IPEndPoint(ipAddress, Port);
        }

        private IPAddress GetIpv4Address(string name)
        {
            IPAddress ipAddress;
            var isValidIpAddress = IPAddress.TryParse(name, out ipAddress);

            if (!isValidIpAddress)
            {
                var addressList = Dns.GetHostEntry(Name).AddressList;

                var positionForIpv4 = addressList.Length - 1;

                ipAddress = addressList[positionForIpv4];
            }
            return ipAddress;
        }

        public void Send(string command)
        {
            Send(Encoding.ASCII.GetBytes(command));
        }

        private void Send(byte[] encodedCommand)
        {
            if (MaxUDPPacketSize > 0 && encodedCommand.Length > MaxUDPPacketSize)
            {
                // If the command is too big to send, linear search backwards from the maximum
                // packet size to see if we can find a newline delimiting two stats. If we can,
                // split the message across the newline and try sending both componenets individually
                var newline = Encoding.ASCII.GetBytes("\n")[0];
                for (var i = MaxUDPPacketSize; i > 0; i--)
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
            UDPSocket.SendTo(encodedCommand, encodedCommand.Length, SocketFlags.None, IPEndpoint);
        }

        //reference : https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
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
                if (UDPSocket != null)
                {
                    try
                    {
                        UDPSocket.Close();
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
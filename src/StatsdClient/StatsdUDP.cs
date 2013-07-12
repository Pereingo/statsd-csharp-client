using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace StatsdClient
{
    public class StatsdUDP : IDisposable, IStatsdUDP
    {
        public const int MAX_UDP_PACKET_SIZE = 512; // in bytes
        public IPEndPoint IPEndpoint { get; private set; }
        private Socket UDPSocket { get; set; }
        private string Name { get; set; }
        private int Port { get; set; }

        public StatsdUDP(string name, int port)
        {
            Name = name;
            Port = port;

            UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var ipAddress = GetIpv4Address(name);

            IPEndpoint = new IPEndPoint(ipAddress, Port);
        }

        private IPAddress GetIpv4Address(string name)
        {
            IPAddress ipAddress;
            bool isValidIPAddress = IPAddress.TryParse(name, out ipAddress);

            if (!isValidIPAddress)
            {
                IPAddress[] addressList = Dns.GetHostEntry(Name).AddressList;

                int positionForIpv4 = addressList.Length - 1;

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
            if (encodedCommand.Length > MAX_UDP_PACKET_SIZE)
            {
                // If the command is too big to send, linear search backwards from the maximum
                // packet size to see if we can find a newline delimiting two stats. If we can,
                // split the message across the newline and try sending both componenets individually
                byte newline = Encoding.ASCII.GetBytes("\n")[0];
                for (int i = MAX_UDP_PACKET_SIZE; i > 0; i--)
                {
                    if (encodedCommand[i] == newline)
                    {
                        byte[] encodedCommandFirst = new byte[i];
                        Array.Copy(encodedCommand, encodedCommandFirst, encodedCommandFirst.Length); // encodedCommand[0..i-1]
                        Send(encodedCommandFirst);

                        int remainingCharacters = encodedCommand.Length - i - 1;
                        if (remainingCharacters > 0) 
                        {
                            byte[] encodedCommandSecond = new byte[remainingCharacters];
                            Array.Copy(encodedCommand, i + 1, encodedCommandSecond, 0, encodedCommandSecond.Length); // encodedCommand[i+1..end]
                            Send(encodedCommandSecond);
                        }

                        return; // We're done here if we were able to split the message.
                    }
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

        public void Dispose()
        {
            UDPSocket.Close();
        }
    }
}
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial class StatsdUDP : IDisposable, IStatsdUDP
    {
#if !NET451
        public async Task InitializeAsync()
        {
            IPAddress ipAddress = await GetIpv4AddressAsync(_name);
            IPEndpoint = new IPEndPoint(ipAddress, _port);
        }

        private async Task<IPAddress> GetIpv4AddressAsync(string name)
        {
            IPAddress ipAddress;
            var isValidIpAddress = IPAddress.TryParse(name, out ipAddress);

            if (!isValidIpAddress)
            {
                ipAddress = await GetIpFromHostnameAsync();
            }

            return ipAddress;
        }

        private async Task<IPAddress> GetIpFromHostnameAsync()
        {
            var hostEntry = await Dns.GetHostEntryAsync(_name);
            var addressList = hostEntry.AddressList;
            var ipv4Addresses = addressList.Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);

            return ipv4Addresses.First();
        }

        public async Task SendAsync(string command)
        {
            await SendAsync(Encoding.ASCII.GetBytes(command));
        }

        private async Task SendAsync(byte[] encodedCommand)
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
                    await SendAsync(encodedCommandFirst);

                    var remainingCharacters = encodedCommand.Length - i - 1;
                    if (remainingCharacters > 0) 
                    {
                        var encodedCommandSecond = new byte[remainingCharacters];
                        Array.Copy(encodedCommand, i + 1, encodedCommandSecond, 0, encodedCommandSecond.Length); // encodedCommand[i+1..end]
                        await SendAsync(encodedCommandSecond);
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

            ArraySegment<byte> encodedCommandSegment = new ArraySegment<byte>(encodedCommand);

            await _udpSocket.SendToAsync(encodedCommandSegment, SocketFlags.None, IPEndpoint);
        }
#endif
    }
}
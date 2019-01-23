using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StatsdClient
{
    public class AddressResolution
    {
        public static async Task<IPEndPoint> GetIpv4EndPoint(string name, int port)
        {
            if (!IPAddress.TryParse(name, out var ipAddress))
                ipAddress = await GetIpFromHostname(name).ConfigureAwait(false);

            return new IPEndPoint(ipAddress, port);
        }

        private static async Task<IPAddress> GetIpFromHostname(string name)
        {
            var hostEntry = await Dns.GetHostEntryAsync(name).ConfigureAwait(false);
            var ipv4Addresses = hostEntry.AddressList.Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);

            return ipv4Addresses.First();
        }
    }
}

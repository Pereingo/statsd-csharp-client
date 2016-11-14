using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class Address
    {
        public IPAddress GetIpv4Address(string name)
        {
            IPAddress ipAddress;
            var isValidIpAddress = IPAddress.TryParse(name, out ipAddress);

            if (!isValidIpAddress)
            {
                ipAddress = GetIpFromHostname(name);
            }

            return ipAddress;
        }

        private IPAddress GetIpFromHostname(string name)
        {
            var addressList = Dns.GetHostEntry(name).AddressList;
            var ipv4Addresses = addressList.Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);

            return ipv4Addresses.First();
        }
    }
}

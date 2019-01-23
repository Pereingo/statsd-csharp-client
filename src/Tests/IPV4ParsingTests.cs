using NUnit.Framework;
using StatsdClient;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class IPV4ParsingTests
    {
        private const int RandomUnusedLocalPort = 23483;

        [Test]
        public async Task ipv4_parsing_works_with_hostname()
        {
            var statsdUdp = await AddressResolution.GetIpv4EndPoint("localhost", RandomUnusedLocalPort);
            Assert.That(statsdUdp.Address.ToString(), Does.Contain("127.0.0.1"));
        }

        [Test]
        public async Task ipv4_parsing_works_with_ip()
        {
            var statsdUdp = await AddressResolution.GetIpv4EndPoint("127.0.0.1", RandomUnusedLocalPort);
            Assert.That(statsdUdp.Address.ToString(), Does.Contain("127.0.0.1"));
        }
    }
}
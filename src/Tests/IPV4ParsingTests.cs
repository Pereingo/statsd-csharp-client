using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class IPV4ParsingTests
    {
        private const int RandomUnusedLocalPort = 23483;

        [Test]
        public void ipv4_parsing_works_with_hostname()
        {
            var statsdUdp = new StatsdUDPClient("localhost", RandomUnusedLocalPort);
            Assert.That(statsdUdp.IPEndpoint.Address.ToString(), Does.Contain("127.0.0.1"));
        }

        [Test]
        public void ipv4_parsing_works_with_ip()
        {
            var statsdUdp = new StatsdUDPClient("127.0.0.1", RandomUnusedLocalPort);
            Assert.That(statsdUdp.IPEndpoint.Address.ToString(), Does.Contain("127.0.0.1"));
        }
    }
}
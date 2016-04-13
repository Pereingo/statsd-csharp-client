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
            var statsdUdp = new StatsDClient("localhost", RandomUnusedLocalPort);
            Assert.That(statsdUdp.IPEndpoint.Address.ToString(),Is.StringContaining("127.0.0.1"));
        }

        [Test]
        public void ipv4_parsing_works_with_ip()
        {
            var statsdUdp = new StatsDClient("127.0.0.1", RandomUnusedLocalPort);
            Assert.That(statsdUdp.IPEndpoint.Address.ToString(), Is.StringContaining("127.0.0.1"));
        }
    }
}
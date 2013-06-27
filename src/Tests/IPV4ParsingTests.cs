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
            StatsdUDP statsdUDP = new StatsdUDP("localhost", RandomUnusedLocalPort);
            Assert.That(statsdUDP.IPEndpoint.Address.ToString(),Is.StringContaining("127.0.0.1"));
        }

        [Test]
        public void ipv4_parsing_works_with_ip()
        {
            StatsdUDP statsdUDP = new StatsdUDP("127.0.0.1", RandomUnusedLocalPort);
            Assert.That(statsdUDP.IPEndpoint.Address.ToString(), Is.StringContaining("127.0.0.1"));
        }
    }
}
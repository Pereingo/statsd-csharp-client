using System.Net;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class UDPSmokeTests
    {
        // Smoke test should hit the real thing, but for the purpose of passing the appveyor build we are only checking if the client connects.
        // If you want to test against an actual system, change the host/port.

        private static readonly IPAddress ServerHostname = IPAddress.Loopback;

        [Test]
        public void Sends_counter_text()
        {
            using (var client = new StatsdUDPClient(ServerHostname.ToString()))
            {
                client.Send("statsd-client.udp-smoke-test:6|c");
            }
        }
    }
}

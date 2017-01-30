using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class TCPSmokeTests
    {
        // Smoke test should hit the real thing, but for the purpose of passing the appveyor build we are only checking if the client connects.
        // If you want to test against an actual system, change the host/port.

        private TcpListener _tcpListener;
        private static readonly IPAddress ServerHostname = IPAddress.Loopback;
        private int _serverPort;

        [OneTimeSetUp]
        public void UdpListenerThread()
        {
            const int nextAvailablePort = 0;
            _tcpListener = new TcpListener(ServerHostname, nextAvailablePort);
            _tcpListener.Start();

            _serverPort = ((IPEndPoint) _tcpListener.LocalEndpoint).Port;
        }

        [OneTimeTearDown]
        public void TearDownUdpListener()
        {
            _tcpListener.Stop();
        }

        [Test]
        public void Sends_counter_text()
        {
            using (var client = new StatsdTCPClient(ServerHostname.ToString(), _serverPort))
            {
                client.Send("statsd-client.tcp-smoke-test:6|c");
            }
        }
    }
}
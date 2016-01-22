using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StatsdClient.Core.Tests
{
    [TestFixture]
    public class UDPSmokeTests
    {
        private static readonly int _serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);
        private static readonly string _serverName = ConfigurationManager.AppSettings["StatsdServerName"];

        [Test]
        public async Task Sends_a_counter()
        {
            try
            {
                var client = new StatsdUDP(_serverName, _serverPort);
                await client.InitializeAsync();
                await client.SendAsync("socket2:1|c");
            }
            catch(SocketException ex)
            {
                Assert.Fail("Socket Exception, have you setup your Statsd name and port? It's currently '{0}:{1}'. Error: {2}", _serverName, _serverPort, ex.Message);
            }
        }
    }
}

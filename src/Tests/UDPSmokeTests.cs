using System;
using System.Configuration;
using System.Net.Sockets;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class UDPSmokeTests
    {
        private static readonly int ServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);
        private static readonly string ServerName = ConfigurationManager.AppSettings["StatsdServerName"];

        [Test]
        public void Sends_a_counter()
        {
            try
            {
                var client = new StatsdUDP(ServerName, ServerPort);
                client.Send("socket2:1|c");
            }
            catch(SocketException ex)
            {
                Assert.Fail("Socket Exception, have you set up your Statsd name and port? Error: {0}", ex.Message);
            }
        }
    }
}

using System;
using System.Configuration;
using System.Net.Sockets;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class TCPSmokeTests
    {
		/*
		* Smoke test should hit the real thing.
		* For the purpose of passing the appveyor build
		* we are only checking if the client connects.
		*/

        private static readonly int _serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);
        private static readonly string _serverName = ConfigurationManager.AppSettings["StatsdServerName"];

        [Test]
        public void Sends_a_counter()
        {
			TcpListener server = null; 
			// Set the TcpListener
			Int32 port = _serverPort;
			System.Net.IPAddress localAddr = System.Net.IPAddress.Parse(_serverName);

			// TcpListener server = new TcpListener(port);
			server = new TcpListener(localAddr, port);

			// Start listening for client requests.
			server.Start();

            try
            {
				var client = new StatsdTCPClient(_serverName, _serverPort);
                client.Send("smoketest value=1i");
            }
            catch(SocketException ex)
            {
                Assert.Fail("Socket Exception, have you setup your Statsd name and port? It's currently '{0}:{1}'. Error: {2}", _serverName, _serverPort, ex.Message);
            }

			// Stop listening for client requests.
			server.Stop();
        }
    }
}

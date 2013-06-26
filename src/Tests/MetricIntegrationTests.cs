using System;
using System.Threading;
using System.Configuration;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;


namespace Tests
{
    [TestFixture]
    public class MetricIntegrationTests
    {
        private UdpListener udpListener;
        private Thread listenThread;
        private int serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);
        private string localhostAddress = "127.0.0.1";

        [TestFixtureSetUp]
        public void SetUpUdpListener() 
        {
            udpListener = new UdpListener(localhostAddress, serverPort);
            var metricsConfig = new MetricsConfig { StatsdServerName = localhostAddress };
            StatsdClient.Metrics.Configure(metricsConfig);
        }

        [TestFixtureTearDown]
        public void TearDownUdpListener() 
        {
            udpListener.Dispose();
        }

        [SetUp]
        public void StartUdpListenerThread()
        {
            listenThread = new Thread(new ThreadStart(udpListener.Listen));
            listenThread.Start();
        }

        // Test helper. Waits until the listener is done receiving a message,
        // then asserts that the passed string is equal to the message received.
        private void AssertWasReceived(string shouldBe)
        {
                // Stall until the the listener receives a message or times out 
                while(listenThread.IsAlive);
                Assert.AreEqual(shouldBe, udpListener.GetAndClearLastMessage());

        }

        [Test]
        public void _udp_listener_sanity_test()
        {
            var client = new StatsdUDP(localhostAddress, serverPort);
            client.Send("iamnotinsane!");
            AssertWasReceived("iamnotinsane!");

        }

        [Test]
        public void counter()
        {
            Metrics.Counter("counter");
            AssertWasReceived("counter:1|c");
        }
    }
}


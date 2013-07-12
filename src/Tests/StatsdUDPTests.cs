using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;


namespace Tests
{
    // Most of StatsUDP is tested in StatsdUnitTests. This is mainly to test the splitting of oversized
    // UDP packets
    [TestFixture]
    public class StatsUDPTests
    {
        private UdpListener udpListener;
        private Thread listenThread;
		private const int serverPort = 23483;
        private const string serverName = "127.0.0.1";
        private StatsdUDP udp;
        private Statsd statsd;
        private List<string> lastPulledMessages;

        [TestFixtureSetUp]
        public void SetUpUdpListenerAndStatsd() 
        {
            udpListener = new UdpListener(serverName, serverPort);
            var metricsConfig = new MetricsConfig { StatsdServerName = serverName };
            StatsdClient.Metrics.Configure(metricsConfig);
            udp = new StatsdUDP(serverName, serverPort);
            statsd = new Statsd(udp);
        }

        [TestFixtureTearDown]
        public void TearDownUdpListener() 
        {
            udpListener.Dispose();
            udp.Dispose();
        }

        [SetUp]
        public void UdpListenerThread()
        {
            lastPulledMessages = new List<string>();
            listenThread = new Thread(new ParameterizedThreadStart(udpListener.Listen));
        }

        [TearDown]
        public void ClearUdpListenerMessages()
        {
            udpListener.GetAndClearLastMessages(); // just to be sure that nothing is left over
        }

        // Test helper. Waits until the listener is done receiving a message,
        // then asserts that the passed string is equal to the message received.
        private void AssertWasReceived(string shouldBe, int index = 0)
        {
            if (lastPulledMessages.Count == 0)
            {
                // Stall until the the listener receives a message or times out
                while(listenThread.IsAlive);
                lastPulledMessages = udpListener.GetAndClearLastMessages();
            }

            string actual;

            try
            {
                actual = lastPulledMessages[index];
            }
            catch (System.ArgumentOutOfRangeException)
            {
                actual = null;
            }
            Assert.AreEqual(shouldBe, actual);
        }

        [Test]
        public void send()
        {
            listenThread.Start();
            udp.Send("test-metric");
            AssertWasReceived("test-metric");
        }

        [Test]
        public void send_equal_to_udp_packet_limit_is_still_sent()
        {
            var msg = new String('f', StatsdUDP.MAX_UDP_PACKET_SIZE);
            listenThread.Start();
            udp.Send(msg);
            AssertWasReceived(msg);
        }

        [Test]
        public void send_unsplittable_oversized_udp_packets_are_not_split_or_sent_and_no_exception_is_raised()
        {
            var msg = new String('f', StatsdUDP.MAX_UDP_PACKET_SIZE * 100);
            listenThread.Start();
            statsd.Add<Statsd.Counting>(msg, 1);
            statsd.Send();
            AssertWasReceived(null);
        }

        [Test]
        public void send_oversized_udp_packets_are_split_if_possible()
        {
            var msg = new String('f', StatsdUDP.MAX_UDP_PACKET_SIZE - 15);
            listenThread.Start(3); // Listen for 3 messages
            statsd.Add<Statsd.Counting>(msg, 1);
            statsd.Add<Statsd.Gauge>(msg, 2);
            statsd.Send();
            AssertWasReceived(String.Format("{0}:1|c", msg), 0);
            AssertWasReceived(String.Format("{0}:2|g", msg), 1);
            AssertWasReceived(null, 2);
        }
    }
}

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
        private int serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);
        private string serverName = ConfigurationManager.AppSettings["StatsdServerName"];
        private StatsdUDP udp;
        private Statsd statsd;
        private List<string> lastPulledMessages;

        [TestFixtureSetUp]
        public void SetUpUdpListenerAndStatsd()
        {
            udpListener = new UdpListener(serverName, serverPort);
            var metricsConfig = new StatsdConfig { StatsdServerName = serverName };
            StatsdClient.DogStatsd.Configure(metricsConfig);
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
                while (listenThread.IsAlive) ;
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
            // (Sanity test)
            listenThread.Start();
            udp.Send("test-metric");
            AssertWasReceived("test-metric");
        }

        [Test]
        public void send_equal_to_udp_packet_limit_is_still_sent()
        {
            var msg = new String('f', StatsdConfig.DefaultStatsdMaxUDPPacketSize);
            listenThread.Start();
            udp.Send(msg);
            // As long as we're at or below the limit, the packet should still be sent 
            AssertWasReceived(msg);
        }

        [Test]
        public void send_unsplittable_oversized_udp_packets_are_not_split_or_sent_and_no_exception_is_raised()
        {
            // This message will be one byte longer than the theoretical limit of a UDP packet
            var msg = new String('f', 65508);
            listenThread.Start();
            statsd.Add<Statsd.Counting, int>(msg, 1);
            statsd.Send();
            // It shouldn't be split or sent, and no exceptions should be raised.
            AssertWasReceived(null);
        }

        [Test]
        public void send_oversized_udp_packets_are_split_if_possible()
        {
            var msg = new String('f', (StatsdConfig.DefaultStatsdMaxUDPPacketSize - 15));
            listenThread.Start(3); // Listen for 3 messages
            statsd.Add<Statsd.Counting, int>(msg, 1);
            statsd.Add<Statsd.Gauge, int>(msg, 2);
            statsd.Send();
            // These two metrics should be split as their combined lengths exceed the maximum packet size
            AssertWasReceived(String.Format("{0}:1|c", msg), 0);
            AssertWasReceived(String.Format("{0}:2|g", msg), 1);
            // No extra metric should be sent at the end
            AssertWasReceived(null, 2);
        }

        [Test]
        public void send_oversized_udp_packets_are_split_if_possible_with_multiple_messages_in_one_packet()
        {
            var msg = new String('f', StatsdConfig.DefaultStatsdMaxUDPPacketSize / 2);
            listenThread.Start(3); // Listen for 3 messages
            statsd.Add<Statsd.Counting, int>("counter", 1);
            statsd.Add<Statsd.Counting, int>(msg, 2);
            statsd.Add<Statsd.Counting, int>(msg, 3);
            statsd.Send();
            AssertWasReceived(String.Format("counter:1|c\n{0}:2|c", msg), 0);
            AssertWasReceived(String.Format("{0}:3|c", msg), 1);
            AssertWasReceived(null, 2);
        }

        [Test]
        public void set_max_udp_packet_size()
        {
            // Make sure that we can set the max UDP packet size
            udp = new StatsdUDP(serverName, serverPort, 10);
            statsd = new Statsd(udp);
            var msg = new String('f', 5);
            listenThread.Start(2);
            statsd.Add<Statsd.Counting, int>(msg, 1);
            statsd.Add<Statsd.Gauge, int>(msg, 2);
            statsd.Send();
            // Since our packet size limit is now 10, this (short) message should still be split
            AssertWasReceived(String.Format("{0}:1|c", msg), 0);
            AssertWasReceived(String.Format("{0}:2|g", msg), 1);
        }

    }
}

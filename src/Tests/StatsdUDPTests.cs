using System;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;
using StatsdClient.Senders;


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
        private List<string> lastPulledMessages;

        [TestFixtureSetUp]
        public void SetUpUdpListenerAndStatsd()
        {
            udpListener = new UdpListener(serverName, serverPort);
            udp = new StatsdUDP(serverName, serverPort);
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
            var msg = new String('f', MetricsConfig.DefaultStatsdMaxUDPPacketSize);
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

            var statsd = new Statsd(new Statsd.Configuration() { Udp = udp, Sender = new ImmediateSender() });
            statsd.Send<Statsd.Counting>(msg, 1);
            // It shouldn't be split or sent, and no exceptions should be raised.
            AssertWasReceived(null);
        }

        [Test]
        public void send_oversized_udp_packets_are_split_if_possible()
        {
            var msg = new String('f', MetricsConfig.DefaultStatsdMaxUDPPacketSize - 15);
            listenThread.Start(3); // Listen for 3 messages

            var sender = new BatchSender();
            var statsd = new Statsd(new Statsd.Configuration() { Udp = udp, Sender = sender });

            statsd.Send<Statsd.Counting>(msg, 1);
            statsd.Send<Statsd.Timing>(msg, 2);
            sender.Flush();

            // These two metrics should be split as their combined lengths exceed the maximum packet size
            AssertWasReceived(String.Format("{0}:1|c", msg), 0);
            AssertWasReceived(String.Format("{0}:2|ms", msg), 1);
            // No extra metric should be sent at the end
            AssertWasReceived(null, 2);
        }

        [Test]
        public void send_oversized_udp_packets_are_split_if_possible_with_multiple_messages_in_one_packet()
        {
            var msg = new String('f', MetricsConfig.DefaultStatsdMaxUDPPacketSize / 2);
            listenThread.Start(3);

            var sender = new BatchSender();
            var statsd = new Statsd(new Statsd.Configuration() { Udp = udp, Sender = sender });

            statsd.Send<Statsd.Counting>("counter", 1);
            statsd.Send<Statsd.Counting>(msg, 2);
            statsd.Send<Statsd.Counting>(msg, 3);
            sender.Flush();

            // Make sure that a split packet can contain mulitple metrics
            AssertWasReceived(String.Format("counter:1|c\n{0}:2|c", msg), 0);
            AssertWasReceived(String.Format("{0}:3|c", msg), 1);
            AssertWasReceived(null, 2);
        }

        [Test]
        public void set_max_udp_packet_size()
        {
            // Make sure that we can set the max UDP packet size
            udp = new StatsdUDP(serverName, serverPort, 10);
            var msg = new String('f', 5);
            listenThread.Start(2);

            var sender = new BatchSender();
            var statsd = new Statsd(new Statsd.Configuration() { Udp = udp, Sender = sender });
            statsd.Send<Statsd.Counting>(msg, 1);
            statsd.Send<Statsd.Timing>(msg, 2);
            sender.Flush();

            // Since our packet size limit is now 10, this (short) message should still be split
            AssertWasReceived(String.Format("{0}:1|c", msg), 0);
            AssertWasReceived(String.Format("{0}:2|ms", msg), 1);
        }
    }
}

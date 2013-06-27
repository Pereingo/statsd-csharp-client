using System;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
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
        private string serverName = ConfigurationManager.AppSettings["StatsdServerName"];

        [TestFixtureSetUp]
        public void SetUpUdpListener() 
        {
            udpListener = new UdpListener(serverName, serverPort);
            var metricsConfig = new MetricsConfig { StatsdServerName = serverName };
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

        // Test helper. Waits until the listener is done receiving a message,
        // then asserts that the passed regular expression matches the received message.
        private void AssertWasReceivedMatches(string pattern)
        {
            // Stall until the the listener receives a message or times out
            while(listenThread.IsAlive);
            StringAssert.IsMatch(pattern, udpListener.GetAndClearLastMessage());

        }

        [Test]
        public void _udp_listener_sanity_test()
        {
            var client = new StatsdUDP(ConfigurationManager.AppSettings["StatsdServerName"],
                                       Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]));
            client.Send("iamnotinsane!");
            AssertWasReceived("iamnotinsane!");
        }

        [Test]
        public void counter()
        {
            Metrics.Counter("counter");
            AssertWasReceived("counter:1|c");
        }

        [Test]
        public void counter_by_more_than_one()
        {
            Metrics.Counter("counter", 1337);
            AssertWasReceived("counter:1337|c");
        }

        [Test]
        public void increment()
        {
            Metrics.Increment("increment");
            AssertWasReceived("increment:1|c");
        }

        [Test]
        public void decrement()
        {
            Metrics.Decrement("decrement");
            AssertWasReceived("decrement:-1|c");
        }

        [Test]
        public void gauge()
        {
            Metrics.Gauge("gauge", 1337);
            AssertWasReceived("gauge:1337|g");
        }

        [Test]
        public void gauge_double()
        {
            Metrics.Gauge("gauge", 6.3);
            AssertWasReceived("gauge:6.3|g");
        }

        [Test]
        public void gauge_double_rounding()
        {
            Metrics.Gauge("gauge", (double)1/9);
            AssertWasReceived("gauge:0.111111111111111|g");
        }

        [Test]
        public void histogram()
        {
            Metrics.Histogram("histogram", 42);
            AssertWasReceived("histogram:42|h");
        }

        [Test]
        public void histogram_double()
        {
            Metrics.Histogram("histogram", 42.1);
            AssertWasReceived("histogram:42.1|h");
        }

        [Test]
        public void set()
        {
            Metrics.Set("set", 42);
            AssertWasReceived("set:42|s");
        }

        [Test]
        public void timer_passed_int()
        {
            Metrics.Timer("someevent", 999);
            AssertWasReceived("someevent:999|ms");
        }

        [Test]
        public void timer_passed_double()
        {
            Metrics.Timer("someevent", 999.99);
            AssertWasReceived("someevent:999.99|ms");
        }

        [Test]
        public void timer_method()
        {
            Metrics.Time(() => Thread.Sleep(500), "timer");
            // Make sure that the received timer is of the right order of magnitude.
            // The measured value will probably be a few ms longer than the sleep value.
            AssertWasReceivedMatches(@"timer:\d{3}\|ms");
        }

        // [Helper]
        private int pauseAndReturnInt()
        {
            Thread.Sleep(500);
            return 42;
        }

        [Test]
        public void timer_method_sets_return_value()
        {
            var returnValue = Metrics.Time(() => pauseAndReturnInt(), "lifetheuniverseandeverything");
            AssertWasReceivedMatches(@"lifetheuniverseandeverything:\d{3}\|ms");
            Assert.AreEqual(42, returnValue);
        }

        // [Helper]
        private int throwException()
        {
            Thread.Sleep(500);
            throw new Exception("test exception");
        }

        [Test]
        public void timer_method_doesnt_swallow_exception_and_submits_metric()
        {
            Assert.Throws<Exception>(() => Metrics.Time(() => throwException(), "somebadcode"));
            AssertWasReceivedMatches(@"somebadcode:\d{3}\|ms");
        }

        [Test]
        public void timer_block()
        {
            using (Metrics.StartTimer("timer"))
            {
                Thread.Sleep(200);
                Thread.Sleep(300);
            }
            AssertWasReceivedMatches(@"timer:\d{3}\|ms");
        }

        [Test]
        public void timer_block_doesnt_swallow_exception_and_submits_metric()
        {
            // (Wasn't able to get this working with Assert.Throws)
            try
            {
                using (Metrics.StartTimer("timer"))
                {
                    throwException();
                }
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }

            AssertWasReceivedMatches(@"timer:\d{3}\|ms");
        }
    }
}


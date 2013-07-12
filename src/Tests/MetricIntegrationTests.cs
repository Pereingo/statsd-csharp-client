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
            Metrics.Counter("counter", 1337);
            AssertWasReceived("counter:1337|c");
        }


        [Test]
        public void counter_tags()
        {
            Metrics.Counter("counter", 1, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("counter:1|c|#tag1:true,tag2");
        }

        [Test]
        public void counter_sample_rate()
        {
            // A sample rate over 1 doesn't really make sense, but it allows
            // the test to pass every time
            Metrics.Counter("counter", 1, sampleRate: 1.1);
            AssertWasReceived("counter:1|c|@1.1");
        }

        [Test]
        public void counter_sample_rate_tags()
        {
            Metrics.Counter("counter", 1337, sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("counter:1337|c|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void counter_sample_rate_tags_double()
        {
            Metrics.Counter("counter", 1337.3, sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("counter:1337.3|c|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void increment()
        {
            Metrics.Increment("increment");
            AssertWasReceived("increment:1|c");
        }

        [Test]
        public void increment_tags()
        {
            Metrics.Increment("increment", tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("increment:1|c|#tag1:true,tag2");
        }

        [Test]
        public void increment_sample_rate()
        {
            Metrics.Increment("increment", sampleRate: 1.1);
            AssertWasReceived("increment:1|c|@1.1");
        }

        [Test]
        public void increment_sample_rate_tags()
        {
            Metrics.Increment("increment", sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("increment:1|c|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void decrement()
        {
            Metrics.Decrement("decrement");
            AssertWasReceived("decrement:-1|c");
        }

        [Test]
        public void decrement_tags()
        {
            Metrics.Decrement("decrement", tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("decrement:-1|c|#tag1:true,tag2");
        }

        [Test]
        public void decrement_sample_rate()
        {
            Metrics.Decrement("decrement", sampleRate: 1.1);
            AssertWasReceived("decrement:-1|c|@1.1");
        }

        [Test]
        public void decrement_sample_rate_tags()
        {
            Metrics.Decrement("decrement", sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("decrement:-1|c|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void gauge()
        {
            Metrics.Gauge("gauge", 1337);
            AssertWasReceived("gauge:1337|g");
        }

        [Test]
        public void gauge_tags()
        {
            Metrics.Gauge("gauge", 1337, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("gauge:1337|g|#tag1:true,tag2");
        }

        [Test]
        public void gauge_sample_rate()
        {
            Metrics.Gauge("gauge", 1337, sampleRate: 1.1);
            AssertWasReceived("gauge:1337|g|@1.1");
        }

        [Test]
        public void gauge_sample_rate_tags()
        {
            Metrics.Gauge("gauge", 1337, sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("gauge:1337|g|@1.1|#tag1:true,tag2");
        }

        [Test]
        public void gauge_double()
        {
            Metrics.Gauge("gauge", 6.3);
            AssertWasReceived("gauge:6.3|g");
        }

        [Test]
        public void gauge_double_tags()
        {
            Metrics.Gauge("gauge", 3.1337, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("gauge:3.1337|g|#tag1:true,tag2");
        }

        [Test]
        public void gauge_double_sample_rate()
        {
            Metrics.Gauge("gauge", 3.1337, sampleRate: 1.1);
            AssertWasReceived("gauge:3.1337|g|@1.1");
        }

        [Test]
        public void gauge_double_sample_rate_tags()
        {
            Metrics.Gauge("gauge", 3.1337, sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("gauge:3.1337|g|@1.1|#tag1:true,tag2");
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
        public void histogram_tags()
        {
            Metrics.Histogram("histogram", 42, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("histogram:42|h|#tag1:true,tag2");
        }

        [Test]
        public void histogram_sample_rate()
        {
            Metrics.Histogram("histogram", 42, sampleRate: 1.1);
            AssertWasReceived("histogram:42|h|@1.1");
        }

        [Test]
        public void histogram_sample_rate_tags()
        {
            Metrics.Histogram("histogram", 42, sampleRate: 1.1, tags: new[] {"tag1:true,tag2"});
            AssertWasReceived("histogram:42|h|@1.1|#tag1:true,tag2");
        }

        [Test]
        public void histogram_double()
        {
            Metrics.Histogram("histogram", 42.1);
            AssertWasReceived("histogram:42.1|h");
        }

        [Test]
        public void histogram_double_tags()
        {
            Metrics.Histogram("histogram", 42.1, tags: new[] {"tag1:true,tag2"});
            AssertWasReceived("histogram:42.1|h|#tag1:true,tag2");
        }

        [Test]
        public void histogram_double_sample_rate()
        {
            Metrics.Histogram("histogram", 42.1, 1.1);
            AssertWasReceived("histogram:42.1|h|@1.1");
        }

        [Test]
        public void histogram_double_sample_rate_tags()
        {
            Metrics.Histogram("histogram", 42.1, sampleRate: 1.1, tags: new[] {"tag1:true,tag2"});
            AssertWasReceived("histogram:42.1|h|@1.1|#tag1:true,tag2");
        }

        [Test]
        public void set()
        {
            Metrics.Set("set", 42);
            AssertWasReceived("set:42|s");
        }

        [Test]
        public void set_tags()
        {
            Metrics.Set("set", 42, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:42|s|#tag1:true,tag2");
        }

        [Test]
        public void set_sample_rate()
        {
            Metrics.Set("set", 42, sampleRate: 1.1);
            AssertWasReceived("set:42|s|@1.1");
        }

        [Test]
        public void set_sample_rate_tags()
        {
            Metrics.Set("set", 42, sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:42|s|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void set_double()
        {
            Metrics.Set("set", 42.2);
            AssertWasReceived("set:42.2|s");
        }

        [Test]
        public void set_double_tags()
        {
            Metrics.Set("set", 42.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:42.2|s|#tag1:true,tag2");
        }

        [Test]
        public void set_double_sample_rate()
        {
            Metrics.Set("set", 42.2, sampleRate: 1.1);
            AssertWasReceived("set:42.2|s|@1.1");
        }

        [Test]
        public void set_double_sample_rate_tags()
        {
            Metrics.Set("set", 42.2, sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:42.2|s|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void set_string()
        {
            Metrics.Set("set", "string");
            AssertWasReceived("set:string|s");
        }

        [Test]
        public void set_string_tags()
        {
            Metrics.Set("set", "string", tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:string|s|#tag1:true,tag2");
        }

        [Test]
        public void set_string_sample_rate()
        {
            Metrics.Set("set", "string", sampleRate: 1.1);
            AssertWasReceived("set:string|s|@1.1");
        }

        [Test]
        public void set_string_sample_rate_tags()
        {
            Metrics.Set("set", "string", sampleRate: 12.2, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("set:string|s|@12.2|#tag1:true,tag2");
        }

        [Test]
        public void timer()
        {
            Metrics.Timer("someevent", 999);
            AssertWasReceived("someevent:999|ms");
        }

        [Test]
        public void timer_tags()
        {
            Metrics.Timer("someevent", 999, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("someevent:999|ms|#tag1:true,tag2");
        }

        [Test]
        public void timer_sample_rate()
        {
            Metrics.Timer("someevent", 999, sampleRate: 1.1);
            AssertWasReceived("someevent:999|ms|@1.1");
        }

        [Test]
        public void timer_sample_rate_tags()
        {
            Metrics.Timer("someevent", 999, sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("someevent:999|ms|@1.1|#tag1:true,tag2");
        }

        [Test]
        public void timer_double()
        {
            Metrics.Timer("someevent", 999.99);
            AssertWasReceived("someevent:999.99|ms");
        }

        [Test]
        public void timer_double_tags()
        {
            Metrics.Timer("someevent", 999.99, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("someevent:999.99|ms|#tag1:true,tag2");
        }

        [Test]
        public void timer_double_sample_rate()
        {
            Metrics.Timer("someevent", 999.99, sampleRate: 1.1);
            AssertWasReceived("someevent:999.99|ms|@1.1");
        }

        [Test]
        public void timer_double_sample_rate_tags()
        {
            Metrics.Timer("someevent", 999.99, sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"});
            AssertWasReceived("someevent:999.99|ms|@1.1|#tag1:true,tag2");
        }

        [Test]
        public void timer_method()
        {
            Metrics.Time(() => Thread.Sleep(500), "timer");
            // Make sure that the received timer is of the right order of magnitude.
            // The measured value will probably be a few ms longer than the sleep value.
            AssertWasReceivedMatches(@"timer:\d{3}\|ms");
        }

        [Test]
        public void timer_method_tags()
        {
            Metrics.Time(() => Thread.Sleep(500), "timer", tags: new[] {"tag1:true", "tag2"});
            // Make sure that the received timer is of the right order of magnitude.
            // The measured value will probably be a few ms longer than the sleep value.
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|#tag1:true,tag2");
        }

        [Test]
        public void timer_method_sample_rate()
        {
            Metrics.Time(() => Thread.Sleep(500), "timer", sampleRate: 1.1);
            // Make sure that the received timer is of the right order of magnitude.
            // The measured value will probably be a few ms longer than the sleep value.
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|@1\.1");
        }

        [Test]
        public void timer_method_sample_rate_tags()
        {
            Metrics.Time(() => Thread.Sleep(500), "timer", sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"});
            // Make sure that the received timer is of the right order of magnitude.
            // The measured value will probably be a few ms longer than the sleep value.
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|@1\.1\|#tag1:true,tag2");
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

        [Test]
        public void timer_method_sets_return_value_tags()
        {
            var returnValue = Metrics.Time(() => pauseAndReturnInt(), "lifetheuniverseandeverything", tags: new[] {"towel:present"});
            AssertWasReceivedMatches(@"lifetheuniverseandeverything:\d{3}\|ms\|#towel:present");
            Assert.AreEqual(42, returnValue);
        }

        [Test]
        public void timer_method_sets_return_value_sample_rate()
        {
            var returnValue = Metrics.Time(() => pauseAndReturnInt(), "lifetheuniverseandeverything", sampleRate: 4.2);
            AssertWasReceivedMatches(@"lifetheuniverseandeverything:\d{3}\|ms\|@4\.2");
            Assert.AreEqual(42, returnValue);
        }

        [Test]
        public void timer_method_sets_return_value_sample_rate_and_tag()
        {
            var returnValue = Metrics.Time(() => pauseAndReturnInt(), "lifetheuniverseandeverything", sampleRate: 4.2, tags: new[] {"fjords"});
            AssertWasReceivedMatches(@"lifetheuniverseandeverything:\d{3}\|ms\|@4\.2\|#fjords");
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
        public void timer_block_tags()
        {
            using (Metrics.StartTimer("timer", tags: new[] {"tag1:true", "tag2"}))
            {
                Thread.Sleep(200);
                Thread.Sleep(300);
            }
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|#tag1:true,tag2");
        }

        [Test]
        public void timer_block_sampleRate()
        {
            using (Metrics.StartTimer("timer", sampleRate: 1.1))
            {
                Thread.Sleep(200);
                Thread.Sleep(300);
            }
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|@1\.1");
        }

        [Test]
        public void timer_block_sampleRate_and_tag()
        {
            using (Metrics.StartTimer("timer", sampleRate: 1.1, tags: new[] {"tag1:true", "tag2"}))
            {
                Thread.Sleep(200);
                Thread.Sleep(300);
            }
            AssertWasReceivedMatches(@"timer:\d{3}\|ms\|@1\.1\|#tag1:true,tag2");
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


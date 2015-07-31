using System.Threading;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;
using StatsdClient.Senders;

namespace Tests
{
    [TestFixture]
    public class MetricIntegrationTests
    {
        private UdpListener _udpListener;
        private Thread _listenThread;
        private const int _randomUnusedLocalPort = 23483;
        private const string _localhostAddress = "127.0.0.1";
        private MetricsConfig _defaultMetricsConfig;

        const string _expectedTestPrefixRegex = @"test_prefix\.";
        const string _expectedTimeRegEx = @"time:.\\|ms";

        [TestFixtureSetUp]
        public void SetUpUdpListener()
        {
            _udpListener = new UdpListener(_localhostAddress, _randomUnusedLocalPort);
        }

        [TestFixtureTearDown]
        public void TearDownUdpListener() 
        {
            _udpListener.Dispose();
        }

        [SetUp]
        public void StartUdpListenerThread()
        {
            _defaultMetricsConfig = new MetricsConfig
            {
                StatsdServerName = _localhostAddress,
                StatsdServerPort = _randomUnusedLocalPort,
                Sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 100 })
            };

            _listenThread = new Thread(_udpListener.Listen);
            _listenThread.Start();
        }

        private string LastPacketMessageReceived()
        {
            // Stall until the the listener receives a message or times out.
            while(_listenThread.IsAlive) {}

            var lastMessages = _udpListener.GetAndClearLastMessages();
            try
            {
                return lastMessages[0];
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        [Test]
        public void udp_listener_sanity_test()
        {
            var client = new StatsdUDP(_localhostAddress, _randomUnusedLocalPort);
            client.Send("iamnotinsane!");

            Assert.That(LastPacketMessageReceived(), Is.EqualTo("iamnotinsane!"));
        }

        public class Counter : MetricIntegrationTests
        {
            [Test]
            public void counter()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Counter("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:1|c"));
            }

            [Test]
            public void counter_with_value()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Counter("counter", 10);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:10|c"));
            }

            [Test]
            public void counter_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Counter("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.counter:1|c"));
            }

            [Test]
            public void counter_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Counter("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.counter:1|c"));
            }

            [Test]
            public void counter_with_value_and_sampleRate()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Counter("counter", 10, 0.9999);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:10|c|@0.9999"));
            }

            [Test]
            public void counter_with_no_config_setup_should_not_send_metric()
            {
                Metrics.Configure(new MetricsConfig());

                Metrics.Counter("counter");
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class Timer : MetricIntegrationTests
        {
            [Test]
            public void timer()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Timer("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("timer:6|ms"));
            }

            [Test]
            public void timer_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Timer("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:6|ms"));
            }

            [Test]
            public void timer_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Timer("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:6|ms"));
            }

            [Test]
            public void timer_with_no_config_setup_should_not_send_metric()
            {
                Metrics.Configure(new MetricsConfig());

                Metrics.Timer("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class Time : MetricIntegrationTests
        {
            [Test]
            public void time()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Time(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTimeRegEx));
            }

            [Test]
            public void time_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Time(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
            }

            [Test]
            public void time_with_prefix_having_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Time(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
            }

            [Test]
            public void time_with_no_config_setup_should_not_send_metric_but_still_run_action()
            {
                Metrics.Configure(new MetricsConfig());

                var someValue = 5;
                Metrics.Time(() => { someValue = 10; }, "timer");

                Assert.That(someValue, Is.EqualTo(10));
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }

            [Test]
            public void time_with_return_value()
            {
                Metrics.Configure(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public void time_with_return_value_and_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public void time_with_return_value_and_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public void time_with_return_value_and_no_config_setup_should_not_send_metric_but_still_return_value()
            {
                Metrics.Configure(new MetricsConfig());

                var returnValue = Metrics.Time(() => 5, "time");

                Assert.That(LastPacketMessageReceived(), Is.Null);
                Assert.That(returnValue, Is.EqualTo(5));
            }
        }

        public class Gauge : MetricIntegrationTests
        {
            [Test]
            public void gauge_with_double_value()
            {
                Metrics.Configure(_defaultMetricsConfig);

                const double value = 12345678901234567890;
                Metrics.Gauge("gauge", value);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("gauge:12345678901234600000.000000000000000|g"));
            }

            [Test]
            public void gauge_with_double_value_with_floating_point()
            {
                Metrics.Configure(_defaultMetricsConfig);

                const double value = 1.234567890123456;
                Metrics.Gauge("gauge", value);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("gauge:1.234567890123460|g"));
            }

            [Test]
            public void gauge_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Gauge("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.gauge:3.000000000000000|g"));
            }

            [Test]
            public void gauge_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Gauge("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.gauge:3.000000000000000|g"));
            }

            [Test]
            public void gauge_with_no_config_setup_should_not_send_metric()
            {
                Metrics.Configure(new MetricsConfig());

                Metrics.Gauge("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class Set : MetricIntegrationTests
        {
            [Test]
            public void set()
            {
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Set("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("timer:value|s"));
            }

            [Test]
            public void set_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Set("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:value|s"));
            }

            [Test]
            public void set_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                Metrics.Configure(_defaultMetricsConfig);

                Metrics.Set("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:value|s"));
            }

            [Test]
            public void set_with_no_config_setup_should_not_send_metric()
            {
                Metrics.Configure(new MetricsConfig());

                Metrics.Set("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }
    }
}
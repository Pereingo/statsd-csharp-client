using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;

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
                StatsdServerPort = _randomUnusedLocalPort
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
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public class SanityCheck : MetricIntegrationTests
        {
            [Test]
            public async Task udp_listener_works()
            {
                var client = new StatsdUDP(_localhostAddress, _randomUnusedLocalPort);
                await client.InitializeAsync();
                await client.SendAsync("iamnotinsane!");

                Assert.That(LastPacketMessageReceived(), Is.EqualTo("iamnotinsane!"));
            }
        }

        public class Counter : MetricIntegrationTests
        {
            [Test]
            public async Task counter()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.CounterAsync("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:1|c"));
            }

            [Test]
            public async Task counter_with_value()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.CounterAsync("counter", 10);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:10|c"));
            }

            [Test]
            public async Task counter_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.CounterAsync("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.counter:1|c"));
            }

            [Test]
            public async Task counter_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.CounterAsync("counter");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.counter:1|c"));
            }

            [Test]
            public async Task counter_with_value_and_sampleRate()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.CounterAsync("counter", 10, 0.9999);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("counter:10|c|@0.9999"));
            }

            [Test]
            public async Task counter_with_no_config_setup_should_not_send_metric()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                await Metrics.CounterAsync("counter");
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class Timer : MetricIntegrationTests
        {
            [Test]
            public async Task timer()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimerAsync("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("timer:6|ms"));
            }

            [Test]
            public async Task timer_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimerAsync("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:6|ms"));
            }

            [Test]
            public async Task timer_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimerAsync("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:6|ms"));
            }

            [Test]
            public async Task timer_with_no_config_setup_should_not_send_metric()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                await Metrics.TimerAsync("timer", 6);
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class DisposableTimer : MetricIntegrationTests
        {
            [Test]
            public async Task disposable_timer()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                using (Metrics.StartTimer("time"))
                {
                    Thread.Sleep(2);
                }

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTimeRegEx));
            }
        }

        public class Time : MetricIntegrationTests
        {
            [Test]
            public async Task time()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimeAsync(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTimeRegEx));
            }

            [Test]
            public async Task time_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimeAsync(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
            }

            [Test]
            public async Task time_with_prefix_having_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.TimeAsync(() => Thread.Sleep(2), "time");
                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
            }

            [Test]
            public async Task time_with_no_config_setup_should_not_send_metric_but_still_run_action()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                var someValue = 5;
                await Metrics.TimeAsync(() => { someValue = 10; }, "timer");

                Assert.That(someValue, Is.EqualTo(10));
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }

            [Test]
            public async Task time_with_return_value()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public async Task time_with_return_value_and_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public async Task time_with_return_value_and_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                var returnValue = Metrics.Time(() =>
                {
                    Thread.Sleep(2);
                    return 5;
                }, "time");

                Assert.That(LastPacketMessageReceived(), Is.StringMatching(_expectedTestPrefixRegex + _expectedTimeRegEx));
                Assert.That(returnValue, Is.EqualTo(5));
            }

            [Test]
            public async Task time_with_return_value_and_no_config_setup_should_not_send_metric_but_still_return_value()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                var returnValue = Metrics.Time(() => 5, "time");

                Assert.That(LastPacketMessageReceived(), Is.Null);
                Assert.That(returnValue, Is.EqualTo(5));
            }
        }

        public class GaugeDelta : MetricIntegrationTests
        {
            [Test]
            [TestCase(123d, "gauge:+123|g")]
            [TestCase(-123d, "gauge:-123|g")]
            [TestCase(0d, "gauge:+0|g")]
            public async Task GaugeDelta_EmitsCorrect_Format(double gaugeDeltaValue, string expectedPacketMessageFormat)
            {
              await Metrics.ConfigureAsync(_defaultMetricsConfig);

              await Metrics.GaugeDeltaAsync("gauge", gaugeDeltaValue);
              Assert.That(LastPacketMessageReceived(), Is.EqualTo(expectedPacketMessageFormat));
            }
        }

        public class GaugeObsolete : MetricIntegrationTests
        {
            [Test]
            public async Task obsolete_gauge_with_double_value()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                const double value = 12345678901234567890;
                await Metrics.GaugeAsync("gauge", value);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("gauge:12345678901234600000.000000000000000|g"));
            }
        }

        public class GaugeAbsolute : MetricIntegrationTests
        {
            [Test]
            public async Task absolute_gauge_with_double_value()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                const double value = 12345678901234567890;
                await Metrics.GaugeAbsoluteValueAsync("gauge", value);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("gauge:12345678901234600000.000000000000000|g"));
            }

            [Test]
            public async Task absolute_gauge_with_double_value_with_floating_point()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                const double value = 1.234567890123456;
                await Metrics.GaugeAbsoluteValueAsync("gauge", value);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("gauge:1.234567890123460|g"));
            }

            [Test]
            public async Task absolute_gauge_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.GaugeAbsoluteValueAsync("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.gauge:3.000000000000000|g"));
            }

            [Test]
            public async Task absolute_gauge_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.GaugeAbsoluteValueAsync("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.gauge:3.000000000000000|g"));
            }

            [Test]
            public async Task gauge_with_no_config_setup_should_not_send_metric()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                await Metrics.GaugeAbsoluteValueAsync("gauge", 3);
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }

        public class Set : MetricIntegrationTests
        {
            [Test]
            public async Task set()
            {
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.SetAsync("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("timer:value|s"));
            }

            [Test]
            public async Task set_with_prefix()
            {
                _defaultMetricsConfig.Prefix = "test_prefix";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.SetAsync("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:value|s"));
            }

            [Test]
            public async Task set_with_prefix_having_a_trailing_dot()
            {
                _defaultMetricsConfig.Prefix = "test_prefix.";
                await Metrics.ConfigureAsync(_defaultMetricsConfig);

                await Metrics.SetAsync("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.timer:value|s"));
            }

            [Test]
            public async Task set_with_no_config_setup_should_not_send_metric()
            {
                await Metrics.ConfigureAsync(new MetricsConfig());

                await Metrics.SetAsync("timer", "value");
                Assert.That(LastPacketMessageReceived(), Is.Null);
            }
        }
    }
}
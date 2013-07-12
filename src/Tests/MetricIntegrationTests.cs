using System.Threading;
using System.Collections.Generic;
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

            _listenThread = new Thread(new ParameterizedThreadStart(_udpListener.Listen));
            _listenThread.Start();
        }

		private string LastPacketMessageReceived()
		{
			// Stall until the the listener receives a message or times out.
			while(_listenThread.IsAlive) {}

			List<string> _lastMessages = _udpListener.GetAndClearLastMessages();
			try
			{
				return _lastMessages[0];
			}
			catch (System.ArgumentOutOfRangeException)
			{
				return null;
			}
		}

        [Test]
        public void _udp_listener_sanity_test()
        {
            var client = new StatsdUDP(_localhostAddress, _randomUnusedLocalPort);
            client.Send("iamnotinsane!");

	        Assert.That(LastPacketMessageReceived(), Is.EqualTo("iamnotinsane!"));
        }

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
		public void counter_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Counter("counter");
			Assert.That(LastPacketMessageReceived(), Is.Null);
		}

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
		public void timer_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Timer("timer", 6);
			Assert.That(LastPacketMessageReceived(), Is.Null);
		}

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
		public void time_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Time(() => {}, "timer");
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
		public void time_with_return_value_and_no_config_setup_should_not_send_metric_but_still_return_value()
		{
			Metrics.Configure(new MetricsConfig());

			var returnValue = Metrics.Time(() => 5, "time");

			Assert.That(LastPacketMessageReceived(), Is.Null);
			Assert.That(returnValue, Is.EqualTo(5));
		}

	    [Test]
		public void guage()
		{
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Gauge("guage", 3);
		    Assert.That(LastPacketMessageReceived(), Is.EqualTo("guage:3|g"));
		}

		[Test]
		public void guage_with_prefix()
		{
			_defaultMetricsConfig.Prefix = "test_prefix";
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Gauge("guage", 3);
			Assert.That(LastPacketMessageReceived(), Is.EqualTo("test_prefix.guage:3|g"));
		}

		[Test]
		public void guage_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Gauge("guage", 3);
			Assert.That(LastPacketMessageReceived(), Is.Null);
		}
    }
}

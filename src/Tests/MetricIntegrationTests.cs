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
        private int randomUnusedLocalPort = 23483;
        private string localhostAddress = "127.0.0.1";
	    private MetricsConfig _defaultMetricsConfig;

		const string expectedTestPrefixRegex = @"test_prefix\.";
		const string expectedTimeRegEx = @"time:.\\|ms";

	    [TestFixtureSetUp]
        public void SetUpUdpListener() 
        {
            udpListener = new UdpListener(localhostAddress, randomUnusedLocalPort);
        }

        [TestFixtureTearDown]
        public void TearDownUdpListener() 
        {
            udpListener.Dispose();
        }

        [SetUp]
        public void StartUdpListenerThread()
		{
			_defaultMetricsConfig = new MetricsConfig
			{
				StatsdServerName = localhostAddress,
				StatsdServerPort = randomUnusedLocalPort
			};

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
		// then asserts that the passed string is equal to the message received.
		private void AssertWasReceivedUsingRegEx(string shouldBe)
		{
			// Stall until the the listener receives a message or times out 
			while (listenThread.IsAlive) ;
			Assert.That(udpListener.GetAndClearLastMessage(), Is.StringMatching(shouldBe));

		}

		// Test helper. Waits until the listener is done receiving a message,
		// then asserts that the passed string is equal to the message received.
		private void AssertNothingWasReceived()
		{
			// Stall until the the listener receives a message or times out 
			while (listenThread.IsAlive) ;
			Assert.That(udpListener.GetAndClearLastMessage(), Is.Null);

		}

        [Test]
        public void _udp_listener_sanity_test()
        {
            var client = new StatsdUDP(localhostAddress, randomUnusedLocalPort);
            client.Send("iamnotinsane!");
            AssertWasReceived("iamnotinsane!");

        }

        [Test]
        public void counter()
        {
			Metrics.Configure(_defaultMetricsConfig);

            Metrics.Counter("counter");
            AssertWasReceived("counter:1|c");
        }

		[Test]
		public void counter_with_value()
		{
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Counter("counter", 10);
			AssertWasReceived("counter:10|c");
		}

		[Test]
		public void counter_with_prefix()
		{
			_defaultMetricsConfig.Prefix = "test_prefix";
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Counter("counter");
			AssertWasReceived("test_prefix.counter:1|c");
		}

		[Test]
		public void counter_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Counter("counter");
			AssertNothingWasReceived();
		}

		[Test]
		public void timer()
		{
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Timer("timer", 6);
			AssertWasReceived("timer:6|ms");
		}

		[Test]
		public void timer_with_prefix()
		{
			_defaultMetricsConfig.Prefix = "test_prefix";
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Timer("timer", 6);
			AssertWasReceived("test_prefix.timer:6|ms");
		}

		[Test]
		public void timer_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Timer("timer", 6);
			AssertNothingWasReceived();
		}

		[Test]
		public void time()
		{
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Time(() => Thread.Sleep(2), "time");
			AssertWasReceivedUsingRegEx(expectedTimeRegEx);
		}

	    [Test]
		public void time_with_prefix()
		{
			_defaultMetricsConfig.Prefix = "test_prefix";
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Time(() => Thread.Sleep(2), "time");
			AssertWasReceivedUsingRegEx(expectedTestPrefixRegex + expectedTimeRegEx);
		}

		[Test]
		public void time_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Time(() => {}, "timer");
			AssertNothingWasReceived();
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

			AssertWasReceivedUsingRegEx(expectedTimeRegEx);
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

			AssertWasReceivedUsingRegEx(expectedTestPrefixRegex + expectedTimeRegEx);
			Assert.That(returnValue, Is.EqualTo(5));
		}

		[Test]
		public void time_with_return_value_and_no_config_setup_should_not_send_metric_but_still_return_value()
		{
			Metrics.Configure(new MetricsConfig());

			var returnValue = Metrics.Time(() => 5, "time");

			AssertNothingWasReceived();
			Assert.That(returnValue, Is.EqualTo(5));
		}

	    [Test]
		public void guage()
		{
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Gauge("guage", 3);
			AssertWasReceived("guage:3|g");
		}

		[Test]
		public void guage_with_prefix()
		{
			_defaultMetricsConfig.Prefix = "test_prefix";
			Metrics.Configure(_defaultMetricsConfig);

			Metrics.Gauge("guage", 3);
			AssertWasReceived("test_prefix.guage:3|g");
		}

		[Test]
		public void guage_with_no_config_setup_should_not_send_metric()
		{
			Metrics.Configure(new MetricsConfig());

			Metrics.Gauge("guage", 3);
			AssertNothingWasReceived();
		}
    }
}


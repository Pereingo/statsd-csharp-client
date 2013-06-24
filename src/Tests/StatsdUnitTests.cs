using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class UnitTests
    {
        private IStatsdUDP udp;
        private IRandomGenerator _randomGenerator;
        private IStopWatchFactory _stopwatch;

        [SetUp]
        public void Setup()
        {
            udp = MockRepository.GenerateMock<IStatsdUDP>();
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);
            _stopwatch = MockRepository.GenerateMock<IStopWatchFactory>();
        }


		// =-=-=-=- COUNTER -=-=-=-=

        [Test]
        public void send_increase_counter_by_x()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Counting>("counter", 5);
            udp.AssertWasCalled(x => x.Send("counter:5|c"));
        }

        [Test]
        public void send_increase_counter_by_x_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Counting>("counter", 5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("counter:5|c|#tag1:true"));
        }

        [Test]
        public void send_increase_counter_by_x_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Counting>("counter", 5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send("counter:5|c|#tag1:true,tag2"));
        }

		[Test]
        public void send_increase_counter_by_x_and_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send("counter", 5,0.1);
            udp.AssertWasCalled(x => x.Send("counter:5|c|@0.1"));
        }

        [Test]
        public void send_increase_counter_by_x_and_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send("counter", 5, 0.1, "tag1:true");
            udp.AssertWasCalled(x => x.Send("counter:5|c|@0.1|#tag1:true"));
        }

        [Test]
        public void send_increase_counter_by_x_and_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send("counter", 5, 0.1, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send("counter:5|c|@0.1|#tag1:true,tag2"));
        }

		[Test]
		public void send_increase_counter_counting_exception_fails_silently()
		{
			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
			udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
			s.Send<Statsd.Counting>("counter", 5);
			Assert.Pass();
		}

        [Test]
        public void add_increase_counter_by_x()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c"));
        }

        [Test]
        public void add_increase_counter_by_x_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5, 0.5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c|@0.5"));
        }

        [Test]
        public void add_increase_counter_by_x_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5, 0.5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c|@0.5|#tag1:true"));
        }

        [Test]
        public void add_increase_counter_by_x_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5, 0.5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void add_increase_counter_by_x_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c|#tag1:true"));
        }

        [Test]
        public void add_increase_counter_by_x_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("counter:5|c|#tag1:true,tag2"));
        }

        // TODO: TEST COUNTER ADD

		// =-=-=-=- TIMER -=-=-=-=

        [Test]
        public void send_timer()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5);
            udp.AssertWasCalled(x => x.Send("timer:5|ms"));
        }

        [Test]
        public void send_timer_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, 0.5);
            udp.AssertWasCalled(x => x.Send("timer:5|ms|@0.5"));
        }

        [Test]
        public void send_timer_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, 0.5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("timer:5|ms|@0.5|#tag1:true"));
        }

        [Test]
        public void send_timer_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, 0.5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send("timer:5|ms|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void send_timer_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("timer:5|ms|#tag1:true"));
        }

        [Test]
        public void send_timer_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("timer:5|ms|#tag1:true"));
        }

		[Test]
		public void send_timer_exception_fails_silently()
		{
			udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
			Statsd s = new Statsd(udp);
			s.Send<Statsd.Timing>("timer", 5);
			Assert.Pass();
		}

		[Test]
		public void send_timer_with_lambda()
		{
			const string statName = "name";
			IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
			stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
			_stopwatch.Stub(x => x.Get()).Return(stopwatch);

			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
			s.Send(() => testMethod(), statName);

			udp.AssertWasCalled(x => x.Send("name:500|ms"));       
		}

        [Test]
        public void send_timer_with_lambda_and_tag()
        {
            const string statName = "name";
            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send(() => testMethod(), statName, "tag1:true");

            udp.AssertWasCalled(x => x.Send("name:500|ms|#tag1:true"));       
        }

        [Test]
        public void send_timer_with_lambda_and_tags()
        {
            const string statName = "name";
            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send(() => testMethod(), statName, "tag1:true", "tag2");

            udp.AssertWasCalled(x => x.Send("name:500|ms|#tag1:true,tag2"));       
        }

		[Test]
		public void send_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
		{
			const string statName = "name";
			var stopwatch = MockRepository.GenerateMock<IStopwatch>();
			stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
			_stopwatch.Stub(x => x.Get()).Return(stopwatch);

			var s = new Statsd(udp, _randomGenerator, _stopwatch);
			Assert.Throws<InvalidOperationException>(() => s.Send(() => { throw new InvalidOperationException(); }, statName));

			udp.AssertWasCalled(x => x.Send("name:500|ms"));
		}

		[Test]
		public void send_timer_with_lambda_set_return_value_with()
		{
			const string statName = "name";
			IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
			stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
			_stopwatch.Stub(x => x.Get()).Return(stopwatch);

			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
			int returnValue = 0;
			s.Send(() => returnValue = testMethod(), statName);

			udp.AssertWasCalled(x => x.Send("name:500|ms"));
			Assert.That(returnValue,Is.EqualTo(5));
		}

        [Test]
        public void add_timer_with_lamba()
        {
            const string statName = "name";

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
        }

        [Test]
        public void add_timer_with_lamba_and_tag()
        {
            const string statName = "name";

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("name:500|ms|#tag1:true"));
        }

        [Test]
        public void add_timer_with_lamba_and_tags()
        {
            const string statName = "name";

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("name:500|ms|#tag1:true,tag2"));
        }

        [Test]
        public void add_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
        {
            const string statName = "name";

            var stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            var s = new Statsd(udp, _randomGenerator, _stopwatch);

            Assert.Throws<InvalidOperationException>(() => s.Add(() => { throw new InvalidOperationException(); }, statName));

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
        }

		// =-=-=-=- GAUGE -=-=-=-=
		
        [Test]
        public void send_gauge()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5);
            udp.AssertWasCalled(x => x.Send("gauge:5|g"));
        }

        [Test]
        public void send_gauge_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5, 0.5);
            udp.AssertWasCalled(x => x.Send("gauge:5|g|@0.5"));
        }

        [Test]
        public void send_gauge_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5, 0.5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("gauge:5|g|@0.5|#tag1:true"));
        }

        [Test]
        public void send_gauge_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5, 0.5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send("gauge:5|g|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void send_gauge_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5, "tag1:true");
            udp.AssertWasCalled(x => x.Send("gauge:5|g|#tag1:true"));
        }

        [Test]
        public void send_gauge_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send("gauge:5|g|#tag1:true,tag2"));
        }

        [Test]
        public void send_gauge_exception_fails_silently()
        {
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Gauge>("gauge", 5);
            Assert.Pass();
        }

        [Test]
        public void add_gauge()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g"));
        }

        [Test]
        public void add_gauge_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5, 0.5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g|@0.5"));
        }

        [Test]
        public void add_gauge_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5, 0.5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g|@0.5|#tag1:true"));
        }

        [Test]
        public void add_gauge_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5, 0.5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void add_gauge_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g|#tag1:true"));
        }

        [Test]
        public void add_gauge_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Gauge>("gauge", 5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("gauge:5|g|#tag1:true,tag2"));
        }
		
		// =-=-=-=- COMBINATION -=-=-=-=

        [Test]
        public void add_one_counter_and_one_gauge_shows_in_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);

            Assert.That(s.Commands.Count, Is.EqualTo(2));
            Assert.That(s.Commands[0], Is.EqualTo("counter:1|c|@0.1"));
            Assert.That(s.Commands[1], Is.EqualTo("timer:1|ms"));
        }

        [Test]
        public void add_one_counter_and_one_gauge_with_no_sample_rate_shows_in_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1);
            s.Add<Statsd.Timing>("timer", 1);

            Assert.That(s.Commands.Count, Is.EqualTo(2));
            Assert.That(s.Commands[0], Is.EqualTo("counter:1|c"));
            Assert.That(s.Commands[1], Is.EqualTo("timer:1|ms"));
        }


        [Test]
        public void add_one_counter_and_one_gauge_sends_in_one_go()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            udp.AssertWasCalled(x => x.Send("counter:1|c|@0.1" + Environment.NewLine + "timer:1|ms"));
        }


        [Test]
        public void add_one_counter_and_one_gauge_sends_and_removes_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            Assert.That(s.Commands.Count, Is.EqualTo(0));
        }

        [Test]
        public void add_one_counter_and_send_one_gauge_sends_only_sends_the_last()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1);
            s.Send<Statsd.Timing>("timer", 1);

            udp.AssertWasCalled(x => x.Send("timer:1|ms"));
        }
		
		// =-=-=-=- PREFIX -=-=-=-=

        [Test]
        public void set_prefix_on_stats_name_when_calling_send()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch, "a.prefix.");
            s.Send<Statsd.Counting>("counter", 5);
            s.Send<Statsd.Counting>("counter", 5);

            udp.AssertWasCalled(x => x.Send("a.prefix.counter:5|c"), x => x.Repeat.Twice());
        }

        [Test]
        public void add_counter_sets_prefix_on_name()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch, "another.prefix.");

            s.Add("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            udp.AssertWasCalled(x => x.Send("another.prefix.counter:1|c|@0.1" + Environment.NewLine + "another.prefix.timer:1|ms"));
        }

        private int testMethod()
        {
            return 5;
        }

        // DOGSTATSD-SPECIFIC

        // =-=-=-=- HISTOGRAM -=-=-=-=
        [Test]
        public void send_histogram()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram> ("histogram", 5);
            udp.AssertWasCalled (x => x.Send ("histogram:5|h"));
        }

        [Test]
        public void send_histogram_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5, 0.5);
            udp.AssertWasCalled(x => x.Send ("histogram:5|h|@0.5"));
        }

        [Test]
        public void send_histogram_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5, 0.5, "tag1:true");
            udp.AssertWasCalled(x => x.Send ("histogram:5|h|@0.5|#tag1:true"));
        }

        [Test]
        public void send_histogram_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5, 0.5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send ("histogram:5|h|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void send_histogram_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5, "tag1:true");
            udp.AssertWasCalled(x => x.Send ("histogram:5|h|#tag1:true"));
        }

        [Test]
        public void send_histogram_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5, "tag1:true", "tag2");
            udp.AssertWasCalled(x => x.Send ("histogram:5|h|#tag1:true,tag2"));
        }

        [Test]
        public void add_histogram()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h"));
        }

        [Test]
        public void add_histogram_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5, 0.5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h|@0.5"));
        }

        [Test]
        public void add_histogram_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5, 0.5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h|@0.5|#tag1:true"));
        }

        [Test]
        public void add_histogram_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5, 0.5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void add_histogram_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h|#tag1:true"));
        }

        [Test]
        public void add_histogram_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Histogram>("histogram", 5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("histogram:5|h|#tag1:true,tag2"));
        }


        // =-=-=-=- SET -=-=-=-=
        [Test]
        public void send_set()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set>("set", 5);
            udp.AssertWasCalled (x => x.Send("set:5|s"));
        }

        [Test]
        public void send_set_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set>("set", 5, 0.1);
            udp.AssertWasCalled (x => x.Send("set:5|s|@0.1"));
        }

        [Test]
        public void send_set_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd (udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set> ("set", 5, 0.1, "tag1:true");
            udp.AssertWasCalled (x => x.Send("set:5|s|@0.1|#tag1:true"));
        }

        [Test]
        public void send_set_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd (udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set> ("set", 5, 0.1, "tag1:true", "tag2");
            udp.AssertWasCalled (x => x.Send("set:5|s|@0.1|#tag1:true,tag2"));
        }

        [Test]
        public void send_set_with_tag()
        {
            Statsd s = new Statsd (udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set> ("set", 5, "tag1:true");
            udp.AssertWasCalled (x => x.Send("set:5|s|#tag1:true"));
        }

        [Test]
        public void send_set_with_tags()
        {
            Statsd s = new Statsd (udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set> ("set", 5, "tag1:true", "tag2");
            udp.AssertWasCalled (x => x.Send("set:5|s|#tag1:true,tag2"));
        }

        [Test]
        public void add_set()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s"));
        }

        [Test]
        public void add_set_with_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5, 0.5);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s|@0.5"));
        }

        [Test]
        public void add_set_with_sample_rate_and_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5, 0.5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s|@0.5|#tag1:true"));
        }

        [Test]
        public void add_set_with_sample_rate_and_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5, 0.5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s|@0.5|#tag1:true,tag2"));
        }

        [Test]
        public void add_set_with_tag()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5, "tag1:true");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s|#tag1:true"));
        }

        [Test]
        public void add_set_with_tags()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Set>("set", 5, "tag1:true", "tag2");

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("set:5|s|#tag1:true,tag2"));
        }
    }
}
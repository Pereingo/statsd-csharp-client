using System;
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
        public void increases_counter_with_value_of_X()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Counting>("counter", 5);
            udp.AssertWasCalled(x => x.Send("counter:5|c"));
        }

		[Test]
		public void increases_counter_with_value_of_X_and_sample_rate()
		{
			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
			s.Send<Statsd.Counting>("counter", 5,0.1);
			udp.AssertWasCalled(x => x.Send("counter:5|c|@0.1"));
		}

		[Test]
		public void counting_exception_fails_silently()
		{
			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
			udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
			s.Send<Statsd.Counting>("counter", 5);
			Assert.Pass();
		}


		// =-=-=-=- TIMER -=-=-=-=

        [Test]
        public void adds_timing()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5);
            udp.AssertWasCalled(x => x.Send("timer:5|ms"));
        }

        [Test]
        public void timing_with_value_of_X_and_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5, 0.1);
            udp.AssertWasCalled(x => x.Send("timer:5|ms|@0.1"));
        }

		[Test]
		public void timing_exception_fails_silently()
		{
			udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
			Statsd s = new Statsd(udp);
			s.Send<Statsd.Timing>("timer", 5);
			Assert.Pass();
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
        public void add_timer_with_lamba_and_sampleRate_passes()
        {
            const string statName = "name";

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName, 0.1);

            Assert.That(s.Commands.Count, Is.EqualTo(1));
            Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
        }

        [Test]
        public void add_timer_with_lamba_and_sampleRate_fails()
        {
            const string statName = "name";

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName, 0.1);

            Assert.That(s.Commands.Count, Is.EqualTo(0));
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
        public void send_timer_with_lambda_and_sampleRate_passes()
        {
            const string statName = "name";
            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send(() => testMethod(), statName);

            udp.AssertWasCalled(x => x.Send("name:500|ms"));
        }


        [Test]
        public void send_timer_with_lambda_and_sampleRate_fails()
        {
            const string statName = "name";
            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send(() => testMethod(), statName);

            udp.AssertWasNotCalled(x => x.Send("name:500|ms"));
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
		public void set_return_value_with_send_timer_with_lambda()
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

		// =-=-=-=- GAUGE -=-=-=-=
		
        [Test]
        public void adds_gauge_with_large_double_values()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 34563478564785);
            udp.AssertWasCalled(x => x.Send("gauge:34563478564785.000000000000000|g"));
        }

        [Test]
        public void gauge_exception_fails_silently()
        {
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Gauge>("gauge", 5.0);
            Assert.Pass();
        }

        // =-=-=-=- METER -=-=-=-=

        [Test]
        public void adds_meter()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Meter>("meter", 5);
            udp.AssertWasCalled(x => x.Send("meter:5|m"));
        }

        [Test]
        public void meter_exception_fails_silently()
        {
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Meter>("meter", 5);
            Assert.Pass();
        }

        // =-=-=-=- HISTOGRAM -=-=-=-=

        [Test]
        public void adds_histogram()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Histogram>("histogram", 5);
            udp.AssertWasCalled(x => x.Send("histogram:5|h"));
        }

        [Test]
        public void histrogram_exception_fails_silently()
        {
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Histogram>("histogram", 5);
            Assert.Pass();
        }

        // =-=-=-=- SET -=-=-=-=

        [Test]
        public void adds_set_with_string_value()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Set>("set", "34563478564785xyz");
            udp.AssertWasCalled(x => x.Send("set:34563478564785xyz|s"));
        }

        [Test]
        public void set_exception_fails_silently()
        {
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Set>("set", "silent-exception-test");
            Assert.Pass();
        }

		// =-=-=-=- COMBINATION -=-=-=-=

        [Test]
        public void add_one_counter_and_one_gauge_shows_in_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1, 0.1);
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
        public void add_one_counter_and_one_timer_sends_in_one_go()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            udp.AssertWasCalled(x => x.Send("counter:1|c|@0.1\ntimer:1|ms"));
        }

        [Test]
        public void add_one_counter_and_one_timer_sends_and_removes_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            Assert.That(s.Commands.Count, Is.EqualTo(0));
        }

        [Test]
        public void add_one_counter_and_send_one_timer_sends_only_sends_the_last()
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

            s.Add<Statsd.Counting>("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            udp.AssertWasCalled(x => x.Send("another.prefix.counter:1|c|@0.1\nanother.prefix.timer:1|ms"));
        }

        private int testMethod()
        {
            return 5;
        }
    }
}
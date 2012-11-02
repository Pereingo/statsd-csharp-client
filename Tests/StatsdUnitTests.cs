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
   
        [Test]
        public void Increases_counter_with_value_of_X()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Counting>("counter", 5);
            udp.AssertWasCalled(x => x.Send("counter:5|c" + Environment.NewLine));
        }

        [Test]
        public void Adds_timing()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Timing>("timer", 5);
            udp.AssertWasCalled(x => x.Send("timer:5|ms" + Environment.NewLine));
        }

        [Test]
        public void Increases_counter_with_value_of_X_and_sample_rate()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send("counter", 5,0.1);
            udp.AssertWasCalled(x => x.Send("counter:5|c|@0.1" + Environment.NewLine));
        }

        [Test]
        public void Adds_gauge()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Send<Statsd.Gauge>("gauge", 5);
            udp.AssertWasCalled(x => x.Send("gauge:5|g" + Environment.NewLine));
        }

        [Test]
        public void counting_exception_fails_silently()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            s.Send<Statsd.Counting>("counter", 5);
            Assert.Pass();
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
        public void gauge_exception_fails_silently()
        {
            udp.Stub(x=>x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
            Statsd s = new Statsd(udp);
            s.Send<Statsd.Gauge>("gauge", 5);
            Assert.Pass();
        }

        [Test]
        public void add_one_counter_and_one_gauge_shows_in_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add("counter",1,0.1);
            s.Add<Statsd.Timing>("timer", 1);       
    
            Assert.That(s.Commands.Count,Is.EqualTo(2));
            Assert.That(s.Commands[0],Is.EqualTo("counter:1|c|@0.1"));
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

            udp.AssertWasCalled(x => x.Send("counter:1|c|@0.1" + Environment.NewLine + "timer:1|ms" + Environment.NewLine));
        }


        [Test]
        public void add_one_counter_and_one_gauge_sends_and_removes_commands()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add("counter", 1, 0.1);
            s.Add<Statsd.Timing>("timer", 1);
            s.Send();

            Assert.That(s.Commands.Count,Is.EqualTo(0));
        }

        [Test]
        public void add_one_counter_and_send_one_gauge_sends_only_sends_the_last()
        {
            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add<Statsd.Counting>("counter", 1);
            s.Send<Statsd.Timing>("timer", 1);

            udp.AssertWasCalled(x => x.Send("timer:1|ms" + Environment.NewLine));
        }

        [Test]
        public void add_timer_with_lamba()
        {
            const string statName = "name";
            const double sampleRate = 0.1;

            IStopwatch stopwatch = MockRepository.GenerateMock<IStopwatch>();
            stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
            _stopwatch.Stub(x => x.Get()).Return(stopwatch);

            Statsd s = new Statsd(udp, _randomGenerator, _stopwatch);
            s.Add(() => testMethod(), statName);
           
            Assert.That(s.Commands.Count,Is.EqualTo(1));
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

            udp.AssertWasCalled(x => x.Send("name:500|ms" + Environment.NewLine));       
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

            udp.AssertWasCalled(x => x.Send("name:500|ms" + Environment.NewLine));
            Assert.That(returnValue,Is.EqualTo(5));
        }

	    [Test]
	    public void set_prefix_on_stats_name_when_calling_send()
	    {
			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch, "a.prefix.");
			s.Send<Statsd.Counting>("counter", 5);
			s.Send<Statsd.Counting>("counter", 5);

			udp.AssertWasCalled(x => x.Send("a.prefix.counter:5|c" + Environment.NewLine),x => x.Repeat.Twice());
	    }

	    [Test]
	    public void add_counter_sets_prefix_on_name()
	    {
			Statsd s = new Statsd(udp, _randomGenerator, _stopwatch, "another.prefix.");

			s.Add("counter", 1, 0.1);
			s.Add<Statsd.Timing>("timer", 1);
			s.Send();

			udp.AssertWasCalled(x => x.Send("another.prefix.counter:1|c|@0.1" + Environment.NewLine + "another.prefix.timer:1|ms" + Environment.NewLine));
	    }
        private int testMethod()
        {
            return 5;
        }
    }
}

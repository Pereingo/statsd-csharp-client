using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using StatsdClient;
using StatsdClient.Senders;
using StatsdClient.MetricTypes;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class StatsdTests
    {
        private IStatsdUDP _udp;
        private IRandomGenerator _randomGenerator;
        private IStopWatchFactory _stopwatch;
        private ISender _sender;

        [SetUp]
        public void Setup()
        {
            _udp = MockRepository.GenerateMock<IStatsdUDP>();
            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);
            _stopwatch = MockRepository.GenerateMock<IStopWatchFactory>();
            _sender = MockRepository.GenerateMock<ISender>();
        }

        public class Configuration : StatsdTests
        {
            [Test]
            public void configuration_throws_if_not_given_udp()
            {
                IStatsd statsd;
                Assert.Throws<ArgumentNullException>(() => statsd = new Statsd(new Statsd.Configuration() { Udp = null }));
            }
        }

        public class Counter : StatsdTests
        {
            [Test]
            public void increases_counter_with_value_of_X()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Counting>("counter", 5);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("counter:5|c"));
            }

            [Test]
            public void increases_counter_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Counting>("counter", 5, 0.1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("counter:5|c|@0.1"));
            }

            [Test]
            public void counting_exception_fails_silently()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                s.Send<Counting>("counter", 5);
                Assert.Pass();
            }
        }

        public class Timer : StatsdTests
        {
            [Test]
            public void sends_timing()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Timing>("timer", 5);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("timer:5|ms"));
            }

            [Test]
            public void timing_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Timing>("timer", 5, 0.1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("timer:5|ms|@0.1"));
            }

            [Test]
            public void timing_exception_fails_silently()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                s.Send<Timing>("timer", 5);
                Assert.Pass();
            }

            [Test]
            public void send_timer_with_lamba()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lamba_and_sampleRate_passes()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName, 0.1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lamba_and_sampleRate_fails()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName, 0.1);

                _sender.AssertWasNotCalled(x => x.Send(Arg<Metric>.Is.Anything));
            }

            [Test]
            public void send_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });

                Assert.Throws<InvalidOperationException>(() => s.Send(() => { throw new InvalidOperationException(); }, statName));

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lambda_and_sampleRate_passes()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName, 0.1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lambda_and_sampleRate_fails()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send(() => TestMethod(), statName, 0.1);

                _sender.AssertWasNotCalled(x => x.Send(Arg<Metric>.Is.Anything));
            }

            [Test]
            public void set_return_value_with_send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                var returnValue = 0;
                s.Send(() => returnValue = TestMethod(), statName);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("name:500|ms"));
                Assert.That(returnValue, Is.EqualTo(5));
            }
        }

        public class Guage : StatsdTests
        {
            [Test]
            public void send_gauge_with_large_double_values()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Gauge>("gauge", 34563478564785);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("gauge:34563478564785.000000000000000|g"));
            }

            [Test]
            public void gauge_exception_fails_silently()
            {
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Gauge>("gauge", 5.0);
                Assert.Pass();
            }
        }

        public class Meter : StatsdTests
        {
            [Test]
            public void send_meter()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<StatsdClient.MetricTypes.Meter>("meter", 5);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("meter:5|m"));
            }

            [Test]
            public void meter_exception_fails_silently()
            {
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });

                s.Send<StatsdClient.MetricTypes.Meter>("meter", 5);
                Assert.Pass();
            }
        }

        public class Histogram : StatsdTests
        {
            [Test]
            public void sends_histogram()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<StatsdClient.MetricTypes.Histogram>("histogram", 5);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("histogram:5|h"));
            }

            [Test]
            public void histogram_exception_fails_silently()
            {
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<StatsdClient.MetricTypes.Histogram>("histogram", 5);
                Assert.Pass();
            }
        }

        public class Set : StatsdTests
        {
            [Test]
            public void sets_set_with_string_value()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<StatsdClient.MetricTypes.Set>("set", "34563478564785xyz");

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("set:34563478564785xyz|s"));
            }

            [Test]
            public void set_exception_fails_silently()
            {
                _udp.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<StatsdClient.MetricTypes.Set>("set", "silent-exception-test");
                Assert.Pass();
            }
        }

        public class Combination : StatsdTests
        {
            [Test]
            public void send_one_counter_and_one_timer_shows_in_commands()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Counting>("counter", 1, 0.1);
                s.Send<Timing>("timer", 1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(2));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("counter:1|c|@0.1"));
                Assert.That(((Metric)argsPerCall[1][0]).Command, Is.EqualTo("timer:1|ms"));
            }

            [Test]
            public void send_one_counter_and_one_timer_with_no_sample_rate_shows_in_commands()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                s.Send<Counting>("counter", 1);
                s.Send<Timing>("timer", 1);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(2));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("counter:1|c"));
                Assert.That(((Metric)argsPerCall[1][0]).Command, Is.EqualTo("timer:1|ms"));
            }
        }

        public class NamePrefixing : StatsdTests
        {
            [Test]
            public void set_prefix_on_stats_name_when_calling_send()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender, Prefix = "a.prefix." });
                s.Send<Counting>("counter", 5);

                IList<object[]> argsPerCall = _sender.GetArgumentsForCallsMadeOn(x => x.Send(Arg<Metric>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((Metric)argsPerCall[0][0]).Command, Is.EqualTo("a.prefix.counter:5|c"));
            }
        }

        public class Concurrency : StatsdTests
        {
            [Test]
            public void can_concurrently_send_integer_metrics()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                Parallel.For(0, 50000, x => Assert.DoesNotThrow(() => s.Send<Counting>("name", 5)));
            }

            [Test]
            public void can_concurrently_send_double_metrics()
            {
                var s = new Statsd(new Statsd.Configuration() { Udp = _udp, RandomGenerator = _randomGenerator, StopwatchFactory = _stopwatch, Sender = _sender });
                Parallel.For(0, 50000, x => Assert.DoesNotThrow(() => s.Send<Gauge>("name", 5d)));
            }
        }

        private static int TestMethod()
        {
            return 5;
        }
    }
}
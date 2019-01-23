using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class StatsdTests
    {
        private IStatsdClient _udp;
        private IRandomGenerator _randomGenerator;
        private IStopWatchFactory _stopwatch;

        [SetUp]
        public void Setup()
        {
            _udp = Substitute.For<IStatsdClient>();
            _stopwatch = Substitute.For<IStopWatchFactory>();
            _randomGenerator = Substitute.For<IRandomGenerator>();
            _randomGenerator.ShouldSend(0).ReturnsForAnyArgs(true);
        }

        public class Counter : StatsdTests
        {
            [Test]
            public void increases_counter_with_value_of_X()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Counting>("counter", 5);
                _udp.Received().SendAsync("counter:5|c");
            }

            [Test]
            public void increases_counter_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Counting>("counter", 5, 0.1);
                _udp.Received().SendAsync("counter:5|c|@0.1");
            }

            [Test]
            public void counting_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Counting>("counter", 5);
            }
        }

        public class Timer : StatsdTests
        {
            [Test]
            public void adds_timing()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Timing>("timer", 5);
                _udp.Received().SendAsync("timer:5|ms");
            }

            [Test]
            public void timing_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Timing>("timer", 5, 0.1);
                _udp.Received().SendAsync("timer:5|ms|@0.1");
            }

            [Test]
            public void timing_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp);
                s.Send<Statsd.Timing>("timer", 5);
            }

            [Test]
            public void add_timer_with_lamba()
            {
                const string statName = "name";

                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);

                var statsd = new Statsd(_udp, _randomGenerator, _stopwatch);
                statsd.Add(() => TestMethod(), statName);

                Assert.That(statsd.Commands.Single(), Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void add_timer_with_lamba_and_sampleRate_passes()
            {
                const string statName = "name";

                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);
                _randomGenerator = Substitute.For<IRandomGenerator>();
                _randomGenerator.ShouldSend(0).ReturnsForAnyArgs(true);

                var statsd = new Statsd(_udp, _randomGenerator, _stopwatch);
                statsd.Add(() => TestMethod(), statName, 0.1);

                Assert.That(statsd.Commands.Single(), Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void add_timer_with_lamba_and_sampleRate_fails()
            {
                const string statName = "name";

                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);
                _randomGenerator = Substitute.For<IRandomGenerator>();
                _randomGenerator.ShouldSend(0).ReturnsForAnyArgs(false);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add(() => TestMethod(), statName, 0.1);

                Assert.That(s.Commands.Count, Is.EqualTo(0));
            }

            [Test]
            public void add_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
            {
                const string statName = "name";

                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);

                Assert.Throws<InvalidOperationException>(() => s.Add(() => { throw new InvalidOperationException(); }, statName));

                Assert.That(s.Commands.Count, Is.EqualTo(1));
                Assert.That(s.Commands.ToArray()[0], Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send(() => TestMethod(), statName);

                _udp.Received().SendAsync("name:500|ms");
            }

            [Test]
            public void send_timer_with_lambda_and_sampleRate_passes()
            {
                const string statName = "name";
                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);
                _randomGenerator = Substitute.For<IRandomGenerator>();
                _randomGenerator.ShouldSend(0).ReturnsForAnyArgs(true);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send(() => TestMethod(), statName);

                _udp.Received().SendAsync("name:500|ms");
            }


            [Test]
            public void send_timer_with_lambda_and_sampleRate_fails()
            {
                const string statName = "name";
                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);
                _randomGenerator = Substitute.For<IRandomGenerator>();
                _randomGenerator.ShouldSend(0).ReturnsForAnyArgs(false);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send(() => TestMethod(), statName);

                _udp.DidNotReceive().SendAsync("name:500|ms");
            }

            [Test]
            public void send_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
            {
                const string statName = "name";
                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                Assert.Throws<InvalidOperationException>(() => s.Send(() => { throw new InvalidOperationException(); }, statName));

                _udp.Received().SendAsync("name:500|ms");
            }

            [Test]
            public void set_return_value_with_send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = Substitute.For<IStopwatch>();
                stopwatch.ElapsedMilliseconds.Returns(500);
                _stopwatch.Get().Returns(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                var returnValue = 0;
                s.Send(() => returnValue = TestMethod(), statName);

                _udp.Received().SendAsync("name:500|ms");
                Assert.That(returnValue, Is.EqualTo(5));
            }
        }

        public class Guage : StatsdTests
        {
            [Test]
            public void adds_gauge_with_large_double_values()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Gauge>("gauge", 34563478564785);
                _udp.Received().SendAsync("gauge:34563478564785.000000000000000|g");
            }

            [Test]
            public void gauge_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp);
                s.Send<Statsd.Gauge>("gauge", 5.0);
            }

            [Test]
            [TestCase(true, 10d, "delta-gauge:+10|g")]
            [TestCase(true, -10d, "delta-gauge:-10|g")]
            [TestCase(true, 0d, "delta-gauge:+0|g")]
            [TestCase(false, 10d, "delta-gauge:10.000000000000000|g")]//because it is looped through to original Gauge send function
            public void adds_gauge_with_deltaValue_formatsCorrectly(bool isDeltaValue, double value, string expectedFormattedStatsdMessage)
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Gauge>("delta-gauge", value, isDeltaValue);
                _udp.Received().SendAsync(expectedFormattedStatsdMessage);
            }
        }

        public class Meter : StatsdTests
        {
            [Test]
            public void adds_meter()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Meter>("meter", 5);
                _udp.Received().SendAsync("meter:5|m");
            }

            [Test]
            public void meter_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp);
                s.Send<Statsd.Meter>("meter", 5);
            }
        }

        public class Historgram : StatsdTests
        {
            [Test]
            public void adds_histogram()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Histogram>("histogram", 5);
                _udp.Received().SendAsync("histogram:5|h");
            }

            [Test]
            public void histrogram_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp);
                s.Send<Statsd.Histogram>("histogram", 5);
            }
        }

        public class Set : StatsdTests
        {
            [Test]
            public void adds_set_with_string_value()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Send<Statsd.Set>("set", "34563478564785xyz");
                _udp.Received().SendAsync("set:34563478564785xyz|s");
            }

            [Test]
            public void set_exception_fails_silently()
            {
                GivenUdpSendFails();
                var s = new Statsd(_udp);
                s.Send<Statsd.Set>("set", "silent-exception-test");
            }
        }

        public class Combination : StatsdTests
        {
            [Test]
            public void add_one_counter_and_one_gauge_shows_in_commands()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);

                Assert.That(s.Commands.Count, Is.EqualTo(2));
                Assert.That(s.Commands.ToArray()[0], Is.EqualTo("counter:1|c|@0.1"));
                Assert.That(s.Commands.ToArray()[1], Is.EqualTo("timer:1|ms"));
            }

            [Test]
            public void add_one_counter_and_one_gauge_with_no_sample_rate_shows_in_commands()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1);
                s.Add<Statsd.Timing>("timer", 1);

                Assert.That(s.Commands.Count, Is.EqualTo(2));
                Assert.That(s.Commands.ToArray()[0], Is.EqualTo("counter:1|c"));
                Assert.That(s.Commands.ToArray()[1], Is.EqualTo("timer:1|ms"));
            }

            [Test]
            public void add_one_counter_and_one_timer_sends_in_one_go()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                s.Send();

                _udp.Received().SendAsync("counter:1|c|@0.1\ntimer:1|ms");
            }

            [Test]
            public void add_one_counter_and_one_timer_sends_and_removes_commands()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                s.Send();

                Assert.That(s.Commands.Count, Is.EqualTo(0));
            }

            [Test]
            public void add_one_counter_and_send_one_timer_sends_only_sends_the_last()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1);
                s.Send<Statsd.Timing>("timer", 1);

                _udp.Received().SendAsync("timer:1|ms");
            }
        }

        public class NamePrefixing : StatsdTests
        {
            [Test]
            public void set_prefix_on_stats_name_when_calling_send()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch, "a.prefix.");
                s.Send<Statsd.Counting>("counter", 5);
                s.Send<Statsd.Counting>("counter", 5);

                _udp.Received(2).SendAsync("a.prefix.counter:5|c");
            }

            [Test]
            public void add_counter_sets_prefix_on_name()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch, "another.prefix.");

                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                s.Send();

                _udp.Received().SendAsync("another.prefix.counter:1|c|@0.1\nanother.prefix.timer:1|ms");
            }
        }

        public class ThreadSafety : StatsdTests
        {
            private const int ThreadCount = 10000;
            private Statsd _stats;

            [SetUp]
            public void Before_each()
            {
                _stats = new Statsd(_udp, _randomGenerator, _stopwatch);
            }

            [Test]
            public void add_counters()
            {
                Parallel.For(0, ThreadCount, x => Assert.DoesNotThrow(() => _stats.Add<Statsd.Counting>("random-name", 5)));
            }

            [Test]
            public void add_gauges()
            {
                Parallel.For(0, ThreadCount, x => Assert.DoesNotThrow(() => _stats.Add<Statsd.Gauge>("random-name", 5d)));
            }

            [Test]
            public void send_counters()
            {
                Parallel.For(0, ThreadCount, x => _stats.Send<Statsd.Counting>(Guid.NewGuid().ToString(), 5));
                Assert.That(DistinctMetricsSent(), Is.EqualTo(ThreadCount));
            }

            [Test]
            public void send_absolute_gauge()
            {
                Parallel.For(0, ThreadCount, x => _stats.Send<Statsd.Gauge>(Guid.NewGuid().ToString(), 5d));
                Assert.That(DistinctMetricsSent(), Is.EqualTo(ThreadCount));
            }

            [Test]
            public void send_delta_gauge()
            {
                Parallel.For(0, ThreadCount, x => _stats.Send<Statsd.Gauge>(Guid.NewGuid().ToString(), 5d, true));
                Assert.That(DistinctMetricsSent(), Is.EqualTo(ThreadCount));
            }

            [Test]
            public void send_set()
            {
                Parallel.For(0, ThreadCount, x => _stats.Send<Statsd.Set>(Guid.NewGuid().ToString(), "foo"));
                Assert.That(DistinctMetricsSent(), Is.EqualTo(ThreadCount));
            }

            [Test]
            public void send_sampled_timer()
            {
                Parallel.For(0, ThreadCount, x => _stats.Send<Statsd.Timing>(Guid.NewGuid().ToString(), 5, 1d));
                Assert.That(DistinctMetricsSent(), Is.EqualTo(ThreadCount));
            }

            private int DistinctMetricsSent()
            {
                return _udp.ReceivedCalls().Select(x => x.GetArguments()[0]).Distinct().Count();
            }
        }

        private static int TestMethod()
        {
            return 5;
        }

        private void GivenUdpSendFails()
        {
            _udp.When(x => x.SendAsync(Arg.Any<string>())).Do(x => { throw new Exception(); });
        }
    }
}

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class StatsdTests
    {
        private IStatsdUDP _udp;
        private IRandomGenerator _randomGenerator;
        private IStopWatchFactory _stopwatch;

        [SetUp]
        public void Setup()
        {
            _udp = MockRepository.GenerateMock<IStatsdUDP>();
            _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Return(Task.FromResult(0));

            _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
            _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);
            _stopwatch = MockRepository.GenerateMock<IStopWatchFactory>();
        }

        public class Counter : StatsdTests
        {
            [Test]
            public async Task increases_counter_with_value_of_X()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Counting>("counter", 5);
                _udp.AssertWasCalled(x => x.SendAsync("counter:5|c"));
            }

            [Test]
            public async Task increases_counter_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Counting>("counter", 5, 0.1);
                _udp.AssertWasCalled(x => x.SendAsync("counter:5|c|@0.1"));
            }

            [Test]
            public async Task counting_exception_fails_silently()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                await s.SendAsync<Statsd.Counting>("counter", 5);
                Assert.Pass();
            }
        }

        public class Timer : StatsdTests
        {
            [Test]
            public async Task adds_timing()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Timing>("timer", 5);
                _udp.AssertWasCalled(x => x.SendAsync("timer:5|ms"));
            }

            [Test]
            public async Task timing_with_value_of_X_and_sample_rate()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Timing>("timer", 5, 0.1);
                _udp.AssertWasCalled(x => x.SendAsync("timer:5|ms|@0.1"));
            }

            [Test]
            public async Task timing_exception_fails_silently()
            {
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(_udp);
                await s.SendAsync<Statsd.Timing>("timer", 5);
                Assert.Pass();
            }

            [Test]
            public void add_timer_with_lamba()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add(() => TestMethod(), statName);

                Assert.That(s.Commands.Count, Is.EqualTo(1));
                Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void add_timer_with_lamba_and_sampleRate_passes()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add(() => TestMethod(), statName, 0.1);

                Assert.That(s.Commands.Count, Is.EqualTo(1));
                Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
            }

            [Test]
            public void add_timer_with_lamba_and_sampleRate_fails()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add(() => TestMethod(), statName, 0.1);

                Assert.That(s.Commands.Count, Is.EqualTo(0));
            }

            [Test]
            public void add_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
            {
                const string statName = "name";

                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);

                Assert.Throws<InvalidOperationException>(() => s.Add(() => { throw new InvalidOperationException(); }, statName));

                Assert.That(s.Commands.Count, Is.EqualTo(1));
                Assert.That(s.Commands[0], Is.EqualTo("name:500|ms"));
            }

            [Test]
            public async Task send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync(() => TestMethod(), statName);

                _udp.AssertWasCalled(x => x.SendAsync("name:500|ms"));
            }

            [Test]
            public async Task send_timer_with_lambda_and_sampleRate_passes()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(true);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync(() => TestMethod(), statName);

                _udp.AssertWasCalled(x => x.SendAsync("name:500|ms"));
            }


            [Test]
            public async Task send_timer_with_lambda_and_sampleRate_fails()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);
                _randomGenerator = MockRepository.GenerateMock<IRandomGenerator>();
                _randomGenerator.Stub(x => x.ShouldSend(Arg<double>.Is.Anything)).Return(false);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync(() => TestMethod(), statName);

                _udp.AssertWasNotCalled(x => x.SendAsync("name:500|ms"));
            }

            [Test]
            public void send_timer_with_lamba_still_records_on_error_and_still_bubbles_up_exception()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                Assert.Throws<InvalidOperationException>(async () => await s.SendAsync(() => { throw new InvalidOperationException(); }, statName));

                _udp.AssertWasCalled(x => x.SendAsync("name:500|ms"));
            }

            [Test]
            public async Task set_return_value_with_send_timer_with_lambda()
            {
                const string statName = "name";
                var stopwatch = MockRepository.GenerateMock<IStopwatch>();
                stopwatch.Stub(x => x.ElapsedMilliseconds()).Return(500);
                _stopwatch.Stub(x => x.Get()).Return(stopwatch);

                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                var returnValue = 0;
                await s.SendAsync(() => returnValue = TestMethod(), statName);

                _udp.AssertWasCalled(x => x.SendAsync("name:500|ms"));
                Assert.That(returnValue, Is.EqualTo(5));
            }
        }

        public class Guage : StatsdTests
        {
            [Test]
            public async Task adds_gauge_with_large_double_values()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Gauge>("gauge", 34563478564785D);
                _udp.AssertWasCalled(x => x.SendAsync("gauge:34563478564785.000000000000000|g"));
            }

            [Test]
            public async Task gauge_exception_fails_silently()
            {
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(_udp);
                await s.SendAsync<Statsd.Gauge>("gauge", 5.0);
                Assert.Pass();
            }

            [Test]
            [TestCase(true, 10d, "delta-gauge:+10|g")]
            [TestCase(true, -10d, "delta-gauge:-10|g")]
            [TestCase(true, 0d, "delta-gauge:+0|g")]
            [TestCase(false, 10d, "delta-gauge:10.000000000000000|g")]//because it is looped through to original Gauge send function
            public async Task adds_gauge_with_deltaValue_formatsCorrectly(bool isDeltaValue, double value, string expectedFormattedStatsdMessage)
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Gauge>("delta-gauge", value, isDeltaValue);
                _udp.AssertWasCalled(x => x.SendAsync(expectedFormattedStatsdMessage));
            }
        }

        public class Meter : StatsdTests
        {
            [Test]
            public async Task adds_meter()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Meter>("meter", 5);
                _udp.AssertWasCalled(x => x.SendAsync("meter:5|m"));
            }

            [Test]
            public async Task meter_exception_fails_silently()
            {
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(_udp);
                await s.SendAsync<Statsd.Meter>("meter", 5);
                Assert.Pass();
            }
        }

        public class Historgram : StatsdTests
        {
            [Test]
            public async Task adds_histogram()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Histogram>("histogram", 5);
                _udp.AssertWasCalled(x => x.SendAsync("histogram:5|h"));
            }

            [Test]
            public async Task histrogram_exception_fails_silently()
            {
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(_udp);
                await s.SendAsync<Statsd.Histogram>("histogram", 5);
                Assert.Pass();
            }
        }

        public class Set : StatsdTests
        {
            [Test]
            public async Task adds_set_with_string_value()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                await s.SendAsync<Statsd.Set>("set", "34563478564785xyz");
                _udp.AssertWasCalled(x => x.SendAsync("set:34563478564785xyz|s"));
            }

            [Test]
            public async Task set_exception_fails_silently()
            {
                _udp.Stub(x => x.SendAsync(Arg<string>.Is.Anything)).Throw(new Exception());
                var s = new Statsd(_udp);
                await s.SendAsync<Statsd.Set>("set", "silent-exception-test");
                Assert.Pass();
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
                Assert.That(s.Commands[0], Is.EqualTo("counter:1|c|@0.1"));
                Assert.That(s.Commands[1], Is.EqualTo("timer:1|ms"));
            }

            [Test]
            public void add_one_counter_and_one_gauge_with_no_sample_rate_shows_in_commands()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1);
                s.Add<Statsd.Timing>("timer", 1);

                Assert.That(s.Commands.Count, Is.EqualTo(2));
                Assert.That(s.Commands[0], Is.EqualTo("counter:1|c"));
                Assert.That(s.Commands[1], Is.EqualTo("timer:1|ms"));
            }

            [Test]
            public async Task add_one_counter_and_one_timer_sends_in_one_go()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                await s.SendAsync();

                _udp.AssertWasCalled(x => x.SendAsync("counter:1|c|@0.1\ntimer:1|ms"));
            }

            [Test]
            public async Task add_one_counter_and_one_timer_sends_and_removes_commands()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                await s.SendAsync();

                Assert.That(s.Commands.Count, Is.EqualTo(0));
            }

            [Test]
            public async Task add_one_counter_and_send_one_timer_sends_only_sends_the_last()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);
                s.Add<Statsd.Counting>("counter", 1);
                await s.SendAsync<Statsd.Timing>("timer", 1);

                _udp.AssertWasCalled(x => x.SendAsync("timer:1|ms"));
            }
        }

        public class NamePrefixing : StatsdTests
        {
            [Test]
            public async Task set_prefix_on_stats_name_when_calling_send()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch, "a.prefix.");
                await s.SendAsync<Statsd.Counting>("counter", 5);
                await s.SendAsync<Statsd.Counting>("counter", 5);

                _udp.AssertWasCalled(x => x.SendAsync("a.prefix.counter:5|c"), x => x.Repeat.Twice());
            }

            [Test]
            public async Task add_counter_sets_prefix_on_name()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch, "another.prefix.");

                s.Add<Statsd.Counting>("counter", 1, 0.1);
                s.Add<Statsd.Timing>("timer", 1);
                await s.SendAsync();

                _udp.AssertWasCalled(x => x.SendAsync("another.prefix.counter:1|c|@0.1\nanother.prefix.timer:1|ms"));
            }
        }

        public class Concurrency : StatsdTests
        {
            [Test]
            public void can_concurrently_add_integer_metrics()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);

                Parallel.For(0, 1000000, x => Assert.DoesNotThrow(() => s.Add<Statsd.Counting>("name", 5)));
            }

            [Test]
            public void can_concurrently_add_double_metrics()
            {
                var s = new Statsd(_udp, _randomGenerator, _stopwatch);

                Parallel.For(0, 1000000, x => Assert.DoesNotThrow(() => s.Add<Statsd.Gauge>("name", 5d)));
            }
        }

        private static int TestMethod()
        {
            return 5;
        }
    }
}
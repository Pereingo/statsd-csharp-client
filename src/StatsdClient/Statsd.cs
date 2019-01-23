using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace StatsdClient
{
    public interface IAllowsSampleRate { }
    public interface IAllowsDelta { }

    public interface IAllowsDouble { }
    public interface IAllowsInteger { }
    public interface IAllowsString { }

    public class Statsd : IStatsd
    {
#if NET45
        private static readonly Task CompletedTask = Task.FromResult<object>(null);
#else
        private static readonly Task CompletedTask = Task.CompletedTask;
#endif
        private readonly object _commandCollectionLock = new object();

        private IStopWatchFactory StopwatchFactory { get; set; }
        private IStatsdClient StatsdClient { get; set; }
        private IRandomGenerator RandomGenerator { get; set; }

        private readonly string _prefix;

        internal ConcurrentQueue<string> Commands { get; private set; }

        public class Counting : IAllowsSampleRate, IAllowsInteger { }
        public class Timing : IAllowsSampleRate, IAllowsInteger { }
        public class Gauge : IAllowsDouble, IAllowsDelta { }
        public class Histogram : IAllowsInteger { }
        public class Meter : IAllowsInteger { }
        public class Set : IAllowsString { }

        private readonly IDictionary<Type, string> _commandToUnit = new Dictionary<Type, string>
                                                                       {
                                                                           {typeof (Counting), "c"},
                                                                           {typeof (Timing), "ms"},
                                                                           {typeof (Gauge), "g"},
                                                                           {typeof (Histogram), "h"},
                                                                           {typeof (Meter), "m"},
                                                                           {typeof (Set), "s"}
                                                                       };

        public Statsd(IStatsdClient statsdClient, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory, string prefix)
        {
            Commands = new ConcurrentQueue<string>();
            StopwatchFactory = stopwatchFactory;
            StatsdClient = statsdClient;
            RandomGenerator = randomGenerator;
            _prefix = prefix;
        }

        public Statsd(IStatsdClient statsdClient, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory)
            : this(statsdClient, randomGenerator, stopwatchFactory, string.Empty) { }

        public Statsd(IStatsdClient statsdClient)
            : this(statsdClient, new RandomGenerator(), new StopWatchFactory(), string.Empty) { }

        public void Send<TCommandType>(string name, int value) where TCommandType : IAllowsInteger =>
            SendAsync<TCommandType>(name, value).GetAwaiter().GetResult();

        public Task SendAsync<TCommandType>(string name, int value) where TCommandType : IAllowsInteger =>
            SendSingleAsync(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1));

        public void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble =>
            SendAsync<TCommandType>(name, value).GetAwaiter().GetResult();

        public Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            var formattedValue = string.Format(CultureInfo.InvariantCulture, "{0:F15}", value);

            return SendSingleAsync(GetCommand(name, formattedValue, _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Send<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta =>
            SendAsync<TCommandType>(name, value, isDeltaValue).GetAwaiter().GetResult();

        public Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
            if (isDeltaValue)
            {
                // Sending delta values to StatsD requires a value modifier sign (+ or -) which we append
                // using this custom format with a different formatting rule for negative/positive and zero values
                // https://msdn.microsoft.com/en-us/library/0c899ak8.aspx#SectionSeparator
                const string deltaValueStringFormat = "{0:+#.###;-#.###;+0}";
                var formattedValue = string.Format(CultureInfo.InvariantCulture, deltaValueStringFormat, value);
                var command = GetCommand(name, formattedValue, _commandToUnit[typeof(TCommandType)], 1);
                return SendSingleAsync(command);
            }

            return SendAsync<TCommandType>(name, value);
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString =>
            SendAsync<TCommandType>(name, value).GetAwaiter().GetResult();

        public Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString =>
            SendSingleAsync(GetCommand(name, Convert.ToString(value, CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1));

        public void Add<TCommandType>(string name, int value) where TCommandType : IAllowsInteger => Commands.Enqueue(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1));

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble => Commands.Enqueue(GetCommand(name, String.Format(CultureInfo.InvariantCulture, "{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1));

        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate =>
            SendAsync<TCommandType>(name, value, sampleRate).GetAwaiter().GetResult();

        public Task SendAsync<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate =>
            RandomGenerator.ShouldSend(sampleRate)
                ? SendSingleAsync(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate))
                : CompletedTask;

        public void Add<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
                Commands.Enqueue(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate));
        }

        public void Send() => SendAsync().GetAwaiter().GetResult();

        public async Task SendAsync()
        {
            try
            {
                await StatsdClient.SendAsync(string.Join("\n", Commands.ToArray())).ConfigureAwait(false);
                AtomicallyClearQueue();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void AtomicallyClearQueue()
        {
            lock (_commandCollectionLock)
            {
                Commands = new ConcurrentQueue<string>();
            }
        }

        private string GetCommand(string name, string value, string unit, double sampleRate)
        {
            var format = Math.Abs(sampleRate - 1) < 0.00000001 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";

            return string.Format(CultureInfo.InvariantCulture, format, _prefix + name, value, unit, sampleRate);
        }

        public void Add(Action actionToTime, string statName, double sampleRate = 1) =>
            HandleTiming(actionToTime, statName, sampleRate, Add<Timing>);

        public void Send(Action actionToTime, string statName, double sampleRate = 1) =>
            HandleTiming(actionToTime, statName, sampleRate, Send<Timing>);

        private void HandleTiming(Action actionToTime, string statName, double sampleRate, Action<string, int> actionToStore)
        {
            var stopwatch = StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                if (RandomGenerator.ShouldSend(sampleRate))
                {
                    actionToStore(statName, stopwatch.ElapsedMilliseconds);
                }
            }
        }

        private async Task SendSingleAsync(string command)
        {
            try
            {
                await StatsdClient.SendAsync(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public Task AddAsync(Func<Task> actionToTime, string statName, double sampleRate = 1) =>
            HandleTiming(actionToTime, statName, sampleRate, Add<Timing>);

        public Task SendAsync(Func<Task> actionToTime, string statName, double sampleRate = 1) =>
            HandleTiming(actionToTime, statName, sampleRate, Send<Timing>);

        private async Task HandleTiming(Func<Task> actionToTime, string statName, double sampleRate, Action<string, int> actionToStore)
        {
            var stopwatch = StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                await actionToTime().ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();
                if (RandomGenerator.ShouldSend(sampleRate))
                {
                    actionToStore(statName, stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}

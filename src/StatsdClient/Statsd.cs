using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace StatsdClient
{
    public interface IAllowsSampleRate { }
    public interface IAllowsDelta { }

    public interface IAllowsDouble { }
    public interface IAllowsInteger { }
    public interface IAllowsString { }

    public class Statsd : IStatsd
    {
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

        public void Send<TCommandType>(string name, int value) where TCommandType : IAllowsInteger
        {
            var command = GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1);
            SendSingle(command);
        }

        public void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            var formattedValue = string.Format(CultureInfo.InvariantCulture,"{0:F15}", value);
            var command = GetCommand(name, formattedValue, _commandToUnit[typeof(TCommandType)], 1);
            SendSingle(command);
        }

        public void Send<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
          if (isDeltaValue)
          {
              // Sending delta values to StatsD requires a value modifier sign (+ or -) which we append 
              // using this custom format with a different formatting rule for negative/positive and zero values
              // https://msdn.microsoft.com/en-us/library/0c899ak8.aspx#SectionSeparator
              const string deltaValueStringFormat = "{0:+#.###;-#.###;+0}";
              var formattedValue = string.Format(CultureInfo.InvariantCulture, deltaValueStringFormat, value);
              var command = GetCommand(name, formattedValue, _commandToUnit[typeof(TCommandType)], 1);
              SendSingle(command);
          }
          else
          {
              Send<TCommandType>(name, value);
          }
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
            var command = GetCommand(name, Convert.ToString(value, CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1);
            SendSingle(command);
        }

        public void Add<TCommandType>(string name, int value) where TCommandType : IAllowsInteger
        {
            Commands.Enqueue(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof (TCommandType)], 1));
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            Commands.Enqueue(GetCommand(name, String.Format(CultureInfo.InvariantCulture,"{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (!RandomGenerator.ShouldSend(sampleRate))
            {
                return;
            }

            var command = GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate);
            SendSingle(command);
        }

        public void Add<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Commands.Enqueue(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate));
            }
        }

        private void SendSingle(string command)
        {
            try
            {
                StatsdClient.Send(command);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void Send()
        {
            try
            {
                StatsdClient.Send(string.Join("\n", Commands.ToArray()));
                AtomicallyClearQueue();
            }
            catch(Exception e)
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
            var format = sampleRate == 1 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";
            return string.Format(CultureInfo.InvariantCulture, format, _prefix + name, value, unit, sampleRate);
        }

        public void Add(Action actionToTime, string statName, double sampleRate=1)
        {
            HandleTiming(actionToTime, statName, sampleRate, Add<Timing>);
        }

        public void Send(Action actionToTime, string statName, double sampleRate=1)
        {
            HandleTiming(actionToTime, statName, sampleRate, Send<Timing>);
        }

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
    }
}

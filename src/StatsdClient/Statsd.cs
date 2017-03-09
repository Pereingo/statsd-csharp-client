using System;
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

        public List<string> Commands { get; private set; }

        public class Counting : IAllowsSampleRate, IAllowsInteger { }
        public class Timing : IAllowsSampleRate, IAllowsInteger { }
        public class Gauge : IAllowsDouble, IAllowsDelta { }
        public class Histogram : IAllowsInteger { }
        public class Meter : IAllowsInteger { }
        public class Set : IAllowsString { }

        private readonly Dictionary<Type, string> _commandToUnit = new Dictionary<Type, string>
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
            Commands = new List<string>();
            StopwatchFactory = stopwatchFactory;
            StatsdClient = statsdClient;
            RandomGenerator = randomGenerator;
            _prefix = prefix;
        }

        public Statsd(IStatsdClient statsdClient, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory)
            : this(statsdClient, randomGenerator, stopwatchFactory, string.Empty) { }

        public Statsd(IStatsdClient statsdClient, string prefix)
            : this(statsdClient, new RandomGenerator(), new StopWatchFactory(), prefix) { }

        public Statsd(IStatsdClient statsdClient)
            : this(statsdClient, "") { }


        public void Send<TCommandType>(string name, int value) where TCommandType : IAllowsInteger
        {
            Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1) };
            Send();
        }
        public void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            Commands = new List<string> { GetCommand(name, String.Format(CultureInfo.InvariantCulture,"{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1) };
            Send();
        }

        public void Send<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
          if (isDeltaValue)
          {
              // Sending delta values to StatsD requires a value modifier sign (+ or -) which we append 
              // using this custom format with a different formatting rule for negative/positive and zero values
              // https://msdn.microsoft.com/en-us/library/0c899ak8.aspx#SectionSeparator
              const string deltaValueStringFormat = "{0:+#.###;-#.###;+0}";
              Commands = new List<string> {
                GetCommand(name, string.Format(CultureInfo.InvariantCulture, 
                deltaValueStringFormat, 
                value), 
                  _commandToUnit[typeof(TCommandType)], 1)
              };
              Send();
          }
          else
          {
              Send<TCommandType>(name, value);
          }
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
#if NETFULL
            Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1) };
#else
            Commands = new List<string> { GetCommand(name, value.ToString(), _commandToUnit[typeof(TCommandType)], 1) };
#endif
            Send();
        }

        public void Add<TCommandType>(string name, int value) where TCommandType : IAllowsInteger
        {
            ThreadSafeAddCommand(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof (TCommandType)], 1));
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            ThreadSafeAddCommand(GetCommand(name, String.Format(CultureInfo.InvariantCulture,"{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate) };
                Send();
            }
        }

        public void Add<TCommandType>(string name, int value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Commands.Add(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate));
            }
        }

        private void ThreadSafeAddCommand(string command)
        {
            lock (_commandCollectionLock)
            {
                Commands.Add(command);
            }
        }

        public void Send()
        {
            try
            {
                StatsdClient.Send(string.Join("\n", Commands.ToArray()));
                Commands = new List<string>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private string GetCommand(string name, string value, string unit, double sampleRate)
        {
            var format = sampleRate == 1 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";
            return string.Format(CultureInfo.InvariantCulture, format, _prefix + name, value, unit, sampleRate);
        }

        public void Add(Action actionToTime, string statName, double sampleRate=1)
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
                    Add<Timing>(statName, stopwatch.Elapsed.Milliseconds);
                }
            }
        }

        public void Send(Action actionToTime, string statName, double sampleRate=1)
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
                    Send<Timing>(statName, stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}

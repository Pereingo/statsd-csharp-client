using System;
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

    public partial class Statsd : IStatsd
    {
        private readonly object _commandCollectionLock = new object();

        private IStopWatchFactory StopwatchFactory { get; set; }
        private IStatsdUDP Udp { get; set; }
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

        public Statsd(IStatsdUDP udp, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory, string prefix)
        {
            Commands = new List<string>();
            StopwatchFactory = stopwatchFactory;
            Udp = udp;
            RandomGenerator = randomGenerator;
            _prefix = prefix;
        }

        public Statsd(IStatsdUDP udp, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory)
            : this(udp, randomGenerator, stopwatchFactory, string.Empty) { }

        public Statsd(IStatsdUDP udp, string prefix)
            : this(udp, new RandomGenerator(), new StopWatchFactory(), prefix) { }

        public Statsd(IStatsdUDP udp)
            : this(udp, "") { }

#if !NET451

        public async Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
            Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1) };
            await SendAsync();
        }

        public async Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            Commands = new List<string> { GetCommand(name, String.Format(CultureInfo.InvariantCulture,"{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1) };
            await SendAsync();
        }

        public async Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
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
              await SendAsync();
          }
          else
          {
              await SendAsync<TCommandType>(name, value);
          }
        }

        public async Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
            Commands = new List<string> { GetCommand(name, value, _commandToUnit[typeof(TCommandType)], 1) };
            await SendAsync();
        }

        public async Task SendAsync<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate) };
                await SendAsync();
            }
        }


        public async Task SendAsync()
        {
            try
            {
                await Udp.SendAsync(string.Join("\n", Commands.ToArray()));
                Commands = new List<string>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public async Task SendAsync(Func<Task> actionToTime, string statName, double sampleRate=1)
        {
            var stopwatch = StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                await actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                if (RandomGenerator.ShouldSend(sampleRate))
                {
                    await SendAsync<Timing>(statName, (long)stopwatch.Elapsed.TotalMilliseconds);
                }
            }
        }

        public async Task<T> SendAsync<T>(Func<Task<T>> actionToTime, string statName, double sampleRate = 1)
        {
            var stopwatch = StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                return await actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                if (RandomGenerator.ShouldSend(sampleRate))
                {
                    await SendAsync<Timing>(statName, (long)stopwatch.Elapsed.TotalMilliseconds);
                }
            }
        }
#endif
    }
}

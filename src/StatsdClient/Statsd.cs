using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace StatsdClient
{
    public interface IAllowsSampleRate { }

    public interface IAllowsDouble { }
    public interface IAllowsInteger { }

    public class Statsd : IStatsd
    {
        private IStopWatchFactory StopwatchFactory { get; set; }
        private IStatsdUDP Udp { get; set; }
        private IRandomGenerator RandomGenerator { get; set; }

        private readonly string _prefix;

        public List<string> Commands { get; private set; }

        public class Counting : IAllowsSampleRate, IAllowsInteger { }
        public class Timing : IAllowsSampleRate, IAllowsInteger { }
        public class Gauge : IAllowsDouble { }
        public class Histogram : IAllowsInteger { }
        public class Meter : IAllowsInteger { }

        private readonly Dictionary<Type, string> _commandToUnit = new Dictionary<Type, string>
                                                                       {
                                                                           {typeof (Counting), "c"},
                                                                           {typeof (Timing), "ms"},
                                                                           {typeof (Gauge), "g"},
                                                                           {typeof (Histogram), "h"},
                                                                           {typeof (Meter), "m"}
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

        public void Add<TCommandType>(string name, int value) where TCommandType : IAllowsInteger
        {
            Commands.Add(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            Commands.Add(GetCommand(name, String.Format(CultureInfo.InvariantCulture,"{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1));
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

        public void Send()
        {
            try
            {
                Udp.Send(string.Join("\n", Commands.ToArray()));
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

        public void Add(Action actionToTime, string statName)
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
				Add<Timing>(statName, stopwatch.ElapsedMilliseconds());
	        }
        }

        public void Send(Action actionToTime, string statName)
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
		        Send<Timing>(statName, stopwatch.ElapsedMilliseconds());
	        }
        }
    }


}

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatsdClient
{
    public class Statsd : IStatsd
    {
        private IStopWatchFactory StopwatchFactory { get; set; }
        private IStatsdUDP Udp { get; set; }
        private IRandomGenerator RandomGenerator { get; set; }

        private readonly string _prefix;

        public List<string> Commands
        {
            get { return _commands; }
            private set { _commands = value; }
        }

        private List<string> _commands = new List<string>();

        public class Counting : ICommandType { }
        public class Timing : ICommandType { }
        public class Gauge : ICommandType { }
        public class Histogram : ICommandType { }
        public class Meter : ICommandType { }
        public class Set : ICommandType { }

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

        public void Add<TCommandType>(string name, int value, params string[] tags) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand<int>(name, value, _commandToUnit[typeof(TCommandType)], 1, tags));
        }

        public void Add<TCommandType>(string name, double value, params string[] tags) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand<double>(name, value, _commandToUnit[typeof(TCommandType)], 1, tags));
        }

        public void Add<TCommandType>(string name, int value, double sampleRate, params string[] tags) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand<int>(name, value, _commandToUnit[typeof(TCommandType)], sampleRate, tags));
        }

        public void Add<TCommandType>(string name, double value, double sampleRate, params string[] tags) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand<double>(name, value, _commandToUnit[typeof(TCommandType)], sampleRate, tags));
        }

        public void Send<TCommandType>(string name, int value, params string[] tags) where TCommandType : ICommandType
        {
            Send<TCommandType>(name, value, 1, tags);
        }

        public void Send<TCommandType>(string name, double value, params string[] tags) where TCommandType : ICommandType
        {
            Send<TCommandType>(name, value, 1, tags);
        }

        public void Send<TCommandType>(string name, int value, double sampleRate, params string[] tags) where TCommandType : ICommandType
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(GetCommand<int>(name, value, _commandToUnit[typeof(TCommandType)], sampleRate, tags));
            }
        }

        public void Send<TCommandType>(string name, double value, double sampleRate, params string[] tags) where TCommandType : ICommandType
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(GetCommand<double>(name, value, _commandToUnit[typeof(TCommandType)], sampleRate, tags));
            }
        }

        public void Send(string command)
        {
            Commands = new List<string> { command };
            Send();
        }

        public void Send()
        {
            try
            {
                Udp.Send(string.Join(Environment.NewLine, Commands.ToArray()));
                Commands = new List<string>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private string GetCommand<T>(string name, T value, string unit, double sampleRate, params string[] tags)
        {
            // It would be cleaner to do this with StringBuilder, but we want sending stats to be as fast as possible
            if (sampleRate == 1 && tags.Length == 0)
                return string.Format ("{0}:{1}|{2}", _prefix + name, value, unit);
            else if (sampleRate == 1 && tags.Length > 0) 
                return string.Format("{0}:{1}|{2}|#{3}", _prefix + name, value, unit, string.Join(",", tags));
            else if (sampleRate != 1 && tags.Length == 0)
                return string.Format("{0}:{1}|{2}|@{3}", _prefix + name, value, unit, sampleRate);
            else // { if (sampleRate != 1 && tags.Length > 0) }
                return string.Format("{0}:{1}|{2}|@{3}|#{4}", _prefix + name, value, unit, sampleRate, 
                                     string.Join (",", tags));
        }

        public void Add(Action actionToTime, string statName, params string[] tags)
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
				Add<Timing>(statName, stopwatch.ElapsedMilliseconds(), tags);
	        }
        }

        public void Send(Action actionToTime, string statName, params string[] tags)
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
		        Send<Timing>(statName, stopwatch.ElapsedMilliseconds(), tags);
	        }
        }
    }
}

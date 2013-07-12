﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

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


        public void Send<TCommandType>(string name, int value) where TCommandType : ICommandType
        {
            Send<TCommandType>(name, value, 1);
        }

        public void Send(string name, int value, double sampleRate)
        {
            Send<Counting>(name, value, sampleRate);
        }

        public void Add<TCommandType>(string name, int value) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand(name, value, _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(GetCommand(name, value, _commandToUnit[typeof(TCommandType)], sampleRate));
            }
        }

        public void Add(string name, int value, double sampleRate)
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                _commands.Add(GetCommand(name, value, _commandToUnit[typeof(Counting)], sampleRate));
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
                Udp.Send(string.Join("\n", Commands.ToArray()));
                Commands = new List<string>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private string GetCommand(string name, int value, string unit, double sampleRate)
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

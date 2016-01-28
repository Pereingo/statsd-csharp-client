using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial class Statsd : IStatsd
    {
        public void Send<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
            Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1) };
            Send();
        }

        public void Add<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
            ThreadSafeAddCommand(GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], 1));
        }

        public void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            Commands = new List<string> { GetCommand(name, String.Format(CultureInfo.InvariantCulture, "{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1) };
            Send();
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
            ThreadSafeAddCommand(GetCommand(name, String.Format(CultureInfo.InvariantCulture, "{0:F15}", value), _commandToUnit[typeof(TCommandType)], 1));
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

        public void Send<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Commands = new List<string> { GetCommand(name, value.ToString(CultureInfo.InvariantCulture), _commandToUnit[typeof(TCommandType)], sampleRate) };
                Send();
            }
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
            Commands = new List<string> { GetCommand(name, value, _commandToUnit[typeof(TCommandType)], 1) };
            Send();
        }

        public void Send()
        {
            try
            {
                Udp.Send(string.Join("\n", Commands.ToArray()));
                Commands = new List<string>();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void Add(Action actionToTime, string statName, double sampleRate = 1)
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
                    Add<Timing>(statName, (long)stopwatch.Elapsed.TotalMilliseconds);
                }
            }
        }

        public void Send(Action actionToTime, string statName, double sampleRate = 1)
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
                    Send<Timing>(statName, (long)stopwatch.Elapsed.TotalMilliseconds);
                }
            }
        }

        public void Add<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
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

        private string GetCommand(string name, string value, string unit, double sampleRate)
        {
            var format = sampleRate == 1 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";
            return string.Format(CultureInfo.InvariantCulture, format, _prefix + name, value, unit, sampleRate);
        }
    }
}

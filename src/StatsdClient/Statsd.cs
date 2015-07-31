using StatsdClient.MetricTypes;
using StatsdClient.Senders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace StatsdClient
{
    public class Statsd : IStatsd
    {
        private readonly Configuration _config = null;

        public Statsd(Configuration config)
        {
            if (config.Udp == null)
                throw new ArgumentNullException("Configuration.Udp");

            if (config.Sender == null)
                config.Sender = new ThreadSafeConsumerProducerSender();
            config.Sender.StatsdUDP = config.Udp;

            if(config.RandomGenerator == null)
                config.RandomGenerator = new RandomGenerator();
            if(config.StopwatchFactory == null)
                config.StopwatchFactory = new StopWatchFactory();
            if (config.Prefix == null)
                config.Prefix = string.Empty;
            else
                config.Prefix = config.Prefix.TrimEnd('.');
            _config = config;
        }
        
        public void Send<TCommandType>(string name, int value) where TCommandType : Metric, IAllowsInteger, new()
        {
            _config.Sender.Send(new TCommandType() { Name = BuildNamespacedStatName(name), ValueAsInt = value });
        }

        public void Send<TCommandType>(string name, double value) where TCommandType : Metric, IAllowsDouble, new()
        {
            _config.Sender.Send(new TCommandType() { Name = BuildNamespacedStatName(name), ValueAsDouble = value });
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : Metric, IAllowsString, new()
        {
            _config.Sender.Send(new TCommandType() { Name = BuildNamespacedStatName(name), Value = value });
        }

        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : Metric, IAllowsInteger, IAllowsSampleRate, new()
        {
            if (_config.RandomGenerator.ShouldSend(sampleRate))
            {
                _config.Sender.Send(new TCommandType() { Name = BuildNamespacedStatName(name), ValueAsInt = value, SampleRate = sampleRate });
            }
        }

        public void Send(Action actionToTime, string statName, double sampleRate = 1)
        {
            var stopwatch = _config.StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                if (_config.RandomGenerator.ShouldSend(sampleRate))
                {
                    Send<Timing>(statName, stopwatch.ElapsedMilliseconds());
                }
            }
        }

        private string BuildNamespacedStatName(string statName)
        {
            return ((string.IsNullOrEmpty(_config.Prefix)) ? statName : String.Concat(_config.Prefix, ".", statName));
        }

        public class Configuration
        {
            public IStopWatchFactory StopwatchFactory { get; set; }
            public IStatsdUDP Udp { get; set; }
            public IRandomGenerator RandomGenerator { get; set; }
            public ISender Sender { get; set; }
            public string Prefix { get; set; }
        }

        #region Backward Compatibility
        public class Counting : MetricTypes.Counting { }
        public class Gauge : MetricTypes.Gauge { }
        public class Histogram : MetricTypes.Histogram { }
        public class Meter : MetricTypes.Meter { }
        public class Set : MetricTypes.Set { }
        public class Timing : MetricTypes.Timing { }
        #endregion

    }
}

using System;

namespace StatsdClient
{
    public static class Metrics
    {
        private static IStatsd _statsD = new NullStatsd();
        private static StatsdUDP _statsdUdp;
        private static string _prefix;

        public static void Configure(MetricsConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _prefix = config.Prefix ?? "";
            _prefix = _prefix.TrimEnd('.');
            CreateStatsD(config);
        }

        private static void CreateStatsD(MetricsConfig config)
        {
            if (_statsdUdp != null)
            {
                _statsdUdp.Dispose();
            }

            _statsdUdp = null;

            if (!string.IsNullOrEmpty(config.StatsdServerName))
            {
                _statsdUdp = new StatsdUDP(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize);
                _statsD = new Statsd(_statsdUdp);
            }
        }

        public static void Counter(string statName, int value = 1, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        public static void Gauge(string statName, double value)
        {
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
        }

        public static void Timer(string statName, int value, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        public static IDisposable StartTimer(string name)
        {
            return new MetricsTimer(name);
        }

        public static void Time(Action action, string statName, double sampleRate=1) 
        {
            _statsD.Send(action, BuildNamespacedStatName(statName), sampleRate);
        }

        public static T Time<T>(Func<T> func, string statName)
        {
            using (StartTimer(statName))
            {
                return func();
            }
        }

        public static void Set(string statName, string value)
        {
            _statsD.Send<Statsd.Set>(BuildNamespacedStatName(statName), value);
        }

        private static string BuildNamespacedStatName(string statName)
        {
            if (string.IsNullOrEmpty(_prefix))
            {
                return statName;
            }

            return _prefix + "." + statName;
        }

	   public static bool IsConfigured()
	   {
		   return _statsD != null && !(_statsD is NullStatsd);
	   }
    }
}
using System;

namespace StatsdClient
{
	public static class Metrics
	{
	 	private static Statsd _statsD;
	 	private static string _prefix;

		public static void Configure(MetricsConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			_prefix = config.Prefix ?? "";
			_prefix = _prefix.TrimEnd('.');

			_statsD = string.IsNullOrEmpty(config.StatsdServerName)
				          ? null
				          : new Statsd(new StatsdUDP(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize));
		}

        public static void Counter(string statName, int value = 1, double sampleRate = 1)	
		{
			if (_statsD == null)
			{
				return;
			}

            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
		}

		public static void Gauge(string statName, double value)
		{
			if (_statsD == null)
			{
				return;
			}

			_statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
		}

		public static void Timer(string statName, int value, double sampleRate = 1)
		{
			if (_statsD == null)
			{
				return;
			}

			_statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
		}

		public static IDisposable StartTimer(string name)
		{
			return new MetricsTimer(name);
		}

		public static void Time(Action action, string statName, double sampleRate=1) 
		{
			if (_statsD == null)
			{
				action();
				return;
			}

			_statsD.Send(action, BuildNamespacedStatName(statName), sampleRate);
		}

		public static T Time<T>(Func<T> func, string statName)
		{
			if (_statsD == null)
			{
				return func();
			}

			using (StartTimer(statName))
			{
				return func();
			}
		}

		public static void Set(string statName, string value)
		{
			if (_statsD == null)
			{
				return;
			}

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
	}
}

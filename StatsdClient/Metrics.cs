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

			if (! string.IsNullOrEmpty(config.StatsdServerName))
			{
				_prefix = config.Prefix;
				_statsD = new Statsd(new StatsdUDP(config.StatsdServerName, 8125));
			}
		}

		public static void Counter(string statName, int value = 1)	
		{
			if (_statsD == null)
			{
				return;
			}
			_statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value);
		}

		public static void Gauge(string statName, int value)
		{
			if (_statsD == null)
			{
				return;
			}
			_statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
		}

		public static void Timer(string statName, int value)
		{
			if (_statsD == null)
			{
				return;
			}

			_statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value);
		}

		public static string BuildNamespacedStatName(string statName)
		{
			return _prefix + "." + statName;
		}

		public static IDisposable StartTimer(string name)
		{
			return new MetricsTimer(name);
		}
	}
}

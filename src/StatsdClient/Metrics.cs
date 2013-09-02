﻿using System;

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

			_prefix = config.Prefix;
			_statsD = string.IsNullOrEmpty(config.StatsdServerName)
				          ? null
				          : new Statsd(new StatsdUDP(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize));
		}

        public static void Counter(string statName, long value = 1, double sampleRate = 1)	
		{
			if (_statsD == null)
			{
				return;
			}

            _statsD.Send(BuildNamespacedStatName(statName), value, sampleRate);
		}

		public static void Gauge(string statName, long value)
		{
			if (_statsD == null)
			{
				return;
			}

			_statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
		}

		public static void Timer(string statName, long value)
		{
			if (_statsD == null)
			{
				return;
			}

			_statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value);
		}

		public static IDisposable StartTimer(string name)
		{
			return new MetricsTimer(name);
		}

		public static void Time(Action action, string statName) 
		{
			if (_statsD == null)
			{
				action();
				return;
			}

			_statsD.Send(action, BuildNamespacedStatName(statName));
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

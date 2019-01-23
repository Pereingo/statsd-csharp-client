﻿using System;
using System.Threading.Tasks;

namespace StatsdClient
{
    public static class Metrics
    {
        private static IStatsd _statsD = new NullStatsd();
        private static IStatsdClient _statsdClient;
        private static string _prefix;

        /// <summary>
        /// Configures the Metric class with a configuration. Call this once at application startup (Main(), Global.asax, etc).
        /// </summary>
        /// <param name="config">Configuration settings.</param>
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
            _statsdClient?.Dispose();

            _statsdClient = null;

            if (!string.IsNullOrEmpty(config.StatsdServerName))
            {
                _statsdClient = new StatsdUDPClient(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize);
                _statsD = new Statsd(_statsdClient);
            }
        }

        /// <summary>
        /// Send a counter value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value of the counter. Defaults to 1.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Counter(string statName, int value = 1, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Send a counter value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value of the counter. Defaults to 1.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static Task CounterAsync(string statName, int value = 1, double sampleRate = 1)
        {
            return _statsD.SendAsync<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Modify the current value of the gauge with the given value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="deltaValue"></param>
        public static void GaugeDelta(string statName, double deltaValue)
        {
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), deltaValue, true);
        }

        /// <summary>
        /// Modify the current value of the gauge with the given value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="deltaValue"></param>
        public static Task GaugeDeltaAsync(string statName, double deltaValue)
        {
            return _statsD.SendAsync<Statsd.Gauge>(BuildNamespacedStatName(statName), deltaValue, true);
        }

        /// <summary>
        /// Set the gauge to the given absolute value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="absoluteValue">Absolute value of the gauge to set.</param>
        public static void GaugeAbsoluteValue(string statName, double absoluteValue)
        {
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), absoluteValue, false);
        }

        /// <summary>
        /// Set the gauge to the given absolute value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="absoluteValue">Absolute value of the gauge to set.</param>
        public static Task GaugeAbsoluteValueAsync(string statName, double absoluteValue)
        {
            return _statsD.SendAsync<Statsd.Gauge>(BuildNamespacedStatName(statName), absoluteValue, false);
        }

        /// <summary>
        /// Send a manually timed value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Elapsed miliseconds of the event.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Timer(string statName, int value, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Send a manually timed value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Elapsed miliseconds of the event.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static Task TimerAsync(string statName, int value, double sampleRate = 1)
        {
            return _statsD.SendAsync<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a using block) and send the elapsed miliseconds
        /// </summary>
        /// <param name="name">Name of the metric.</param>
        /// <returns>A disposable object that will record & send the metric.</returns>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static IDisposable StartTimer(string name, double sampleRate = 1)
        {
            return new MetricsTimer(name, sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a lambda) and send the elapsed miliseconds.
        /// </summary>
        /// <param name="action">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Time(Action action, string statName, double sampleRate = 1)
        {
            _statsD.Send(action, BuildNamespacedStatName(statName), sampleRate);
        }

        /// <summary>
        /// Time a given piece of async code and send the elapsed miliseconds.
        /// </summary>
        /// <param name="func">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static async Task Time(Func<Task> func, string statName, double sampleRate = 1)
        {
            using (StartTimer(statName, sampleRate))
            {
                await func().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Time a given piece of code and send the elapsed miliseconds.
        /// </summary>
        /// <param name="func">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        /// <returns>Return value of the function.</returns>
        public static T Time<T>(Func<T> func, string statName, double sampleRate = 1)
        {
            using (StartTimer(statName, sampleRate))
            {
                return func();
            }
        }

        /// <summary>
        /// Time a given piece of async code and send the elapsed miliseconds.
        /// </summary>
        /// <param name="func">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        /// <returns>Return value of the function.</returns>
        public static async Task<T> Time<T>(Func<Task<T>> func, string statName, double sampleRate = 1)
        {
            using (StartTimer(statName, sampleRate))
            {
                return await func().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Store a unique occurence of an event between flushes.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value to set.</param>
        public static void Set(string statName, string value)
        {
            _statsD.Send<Statsd.Set>(BuildNamespacedStatName(statName), value);
        }

        /// <summary>
        /// Store a unique occurence of an event between flushes.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value to set.</param>
        public static Task SetAsync(string statName, string value)
        {
            return _statsD.SendAsync<Statsd.Set>(BuildNamespacedStatName(statName), value);
        }

        private static string BuildNamespacedStatName(string statName)
        {
            if (string.IsNullOrEmpty(_prefix))
            {
                return statName;
            }

            return _prefix + "." + statName;
        }

        /// <summary>
        /// Determine if the Metrics instance has been configured previously.
        /// </summary>
        public static bool IsConfigured()
        {
            return _statsD != null && !(_statsD is NullStatsd);
        }
    }
}

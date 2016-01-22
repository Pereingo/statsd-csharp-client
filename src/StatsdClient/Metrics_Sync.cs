using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatsdClient
{
    public static partial class Metrics
    {
#if NET451
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
            _statsdUdp?.Dispose();

            _statsdUdp = null;

            if (!string.IsNullOrEmpty(config.StatsdServerName))
            {
                _statsdUdp = new StatsdUDP(config.StatsdServerName,
                    config.StatsdServerPort,
                    config.StatsdMaxUDPPacketSize);
                _statsD = new Statsd(_statsdUdp);
            }
        }
#endif

        /// <summary>
        /// Send a counter value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value of the counter. Defaults to 1.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Counter(string statName, long value = 1, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
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
        /// Set the gauge to the given absolute value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="absoluteValue">Absolute value of the gauge to set.</param>
        public static void GaugeAbsoluteValue(string statName, double absoluteValue)
        {
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), absoluteValue, false);
        }

        [Obsolete("Will be removed in future version. Use explicit GaugeDelta or GaugeAbsoluteValue instead.")]
        public static void Gauge(string statName, double value)
        {
            GaugeAbsoluteValue(statName, value);
        }

        /// <summary>
        /// Send a manually timed value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Elapsed miliseconds of the event.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Timer(string statName, long value, double sampleRate = 1)
        {
            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        public static T Time<T>(Func<T> func, string statName)
        {
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
    }
}

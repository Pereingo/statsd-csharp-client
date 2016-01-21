using System;
using System.Threading.Tasks;

namespace StatsdClient
{
    public static class Metrics
    {
        private static IStatsd _statsD = new NullStatsd();
        private static StatsdUDP _statsdUdp;
        private static string _prefix;

        /// <summary>
        /// Configures the Metric class with a configuration. Call this once at application startup (Main(), Global.asax, etc).
        /// </summary>
        /// <param name="config">Configuration settings.</param>
        public static async Task ConfigureAsync(MetricsConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _prefix = config.Prefix ?? "";
            _prefix = _prefix.TrimEnd('.');
            await CreateStatsDAsync(config);
        }

        private static async Task CreateStatsDAsync(MetricsConfig config)
        {
            _statsdUdp?.Dispose();

            _statsdUdp = null;

            if (!string.IsNullOrEmpty(config.StatsdServerName))
            {
                _statsdUdp = new StatsdUDP(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize);
                await _statsdUdp.InitializeAsync();
                _statsD = new Statsd(_statsdUdp);
            }
        }

        /// <summary>
        /// Send a counter value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value of the counter. Defaults to 1.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static async Task CounterAsync(string statName, long value = 1, double sampleRate = 1)
        {
            await _statsD.SendAsync<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Modify the current value of the gauge with the given value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="deltaValue"></param>
        public static async Task GaugeDeltaAsync(string statName, double deltaValue)
        {
            await _statsD.SendAsync<Statsd.Gauge>(BuildNamespacedStatName(statName), deltaValue, true);
        }

        /// <summary>
        /// Set the gauge to the given absolute value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="absoluteValue">Absolute value of the gauge to set.</param>
        public static async Task GaugeAbsoluteValueAsync(string statName, double absoluteValue)
        {
            await _statsD.SendAsync<Statsd.Gauge>(BuildNamespacedStatName(statName), absoluteValue, false);
        }

        [Obsolete("Will be removed in future version. Use explicit GaugeDelta or GaugeAbsoluteValue instead.")]
        public static async Task GaugeAsync(string statName, double value)
        {
            await GaugeAbsoluteValueAsync(statName, value);
        }

        /// <summary>
        /// Send a manually timed value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Elapsed miliseconds of the event.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static async Task TimerAsync(string statName, long value, double sampleRate = 1)
        {
            await _statsD.SendAsync<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate);
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
        public static async Task TimeAsync(Action action, string statName, double sampleRate = 1)
        {
            await _statsD.SendAsync(action, BuildNamespacedStatName(statName), sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a lambda) and send the elapsed miliseconds.
        /// </summary>
        /// <param name="func">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <returns>Return value of the function.</returns>
        public static T Time<T>(Func<T> func, string statName)
        {
            using (StartTimer(statName))
            {
                return func();
            }
        }

        /// <summary>
        /// Store a unique occurence of an event between flushes.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value to set.</param>
        public static async Task SetAsync(string statName, string value)
        {
            await _statsD.SendAsync<Statsd.Set>(BuildNamespacedStatName(statName), value);
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
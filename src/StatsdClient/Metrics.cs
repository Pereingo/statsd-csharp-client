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
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(config.StatsdServerName))
                throw new ArgumentNullException("config.StatsdServername");

            _prefix = config.Prefix;

            int port;
            if (config.StatsdPort > 0)
                port = config.StatsdPort;
            else
                port = 8125;
    
            _statsD = new Statsd(new StatsdUDP(config.StatsdServerName, port));
        }

        public static void Counter(string statName, int value = 1)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value);
        }

        public static void Counter(string statName, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1, tags);
        }

        public static void Counter(string statName, int value, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Counter(string statName, double sampleRate, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1, sampleRate, tags);
        }

        public static void Counter(string statName, int value, double sampleRate, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Increment(string statName)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1);
        }

        public static void Increment(string statName, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1, tags);
        }

        public static void Increment(string statName, double sampleRate)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1, sampleRate);
        }

        public static void Increment(string statName, double sampleRate, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), 1, sampleRate, tags);
        }

        public static void Decrement(string statName)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), -1);
        }

        public static void Decrement(string statName, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), -1, tags);
        }

        public static void Decrement(string statName, double sampleRate)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), -1, sampleRate);
        }

        public static void Decrement(string statName, double sampleRate, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting>(BuildNamespacedStatName(statName), -1, sampleRate, tags);
        }

        public static void Gauge(string statName, int value)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
        }

        public static void Gauge(string statName, int value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Gauge(string statName, int value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Gauge(string statName, double value)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value);
        }

        public static void Gauge(string statName, double value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Gauge(string statName, double value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Histogram(string statName, int value)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value);
        }

        public static void Histogram(string statName, int value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Histogram(string statName, int value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Histogram(string statName, double value)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value);
        }

        public static void Histogram(string statName, double value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Histogram(string statName, double value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Histogram>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Set(string statName, int value)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Set>(BuildNamespacedStatName(statName), value);
        }

        public static void Set(string statName, int value, params string[] tags)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Set>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Set(string statName, int value, double sampleRate, params string[] tags)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Set>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Timer(string statName, int value)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value);
        }

        public static void Timer(string statName, int value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Timer(string statName, int value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static void Timer(string statName, double value)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value);
        }

        public static void Timer(string statName, double value, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, tags);
        }

        public static void Timer(string statName, double value, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing>(BuildNamespacedStatName(statName), value, sampleRate, tags);
        }

        public static IDisposable StartTimer(string name)
        {
            return new MetricsTimer(name);
        }

        public static IDisposable StartTimer(string name, params string[] tags)
        {
            return new MetricsTimer(name, tags);
        }

        public static IDisposable StartTimer(string name, double sampleRate, params string[] tags)
        {
            return new MetricsTimer(name, sampleRate, tags);
        }

        public static void Time(Action action, string statName, params string[] tags) 
        {
            if (_statsD == null)
            {
                action();
            }
            else
            {
                _statsD.Send(action, BuildNamespacedStatName(statName), tags);
            }
        }

        public static void Time(Action action, string statName, double sampleRate, params string[] tags) 
        {
            if (_statsD == null)
            {
                action();
            }
            else
            {
                _statsD.Send(action, BuildNamespacedStatName(statName), sampleRate, tags);
            }
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

        public static T Time<T>(Func<T> func, string statName, params string[] tags)
        {
            if (_statsD == null)
            {
                return func();
            }

            using (StartTimer(statName, tags))
            {
                return func();
            }
        }

        public static T Time<T>(Func<T> func, string statName, double sampleRate, params string[] tags)
        {
            if (_statsD == null)
            {
                return func();
            }

            using (StartTimer(statName, sampleRate, tags))
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

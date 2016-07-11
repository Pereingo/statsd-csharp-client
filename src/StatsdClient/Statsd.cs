using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace StatsdClient
{
    public class Statsd : IStatsd
    {
        private IStopWatchFactory StopwatchFactory { get; set; }
        private IStatsdUDP Udp { get; set; }
        private IRandomGenerator RandomGenerator { get; set; }

        private readonly string _prefix;

        public List<string> Commands
        {
            get { return _commands; }
            private set { _commands = value; }
        }

        private List<string> _commands = new List<string>();

        public abstract class Metric : ICommandType
        {
            private static readonly Dictionary<Type, string> _commandToUnit = new Dictionary<Type, string>
                                                                {
                                                                    {typeof (Counting), "c"},
                                                                    {typeof (Timing), "ms"},
                                                                    {typeof (Gauge), "g"},
                                                                    {typeof (Histogram), "h"},
                                                                    {typeof (Meter), "m"},
                                                                    {typeof (Set), "s"}
                                                                };

            public static string GetCommand<TCommandType, T>(string prefix, string name, T value, double sampleRate, string[] tags) where TCommandType : Metric
            {
                string full_name = prefix + name;
                string unit = _commandToUnit[typeof(TCommandType)];
                // It would be cleaner to do this with StringBuilder, but we want sending stats to be as fast as possible
                if (sampleRate == 1.0 && (tags == null || tags.Length == 0))
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}|{2}", full_name, value, unit);
                else if (sampleRate == 1.0 && (tags == null || tags.Length > 0))
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}|{2}|#{3}", full_name, value, unit, string.Join(",", tags));
                else if (sampleRate != 1.0 && (tags == null || tags.Length == 0))
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}|{2}|@{3}", full_name, value, unit, sampleRate);
                else // { if (sampleRate != 1 && (tags == null || tags.Length > 0)) }
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}|{2}|@{3}|#{4}", full_name, value, unit, sampleRate,
                                         string.Join(",", tags));
            }
        }

        public class Event : ICommandType
        {
            private const int MaxSize = 8 * 1024;

            public static string GetCommand(string title, string text, string alertType, string aggregationKey, string sourceType, int? dateHappened, string priority, string hostname, string[] tags, bool truncateIfTooLong = false)
            {
                string processedTitle = EscapeContent(title);
                string processedText = EscapeContent(text);
                string result = string.Format(CultureInfo.InvariantCulture, "_e{{{0},{1}}}:{2}|{3}", processedTitle.Length.ToString(), processedText.Length.ToString(), processedTitle, processedText);
                if (dateHappened != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|d:{0}", dateHappened);
                }
                if (hostname != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|h:{0}", hostname);
                }
                if (aggregationKey != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|k:{0}", aggregationKey);
                }
                if (priority != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|p:{0}", priority);
                }
                if (sourceType != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|s:{0}", sourceType);
                }
                if (alertType != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|t:{0}", alertType);
                }
                if (tags != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|#{0}", string.Join(",", tags));
                }
                if (result.Length > MaxSize)
                {
                    if (truncateIfTooLong)
                    {
                        var overage = result.Length - MaxSize;
                        if (title.Length > text.Length)
                            title = TruncateOverage(title, overage);
                        else
                            text = TruncateOverage(text, overage);
                        return GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, tags, true);
                    }
                    else
                        throw new Exception(string.Format("Event {0} payload is too big (more than 8kB)", title));
                }
                return result;
            }
        }

        public class ServiceCheck : ICommandType
        {
            private const int MaxSize = 8 * 1024;

            public static string GetCommand(string name, int status, int? timestamp, string hostname, string[] tags, string serviceCheckMessage, bool truncateIfTooLong = false)
            {
                string processedName = EscapeName(name);
                string processedMessage = EscapeMessage(serviceCheckMessage);

                string result = string.Format(CultureInfo.InvariantCulture, "_sc|{0}|{1}", processedName, status);
               
                if (timestamp != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|d:{0}", timestamp);
                }
                if (hostname != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|h:{0}", hostname);
                }
                if (tags != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|#{0}", string.Join(",", tags));
                }
                // Note: this must always be appended to the result last.
                if (processedMessage != null)
                {
                    result += string.Format(CultureInfo.InvariantCulture, "|m:{0}", processedMessage);
                }

                if (result.Length > MaxSize)
                {
                    if (!truncateIfTooLong)
                        throw new Exception(string.Format("ServiceCheck {0} payload is too big (more than 8kB)", name));

                    var overage = result.Length - MaxSize;

                    if (processedMessage == null || overage > processedMessage.Length)
                        throw new ArgumentException(string.Format("ServiceCheck name is too long to truncate, payload is too big (more than 8Kb) for {0}", name), "name");

                    var truncMessage = TruncateOverage(processedMessage, overage);
                    return GetCommand(name, status, timestamp, hostname, tags, truncMessage, true);
                }

                return result;
            }

            // Service check name string, shouldn’t contain any |
            private static string EscapeName(string name)
            {
                name = EscapeContent(name);

                if (name.Contains("|"))
                    throw new ArgumentException("Name must not contain any | (pipe) characters", "name");

                return name;
            }

            private static string EscapeMessage(string message)
            {
                if (!string.IsNullOrEmpty(message))
                    return EscapeContent(message).Replace("m:", "m\\:");
                return message;
            }
        }

        private static string EscapeContent(string content)
        {
            return content
                .Replace("\r", "")
                .Replace("\n", "\\n");
        }

        private static string TruncateOverage(string str, int overage)
        {
            return str.Substring(0, str.Length - overage);
        }

        public class Counting : Metric { }
        public class Timing : Metric { }
        public class Gauge : Metric { }
        public class Histogram : Metric { }
        public class Meter : Metric { }
        public class Set : Metric { }

        public Statsd(IStatsdUDP udp, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory, string prefix)
        {
            StopwatchFactory = stopwatchFactory;
            Udp = udp;
            RandomGenerator = randomGenerator;
            _prefix = prefix;
        }

        public Statsd(IStatsdUDP udp, IRandomGenerator randomGenerator, IStopWatchFactory stopwatchFactory)
            : this(udp, randomGenerator, stopwatchFactory, string.Empty) { }

        public Statsd(IStatsdUDP udp, string prefix)
            : this(udp, new RandomGenerator(), new StopWatchFactory(), prefix) { }

        public Statsd(IStatsdUDP udp)
            : this(udp, "") { }

        public void Add<TCommandType, T>(string name, T value, double sampleRate = 1.0, string[] tags = null) where TCommandType : Metric
        {
            _commands.Add(Metric.GetCommand<TCommandType, T>(_prefix, name, value, sampleRate, tags));
        }

        public void Add(string title, string text, string alertType = null, string aggregationKey = null, string sourceType = null, int? dateHappened = null, string priority = null, string hostname = null, string[] tags = null)
        {
            _commands.Add(Event.GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, tags));
        }

        public void Send(string title, string text, string alertType = null, string aggregationKey = null, string sourceType = null, int? dateHappened = null, string priority = null, string hostname = null, string[] tags = null, bool truncateIfTooLong = false)
        {
            Send(Event.GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, tags, truncateIfTooLong));
        }

        /// <summary>
        /// Add a Service check
        /// </summary>
        public void Add(string name, int status, int? timestamp = null, string hostname = null, string[] tags = null, string serviceCheckMessage = null)
        {
            _commands.Add(ServiceCheck.GetCommand(name, status, timestamp, hostname, tags, serviceCheckMessage));
        }

        /// <summary>
        /// Send a service check
        /// </summary>
        public void Send(string name, int status, int? timestamp = null, string hostname = null, string[] tags = null, string serviceCheckMessage = null, bool truncateIfTooLong = false)
        {
            Send(ServiceCheck.GetCommand(name, status, timestamp, hostname, tags, serviceCheckMessage, truncateIfTooLong));
        }

        public void Send<TCommandType, T>(string name, T value, double sampleRate = 1.0, string[] tags = null) where TCommandType : Metric
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(Metric.GetCommand<TCommandType, T>(_prefix, name, value, sampleRate, tags));
            }
        }

        public void Send(string command)
        {
            try
            {
                Udp.Send(command);
                // clear buffer (keep existing behavior)
                if (Commands.Count > 0)
                    Commands = new List<string>();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void Send()
        {
            int count = Commands.Count;
            if (count < 1) return;

            Send(1 == count ? Commands[0] : string.Join("\n", Commands.ToArray()));
        }

        public void Add(Action actionToTime, string statName, double sampleRate = 1.0, string[] tags = null)
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
                Add<Timing, int>(statName, stopwatch.ElapsedMilliseconds(), sampleRate, tags);
            }
        }

        public void Send(Action actionToTime, string statName, double sampleRate = 1.0, string[] tags = null)
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
                Send<Timing, int>(statName, stopwatch.ElapsedMilliseconds(), sampleRate, tags);
            }
        }
    }
}

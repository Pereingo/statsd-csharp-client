using System;

namespace StatsdClient
{
    public class MetricsTimer : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _stopWatch;
        private bool _disposed;
        private readonly double _sampleRate;
        private readonly string[] _tags; 

        public MetricsTimer(string name, params string[] tags)
        {
            _name = name;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _sampleRate = 1.0;
            _tags = tags;
        }

        public MetricsTimer(string name, double sampleRate, params string[] tags)
        {
            _name = name;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _sampleRate = sampleRate;
            _tags = tags;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopWatch.Stop();
                Metrics.Timer(_name, _stopWatch.ElapsedMilliseconds(), _sampleRate, _tags);
            }
        }
    }
}
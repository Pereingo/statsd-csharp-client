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

        public MetricsTimer(string name, double sampleRate = 1.0, string[] tags = null)
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
                DogStatsd.Timer(_name, _stopWatch.ElapsedMilliseconds(), _sampleRate, _tags);
            }
        }
    }
}
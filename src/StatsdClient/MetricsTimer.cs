using System;

namespace StatsdClient
{
    public class MetricsTimer : IDisposable
    {
        private readonly string _name;
        private readonly IStopwatch _stopWatch;
        private bool _disposed;
        private readonly double _sampleRate;

        public MetricsTimer(string name, double sampleRate)
        {
            _name = name;
            _sampleRate = sampleRate;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopWatch.Stop();
                Metrics.Timer(_name, _stopWatch.Elapsed.Milliseconds, _sampleRate);
            }
        }
    }
}
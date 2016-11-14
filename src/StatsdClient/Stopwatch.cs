using System;

namespace StatsdClient
{
    public interface IStopwatch
    {
        void Start();
        void Stop();
        TimeSpan Elapsed { get; }

        [Obsolete("use Elapsed property")]
        int ElapsedMilliseconds();
    }

    public class Stopwatch : IStopwatch
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public TimeSpan Elapsed
        {
            get {
                return _stopwatch.Elapsed;
            }
        }

        [Obsolete("use Elapsed property")]
        public int ElapsedMilliseconds()
        {
            double milliseconds = Elapsed.TotalMilliseconds;
            if (milliseconds > int.MaxValue)
            {
                throw new InvalidOperationException("Please use new API 'Elapsed' instead of 'ElapsedMilliseconds()', as your value just overflowed for this old API");
            }
            return (int)milliseconds;
        }
    }
}
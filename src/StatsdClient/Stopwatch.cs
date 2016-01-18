using System;

namespace StatsdClient
{
    public interface IStopwatch
    {
        void Start();
        void Stop();
        TimeSpan ElapsedTime { get; }

        [Obsolete("use ElapsedTime property")]
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

        public TimeSpan ElapsedTime
        {
            get {
                var milliseconds = (double)unchecked(_stopwatch.ElapsedMilliseconds);
                return TimeSpan.FromMilliseconds(milliseconds);
            }
        }

        [Obsolete("use ElapsedTime property")]
        public int ElapsedMilliseconds()
        {
            double milliseconds = ElapsedTime.TotalMilliseconds;
            if (milliseconds > int.MaxValue)
            {
                throw new InvalidOperationException("Please use new API 'ElapsedTime' instead of 'ElapsedMilliseconds()', as your value just overflowed for this old API");
            }
            return (int)milliseconds;
        }
    }
}
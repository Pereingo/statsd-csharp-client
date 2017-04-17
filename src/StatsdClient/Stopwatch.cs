namespace StatsdClient
{
    public interface IStopwatch
    {
        void Start();
        void Stop();
        int ElapsedMilliseconds { get; }
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

        public int ElapsedMilliseconds
        {
            get { return (int)_stopwatch.ElapsedMilliseconds; }
        }
    }
}
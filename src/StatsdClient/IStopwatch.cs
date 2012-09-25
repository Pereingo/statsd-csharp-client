namespace StatsdClient
{
    public interface IStopwatch
    {
        void Start();
        void Stop();
        int ElapsedMilliseconds();
    }
}
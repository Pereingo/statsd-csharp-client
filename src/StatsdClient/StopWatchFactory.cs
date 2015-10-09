namespace StatsdClient
{
    public interface IStopWatchFactory
    {
        IStopwatch Get();
    }

    public class StopWatchFactory : IStopWatchFactory
    {
        public IStopwatch Get()
        {
            return new Stopwatch();
        }
    }
}
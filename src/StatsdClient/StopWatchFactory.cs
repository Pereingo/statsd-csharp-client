namespace StatsdClient
{
    public class StopWatchFactory : IStopWatchFactory
    {
        public IStopwatch Get()
        {
            return new Stopwatch();            
        }
    }
}
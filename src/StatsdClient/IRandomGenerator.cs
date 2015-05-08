namespace StatsdClient
{
    public interface IRandomGenerator
    {
        bool ShouldSend(double sampleRate);
    }
}
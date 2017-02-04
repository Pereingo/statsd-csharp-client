using System;

namespace StatsdClient
{
    public interface IRandomGenerator
    {
        bool ShouldSend(double sampleRate);
    }

    public class RandomGenerator : IRandomGenerator
    {
        [ThreadStatic]
        static Random _random;

        private static Random Random
        {
            get
            {
                if (_random != null) return _random;
                return _random = new Random(Guid.NewGuid().GetHashCode());
            }
        }

        public bool ShouldSend(double sampleRate)
        {
            return Random.NextDouble() < sampleRate;
        }
    }
}
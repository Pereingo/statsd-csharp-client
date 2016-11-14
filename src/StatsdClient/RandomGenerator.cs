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
                var random = _random;
                if (random != null) return random;
                return _random = new Random(Guid.NewGuid().GetHashCode());
            }
        }

        public bool ShouldSend(double sampleRate)
        {
            return Random.NextDouble() < sampleRate;
        }
    }
}
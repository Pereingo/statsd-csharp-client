using System;

namespace StatsdClient
{
    public class RandomGenerator : IRandomGenerator
    {
        readonly Random _random;
        public RandomGenerator()
        {
            _random = new Random();
        }

        public bool ShouldSend(double sampleRate)
        {
            return _random.NextDouble() < sampleRate;
        }
    }
}
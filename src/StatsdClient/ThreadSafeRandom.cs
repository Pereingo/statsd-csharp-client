using System;

namespace StatsdClient
{
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();

        [ThreadStatic]
        private static Random _local;

        private Random Local
        {
            get
            {
                if (_local == null)
                {
                    int seed;
                    lock (_global)
                    {
                        seed = _global.Next();
                    }
                    _local = new Random(seed);
                }
                return _local;
            }
        }

        public double NextDouble()
        {
            return Local.NextDouble();
        }
    }
}

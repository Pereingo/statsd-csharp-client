using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Timing : Metric, IAllowsInteger, IAllowsSampleRate
    {
        public Timing(string name, double sampleRate = 1) : base(name, "ms", sampleRate)
        {
        }

        public Timing() : this(string.Empty) { }

    }
}

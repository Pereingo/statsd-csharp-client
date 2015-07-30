using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Histogram : Metric, IAllowsInteger
    {
        public Histogram(string name) : base(name, "h")
        {
        }

        public Histogram() : this(string.Empty) { }

    }
}

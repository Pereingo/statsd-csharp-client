using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Meter : Metric, IAllowsInteger
    {
        public Meter(string name) : base(name, "m")
        {
        }

        public Meter() : this(string.Empty) { }

    }
}

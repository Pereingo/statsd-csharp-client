using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Set : Metric, IAllowsString
    {
        public Set(string name) : base(name, "s")
        {
        }

        public Set() : this(string.Empty) { }

    }
}

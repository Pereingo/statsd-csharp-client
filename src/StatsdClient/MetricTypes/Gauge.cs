using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Gauge : Metric, IAllowsDouble, IAllowsAggregate
    {
        public Gauge(string name) : base(name, "g")
        {
        }

        public Gauge() : this(string.Empty) { }

        public override void Aggregate(Metric otherMetric)
        {
            this.ValueAsDouble = otherMetric.ValueAsDouble;
        }
    }
}

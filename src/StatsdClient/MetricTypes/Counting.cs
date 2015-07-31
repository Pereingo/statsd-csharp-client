using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public class Counting : Metric, IAllowsInteger, IAllowsSampleRate, IAllowsAggregate
    {
        public Counting(string name, double sampleRate = 1) : base(name, "c", sampleRate)
        {
        }

        public Counting() : this(string.Empty) { }

        public override void Aggregate(Metric otherMetric)
        {
            this.ValueAsInt += otherMetric.ValueAsInt;
        }
    }
}

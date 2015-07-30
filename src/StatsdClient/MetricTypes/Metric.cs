using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StatsdClient.MetricTypes
{
    public interface IAllowsSampleRate { }
    public interface IAllowsDouble { }
    public interface IAllowsInteger { }
    public interface IAllowsString { }
    public interface IAllowsAggregate { }

    public abstract class Metric
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public double SampleRate { get; set; }
        public string UnitType { get; protected set; }

        public Metric(string name, string unitType, double sampleRate = 1)
        {
            this.Name = name;
            this.UnitType = unitType;
            this.SampleRate = sampleRate;
        }

        public virtual string Command
        {
            get
            {
                var format = this.SampleRate == 1 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";
                return string.Format(CultureInfo.InvariantCulture, format, this.Name, this.Value, this.UnitType, this.SampleRate);
            }
        }

        public int ValueAsInt
        {
            get
            {
                int rv = 0;
                int.TryParse(this.Value, out rv);
                return rv;
            }
            set
            {
                this.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public double ValueAsDouble
        {
            get
            {
                double rv = 0;
                double.TryParse(this.Value, out rv);
                return rv;
            }
            set
            {
                this.Value = String.Format(CultureInfo.InvariantCulture, "{0:F15}", value);
            }
        }

        public virtual void Aggregate(Metric otherMetric)
        {
            throw new NotImplementedException();
        }
    }
}

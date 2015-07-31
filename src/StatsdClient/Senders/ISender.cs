using StatsdClient.MetricTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.Senders
{
    public interface ISender
    {
        IStatsdUDP StatsdUDP { get; set; }
        void Send(Metric metric);
    }
}

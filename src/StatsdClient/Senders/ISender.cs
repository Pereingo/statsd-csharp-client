using StatsdClient.MetricTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatsdClient.Senders
{
    public interface ISender
    {
        void Send(Metric metric);
    }
}

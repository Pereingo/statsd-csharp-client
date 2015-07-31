using StatsdClient.MetricTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace StatsdClient.Senders
{
    public class ImmediateSender : ISender
    {
        public IStatsdUDP StatsdUDP { get; set; }

        public ImmediateSender()
        {
        }

        public void Send(Metric metric)
        {
            try
            {
                var data = string.Join("\n", metric.Command);
                if(StatsdUDP != null)
                    StatsdUDP.Send(data);
            }
            catch(System.Exception ex)
            {
                Trace.TraceError("StatsdClient::ImmediateSender - Error: {0}", ex.ToString());
            }
        }
    }
}

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
    // Primarily here for unit testing of the StatsdUDP class.
    // Will not send out messages until Flush() is called.

    public class BatchSender : ISender
    {
        public IStatsdUDP StatsdUDP { get; set; }
        
        private List<Metric> _metrics = new List<Metric>();

        public BatchSender()
        {
        }

        public void Send(Metric metric)
        {
            if(metric != null)
            {
                lock (_metrics)
                    _metrics.Add(metric);
            }
        }

        public void Flush()
        {
            try
            { 
                string[] allCommands;
                lock(_metrics)
                {
                    allCommands = _metrics.Select(x => x.Command).ToArray();
                    _metrics.Clear();
                }

                if (allCommands != null && allCommands.Length != 0 && StatsdUDP != null)
                {
                    var data = string.Join("\n", allCommands);
                    StatsdUDP.Send(data);
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("StatsdClient::BatchSender - Error: {0}", ex.ToString());
            }
        }
    }
}

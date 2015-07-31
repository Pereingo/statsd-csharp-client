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
    public class MockSender : ISender
    {
        // Not used
        public IStatsdUDP StatsdUDP { get; set; }

        public MockSender() 
        {
        }

        public void Send(Metric metric)
        {
            try
            {
                var data = string.Join("\n", metric.Command);
                Debug.WriteLine(string.Format("MockSender::{0}", data));
            }
            catch(System.Exception ex)
            {
                Trace.TraceError("StatsdClient::MockSender - Error: {0}", ex.ToString());
            }
        }
    }
}

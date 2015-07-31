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
    public class ThreadSafeConsumerProducerSender : ISender, IDisposable
    {
        private BlockingCollection<Metric> _queue = new BlockingCollection<Metric>();
        private CancellationTokenSource _cancelSource = null;
        private List<Thread> _sendWorkerThreads = new List<Thread>();
        private readonly Configuration _config = null;
        private object _lock = new object();

        public ThreadSafeConsumerProducerSender() : this(new Configuration())
        {
        }

        public ThreadSafeConsumerProducerSender(Configuration config)
        {
            _config = config;
        }

        private IStatsdUDP _statsdUDP;
        public IStatsdUDP StatsdUDP
        {
            get { return _statsdUDP; }
            set
            {
                lock (_lock)
                {
                    if (_statsdUDP != null)
                        Stop();
                    _statsdUDP = value;
                    if (_statsdUDP != null)
                        Start();
                }
            }
        }

        private void Start()
        {
            _cancelSource = new CancellationTokenSource();
            for (var i = 0; i < _config.MaxThreads; i++)
            {
                var thread = new Thread(RunWorkerThread);
                thread.IsBackground = true;
                _sendWorkerThreads.Add(thread);
                thread.Start();
            }
        }

        private void Stop()
        {
            _cancelSource.Cancel();
            for (var i = 0; i < _sendWorkerThreads.Count; i++)
            {
                _sendWorkerThreads[i].Join(1000);
                _sendWorkerThreads[i] = null;
            }
            _sendWorkerThreads.Clear();
            _cancelSource = null;
        }

        public void Send(Metric metric)
        {
            _queue.TryAdd(metric);
        }

        private void RunWorkerThread()
        {
            try
            {
                Metric carryoverMetric = null;
                var maxPacketSize = StatsdUDP.MaxUDPPacketSize;

                while (true)
                {
                    if (_cancelSource.IsCancellationRequested)
                        return;

                    List<Metric> metric = new List<Metric>();
                    List<string> metricsAsString = new List<string>();
                    Dictionary<string, int> mapNameToIndex = new Dictionary<string, int>();

                    int totLen = 0;
                    Metric firstMetric = null;

                    if (carryoverMetric != null)
                    {
                        firstMetric = carryoverMetric;
                        carryoverMetric = null;
                    }
                    else
                        firstMetric = _queue.Take(_cancelSource.Token);

                    if (firstMetric != null)
                    {
                        metric.Add(firstMetric);
                        var cmd = firstMetric.Command;
                        metricsAsString.Add(cmd);
                        totLen += cmd.Length;

                        if (firstMetric is IAllowsAggregate)
                            mapNameToIndex.Add(firstMetric.Name, metric.Count - 1);
                    }

                    Metric nextMetric = null;
                    DateTime delayStart = DateTime.UtcNow;
                    DateTime delayEnd = DateTime.UtcNow.AddMilliseconds(_config.MaxSendDelayMS);
                    int msRemaining;
                    while ((maxPacketSize <= 0 || totLen < maxPacketSize) && ((msRemaining = (int)(delayEnd - DateTime.UtcNow).TotalMilliseconds) > 0) && _queue.TryTake(out nextMetric, msRemaining, _cancelSource.Token))
                    {
                        if (nextMetric != null)
                        {
                            var canAggregate = nextMetric is IAllowsAggregate;
                            if (canAggregate && mapNameToIndex.ContainsKey(nextMetric.Name))
                            {
                                var existingItemIndex = mapNameToIndex[nextMetric.Name];
                                metric[existingItemIndex].Aggregate(nextMetric);

                                var oldStr = metricsAsString[existingItemIndex];
                                metricsAsString[existingItemIndex] = metric[existingItemIndex].Command;
                                totLen += (metricsAsString[existingItemIndex].Length - oldStr.Length);
                            }
                            else
                            {
                                var cmd = nextMetric.Command;
                                totLen += (cmd.Length + 1); // +1 for the \n separating each item
                                if (maxPacketSize <= 0 || totLen < maxPacketSize)
                                {
                                    metric.Add(nextMetric);
                                    metricsAsString.Add(cmd);
                                    if (canAggregate)
                                        mapNameToIndex.Add(nextMetric.Name, metric.Count - 1);
                                }
                                else
                                {
                                    carryoverMetric = nextMetric;
                                    break;
                                }
                            }
                        }
                    }

                    if (metric.Count != 0)
                    {
                        var data = string.Join("\n", metricsAsString.ToArray());
                        StatsdUDP.Send(data);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("StatsdClient::ThreadSafeConsumerProducerSender - Error: {0}", ex.ToString());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Stop();
        }

        public class Configuration
        {
            public int MaxSendDelayMS { get; set; }
            public int MaxThreads { get; set; }

            public Configuration()
            {
                this.MaxSendDelayMS = 5000;
                this.MaxThreads = 1;
            }
        }
    }
}

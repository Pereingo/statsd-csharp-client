using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using StatsdClient;
using StatsdClient.Senders;
using StatsdClient.MetricTypes;
using System.Collections.Generic;
using System.Threading;

namespace Tests
{
    [TestFixture]
    public class StatsdSenderTests
    {
        private IStatsdUDP _udp;

        [SetUp]
        public void Setup()
        {
            _udp = MockRepository.GenerateMock<IStatsdUDP>();
        }

        public class MockSenderTests : StatsdSenderTests
        {
            [Test]
            public void does_not_send_anything()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var sender = new MockSender();
                sender.Send(metric);

                _udp.AssertWasNotCalled(x => x.Send(Arg<string>.Is.Anything));
            }

            [Test]
            public void does_not_blow_up_if_metric_command_throws()
            {
                var metric = MockRepository.GenerateStub<Counting>();
                metric.Stub(x => x.Command).Throw(new Exception());

                var sender = new MockSender();
                sender.Send(metric);
                Assert.Pass();
            }
        }

        public class ImmediateSenderTests : StatsdSenderTests
        {
            [Test]
            public void does_not_blow_up_if_metric_command_throws()
            {
                var metric = MockRepository.GenerateStub<Counting>();
                metric.Stub(x => x.Command).Throw(new Exception());

                var sender = new ImmediateSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric);
                Assert.Pass();
            }

            [Test]
            public void does_not_blow_up_if_udp_send_throws()
            {
                var udpStub = MockRepository.GenerateStub<IStatsdUDP>();
                udpStub.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 1 };
                var sender = new ImmediateSender();
                sender.StatsdUDP = udpStub;
                sender.Send(metric);
                Assert.Pass();
            }

            [Test]
            public void sends_metric_immediately()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var sender = new ImmediateSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric);

                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo(metric.Command));
            }

            [Test]
            public void sends_multiple_metrics_individually()
            {
                var metric1 = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var metric2 = new Timing() { Name = "testtimer", ValueAsInt = 10 };

                var sender = new ImmediateSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric1);
                sender.Send(metric2);

                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(2));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo(metric1.Command));
                Assert.That(((string)argsPerCall[1][0]), Is.EqualTo(metric2.Command));
            }
        }

        public class BatchSenderTests : StatsdSenderTests
        {
            [Test]
            public void does_not_blow_up_if_metric_command_throws()
            {
                var metric = MockRepository.GenerateStub<Counting>();
                metric.Stub(x => x.Command).Throw(new Exception());

                var sender = new BatchSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric);
                sender.Flush();
                Assert.Pass();
            }

            [Test]
            public void does_not_blow_up_if_udp_send_throws()
            {
                var udpStub = MockRepository.GenerateStub<IStatsdUDP>();
                udpStub.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 1 };
                var sender = new BatchSender    ();
                sender.StatsdUDP = udpStub;
                sender.Send(metric);
                sender.Flush();
                Assert.Pass();
            }

            [Test]
            public void does_not_send_metric_immediately()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var sender = new BatchSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric);

                _udp.AssertWasNotCalled(x => x.Send(Arg<string>.Is.Anything));
            }

            [Test]
            public void sends_metric_on_flush()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var sender = new BatchSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric);
                sender.Flush();

                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo(metric.Command));
            }

            [Test]
            public void sends_multiple_metrics_at_once()
            {
                var metric1 = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var metric2 = new Timing() { Name = "testtimer", ValueAsInt = 10 };

                var sender = new BatchSender();
                sender.StatsdUDP = _udp;
                sender.Send(metric1);
                sender.Send(metric2);
                sender.Flush();

                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                var packet = (string)argsPerCall[0][0];
                var lines = packet.Split('\n');
                Assert.That(lines.Length, Is.EqualTo(2));
                Assert.That(lines[0], Is.EqualTo(metric1.Command));
                Assert.That(lines[1], Is.EqualTo(metric2.Command));
            }
        }

        public class ThreadSafeConsumerProducerSenderTests : StatsdSenderTests
        {
            [Test]
            public void does_not_blow_up_if_metric_command_throws()
            {
                var metric = MockRepository.GenerateStub<Counting>();
                metric.Stub(x => x.Command).Throw(new Exception());

                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 2000 });
                sender.StatsdUDP = _udp;
                sender.Send(metric);
                Assert.Pass();
            }

            [Test]
            public void does_not_blow_up_if_udp_send_throws()
            {
                var udpStub = MockRepository.GenerateStub<IStatsdUDP>();
                udpStub.Stub(x => x.Send(Arg<string>.Is.Anything)).Throw(new Exception());
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 1 };
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 2000 });
                sender.StatsdUDP = udpStub;
                sender.Send(metric);
                Assert.Pass();
            }

            [Test]
            public void sends_after_delay()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                DateTime timeCalled = DateTime.MaxValue;
                var udpStub = MockRepository.GenerateStub<IStatsdUDP>();
                udpStub.Stub(x => x.Send(Arg<string>.Is.Anything))
                    .WhenCalled(m => timeCalled = DateTime.Now);

                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 2000 });
                sender.StatsdUDP = udpStub;
                DateTime startTime = DateTime.Now;
                sender.Send(metric);
                Thread.Sleep(3000);

                IList<object[]> argsPerCall = udpStub.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo(metric.Command));

                var sendDelay = (timeCalled - startTime).TotalMilliseconds;
                Assert.That(sendDelay, Is.GreaterThanOrEqualTo(2000));
            }

            [Test]
            public void does_not_exceed_max_packet_size()
            {
                var udpStub = MockRepository.GenerateStub<IStatsdUDP>();
                udpStub.Stub(x => x.MaxUDPPacketSize).Return(300);

                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = udpStub;

                for (var i = 0; i < 100; i ++)
                {
                    var metricName = (Guid.NewGuid()).ToString();
                    var metric = new Counting() { Name = metricName, ValueAsInt = 1 };
                    sender.Send(metric);
                }

                Thread.Sleep(3000);
                IList<object[]> argsPerCall = udpStub.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                for (var i = 0; i < argsPerCall.Count; i++)
                {
                    var packetSize = ((string)argsPerCall[i][0]).Length;
                    Assert.That(packetSize, Is.LessThanOrEqualTo(300));
                }
            }

            [Test]
            public void bundles_multiple_metrics_into_one_packet()
            {
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = _udp;
                var metricsToSend = 10;
                for (var i = 0; i < metricsToSend; i++)
                {
                    var metricName = (Guid.NewGuid()).ToString();
                    var metric = new Counting() { Name = metricName, ValueAsInt = 1 };
                    sender.Send(metric);
                }

                Thread.Sleep(3000);
                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                var packetsReceived = argsPerCall.Count;
                Assert.That(packetsReceived, Is.LessThan(metricsToSend));
            }

            [Test]
            public void does_not_block()
            {
                var metric = new Counting() { Name = "testMetric", ValueAsInt = 5 };
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 2000 });
                sender.StatsdUDP = _udp;
                
                DateTime startTime = DateTime.Now;
                sender.Send(metric);
                DateTime endTime = DateTime.Now;

                var methodCallDelay = (endTime - startTime).TotalMilliseconds;
                Assert.That(methodCallDelay, Is.LessThan(10));
            }

            [Test]
            public void aggregates_counters()
            {
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = _udp;
                var metricsToSend = 10;
                for (var i = 0; i < metricsToSend; i++)
                {
                    var metric = new Counting() { Name = "testMetric", ValueAsInt = 1 };
                    sender.Send(metric);
                }

                Thread.Sleep(1500);
                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo("testMetric:" + metricsToSend.ToString() + "|c"));
            }

            [Test]
            public void aggregates_gauges()
            {
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = _udp;
                var metricsToSend = 10;
                Metric lastMetricSent = null;
                for (var i = 0; i < metricsToSend; i++)
                {
                    var metric = new Gauge() { Name = "testMetric", ValueAsDouble = 1 };
                    sender.Send(metric);
                    lastMetricSent = metric;
                }

                Thread.Sleep(1500);
                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(1));
                Assert.That(((string)argsPerCall[0][0]), Is.EqualTo(lastMetricSent.Command));
            }

            [Test]
            public void does_not_aggregate_timers()
            {
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = _udp;
                var metricsToSend = 10;
                var metric = new Timing() { Name = "testMetric", ValueAsInt = 50 };
                for (var i = 0; i < metricsToSend; i++)
                    sender.Send(metric);

                Thread.Sleep(1500);
                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));

                for (var i = 0; i < argsPerCall.Count; i ++)
                {
                    var packet = (string)argsPerCall[i][0];
                    var lines = packet.Split('\n');
                    for(var j = 0; j < lines.Length; j ++)
                    {
                        Assert.That(lines[j], Is.EqualTo(metric.Command));
                    }
                }
            }

            [Test]
            public void stops_worker_threads_after_dispose()
            {
                var sender = new ThreadSafeConsumerProducerSender(new ThreadSafeConsumerProducerSender.Configuration() { MaxSendDelayMS = 1000 });
                sender.StatsdUDP = _udp;
                var metric = new Timing() { Name = "testMetric", ValueAsInt = 50 };

                sender.Dispose();
                sender.Send(metric);

                Thread.Sleep(1500);
                IList<object[]> argsPerCall = _udp.GetArgumentsForCallsMadeOn(x => x.Send(Arg<string>.Is.Anything));
                Assert.That(argsPerCall.Count, Is.EqualTo(0));
            }
        }
    }
}
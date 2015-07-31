C# Statsd Client - Thread Safe Consumer/Producer Sender
=======================================================

Kyle West (kwest2123@yahoo.com)
7/30/2015


This fork adds a few features to the statsD client:

- Thread safe
- Bundles multiple metrics into a UDP packet (up to max packet size) for increased performance.
- Aggregates metric types that can be aggregated (counters and gauges) and sends the aggregated result rather than sending the same metric multiple times within the same packet
- Uses the producer/consumer pattern to allow a different number of producer threads vs. sender thread(s)


With this fork, an unlimited number of threads can all safely send metrics to the same StatsD client instance.  The client uses a .NET 4 BlockingCollection to add metrics to be sent into a thread safe queue without requiring a lock, so it runs very fast and is thread safe.  A configurable number of consumer worker thread(s) monitor this queue and send metrics to the server.  The default value in the code if not specified is 1 thread, which is probably fine for most applications.

There are two configuration options that control send behavior.

| Configuration Option | Default Value | Description                                                                                                                                                                                                                                                                                                                                      |
|----------------------|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| MaxSendDelayMS       | 5000          | Maximum amount of time (in milliseconds), to wait for additional metrics to be sent before bundling up the metrics and sending them to the server.  It is important that this value is always smaller than the flush interval.                                                                                                                   |
| MaxThreads           | 1             | Number of worker threads that will be used to send metrics to StatsD.  In very high volume use cases, a single worker thread may not be able to keep up with all of the metrics to be sent.  In that case, the number of threads can be increased with this option.  In most cases, the default value of one worker thread should be sufficient. |


To configure these options, create an instance of the ThreadSafeConsumerProducerSender using the appropriate configuration options, and use it as the value for the Sender property in your MetricsConfig object.

``` C#
var metricsConfig = new MetricsConfig
{
  StatsdServerName = "host.name",
  Prefix = "myApp",
  Sender = new ThreadSafeConsumerProducerSender(
    new ThreadSafeConsumerProducerSender.Configuration() { 
      MaxSendDelayMS = 5000,
      MaxThreads = 3
};
Metrics.Configure(metricsConfig);

// Or, if using the Statsd class directly:
var sender = new ThreadSafeConsumerProducerSender(
  new ThreadSafeConsumerProducerSender.Configuration() {
    MaxSendDelayMS = 5000,
    MaxThreads = 3
  });
var statsd = new Statsd(new Statsd.Configuration() { Udp = ..., Sender = sender });
```
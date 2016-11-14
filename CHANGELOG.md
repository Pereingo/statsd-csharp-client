## 1.3.44
- Add support for [TCP](https://github.com/etsy/statsd/blob/master/docs/server.md) sending via the `MetricsConfig.UseTcpProtocol` property (thanks @[pekiZG](https://github.com/pekiZG)!)

## 1.2.32
- Fix the `Stopwatch` class to make it more consisten with .NET's and fix an overflow bug (thanks @[knocte]!)
- Fix bug in how IPv4 addresses are resolved 

## 1.1.0
- Add support for [gauge delta values](https://github.com/etsy/statsd/blob/master/docs/metric_types.md#gauges) (thanks @[crunchie84](https://github.com/crunchie84)!)
- Mark the `Metrics.Gauge()` method obsolete in favour of the new `Metrics.GaugeAbsoluteValue()`
- Mark the `StatsdClient.Configuration.Naming` class obsolete in favour of setting `MetricsConfig.Prefix` when you call `Metrics.Configure()`, which reduces the code you need when actually sending metrics each time
- Expose a sample rate for the disposable version of the timer (`Metrics.StartTimer()`)

## 1.1.0
- Added support for [gauge delta values](https://github.com/etsy/statsd/blob/master/docs/metric_types.md#gauges) (thanks @crunchie84!)
- Marked the `Metrics.Gauge()` method obsolete in favour of the new `Metrics.GaugeAbsoluteValue()`
- Marked the `StatsdClient.Configuration.Naming` class obsolete in favour of setting `MetricsConfig.Prefix` when you call `Metrics.Configure()`, which reduces the code you need when actually sending metrics each time.
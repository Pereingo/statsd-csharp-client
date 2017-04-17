## 3.0.86
- **BREAKING:** Changes to lower level interfaces unlikely to affect most users, including `IStatsd` and `IStopwatch`
- Fix threading bug when calling `Metrics.Send()` from multiple threads (thanks for helping track down @[bronumski](https://github.com/bronumski)!)
- Fix timing reporting bug that significantly under reported multiple-millisecond timings (thanks @[arexsutton](https://github.com/arexsutton)!)
- Fix casing for .NET Core dependency (thanks for pointing out @[mikemitchellrightside](https://github.com/mikemitchellrightside)!)

## 2.0.68
- **BREAKING:** Drops support for < .NET 4.5
- Add .NET Standard 1.3 support (thanks @[TerribleDev](https://github.com/TerribleDev)!)
- Fix async support (previously would only measure async creation time)

## 1.4.51
- Add a `Metrics.IsConfigured()` method which returns whether the Metrics class has been initialised yet (thanks @[dkhanaferov](https://github.com/dkhanaferov)!)

## 1.3.44
- Add support for [TCP](https://github.com/etsy/statsd/blob/master/docs/server.md) sending via the `MetricsConfig.UseTcpProtocol` property (thanks @[pekiZG](https://github.com/pekiZG)!)

## 1.2.32
- Fix the `Stopwatch` class to make it more consisten with .NET's and fix an overflow bug (thanks @[knocte](https://github.com/knocte)!)
- Fix bug in how IPv4 addresses are resolved 

## 1.1.0
- Add support for [gauge delta values](https://github.com/etsy/statsd/blob/master/docs/metric_types.md#gauges) (thanks @[crunchie84](https://github.com/crunchie84)!)
- Mark the `Metrics.Gauge()` method obsolete in favour of the new `Metrics.GaugeAbsoluteValue()`
- Mark the `StatsdClient.Configuration.Naming` class obsolete in favour of setting `MetricsConfig.Prefix` when you call `Metrics.Configure()`, which reduces the code you need when actually sending metrics each time
- Expose a sample rate for the disposable version of the timer (`Metrics.StartTimer()`)

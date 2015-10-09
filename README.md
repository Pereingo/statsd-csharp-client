C# Statsd Client
================

A C# client to interface with Esty's awesome [statsd](https://github.com/etsy/statsd).

Install it via NuGet with the [StatsdClient package](http://nuget.org/packages/StatsdClient).

##Usage

At app startup, configure the `Metrics` class (other options are documented on the config object):

``` C#
Metrics.Configure(new MetricsConfig
{
  StatsdServerName = "hostname",
  Prefix = "myApp.prod"
});
```

Then use it (liberally!) in your app:

``` C#
Metrics.Counter("stat-name");
Metrics.Time(() => myMethod(), "stat-name"));
Metrics.GaugeAbsolute("gauge-name", 35);
Metrics.GaugeDelta("gauge-name", -5);
Metrics.Set("something-special", "3");
```

You can also time with the disposable overload:

``` C#
using (Metrics.StartTimer("stat-name"))
{
  // Lots of code here
}
```

Including functions that return a value:

``` C#
var result = Metrics.Time(() => GetResult(), "stat-name");
```
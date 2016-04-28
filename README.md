Statsd Client
=============

[![Build status](https://ci.appveyor.com/api/projects/status/fklgn25u3k66qu3v?svg=true)](https://ci.appveyor.com/project/DarrellMozingo/statsd-csharp-client)
[![NuGet Version](http://img.shields.io/nuget/v/StatsdClient.svg?style=flat)](https://www.nuget.org/packages/StatsdClient/)

A C# client to interface with Etsy's excellent [statsd](https://github.com/etsy/statsd) server.

Install the client via NuGet with the [StatsdClient package](http://nuget.org/packages/StatsdClient).

##Usage

At app startup, configure the `Metrics` class (other options are documented on `MetricsConfig`):

### Advices

* It's advisable to use UDP over TCP socket protocol (default is UDP).
  If you need TCP protocol maybe it's better to split that responsibility out to another app.
  (ie. have a statsd relay running on each server that you'd send UDP stats to, and it would then relay them in TCP)

### Examples

``` C#
Metrics.Configure(new MetricsConfig
{
  StatsdServerName = "hostname",
  Prefix = "myApp.prod"
});
```

Then start measuring all the things!

``` C#
Metrics.Counter("stat-name");
Metrics.Time(() => myMethod(), "stat-name");
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

##Development
* Please have a chat about any big features before submitting PR's
* NuGet is packaged as an artefact on AppVeyor above. Grab that `*.nupkg` and upload it to NuGet.org
* Change major/minor versions in `appveyor.yml`

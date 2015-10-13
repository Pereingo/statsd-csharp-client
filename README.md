Statsd Client
=============

[![Build status](https://ci.appveyor.com/api/projects/status/fklgn25u3k66qu3v?svg=true)](https://ci.appveyor.com/project/DarrellMozingo/statsd-csharp-client)

A C# client to interface with Etsy's excellent [statsd](https://github.com/etsy/statsd) server.

Install the client via NuGet with the [StatsdClient package](http://nuget.org/packages/StatsdClient).

##Usage

At app startup, configure the `Metrics` class (other options are documented on `MetricsConfig`):

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

##Development
* Please have a chat about any big features before submitting PR's
* NuGet is packaged as an artefact on AppVeyor above. Grab that `*.nupkg` and upload it to NuGet.org
* Change major/minor versions in `appveyor.yml`

DogStatsD for C#
================

A C# [DogStatsD](http://docs.datadoghq.com/guides/dogstatsd/) client. DogStatsD
is an extension of the [StatsD](http://codeascraft.com/2011/02/15/measure-anything-measure-everything/)
metric server for [Datadog](http://datadoghq.com).

Installation
------------

Once the first release is completed, you will be able to get it from NuGet.
For now you can get the source from here and build it.

Usage via the static Metrics class:
-----------------------------

At start of your app, configure the `Metrics` class like this:

    var metricsConfig = new MetricsConfig
    {
      StatsdServerName = "host.name",
      StatsdPort = 8125, // Optional; default is 8125
      Prefix = "myApp" // Optional; by default no prefix will be prepended
    };

    StatsdClient.Metrics.Configure(metricsConfig);

Where "host.name" is the name of the statsd server, 8125 is the optional statsd port number, and "myApp" is an optional prefix that is prepended on all stats.

Then start instrumenting your code:

    // Increment a counter by 1
    Metrics.Increment("eventname");

    // Decrement a counter by 1
    Metrics.Decrement("eventname");

    // Increment a counter by a specific value
    Metrics.Counter("page.views", page.views);

    // Record a gauge
    Metrics.Gauge("gas_tank.level", 0.75);

    // Sample a histogram
    Metrics.Histogram("file.size", file.size);

    // Add an element to a set
    Metrics.Set("users.unique", user.id);

    // Time a block of code
    using (Metrics.StartTimer("stat-name"))
    {
        DoSomethingAmazing();
        DoSomethingFantastic();
    }

    // Time an action
    Metrics.Time(() => DoMagic(), "stat-name");

    // Timing an action preserves its return value
    var result = Metrics.Time(() => GetResult(), "stat-name");

    // See note below for how exceptions in timed methods or blocks are handled

    // Every metric type supports tags and sample rates
    Metrics.Set("users.unique", user.id, tags: new[] {"country:canada"});
    Metrics.Gauge("gas_tank.level", 0.75, sampleRate: 0.5, tags: new[] {"hybrid", "trial_1"});
    using (Metrics.StartTimer("stat-name", sampleRate: 0.1))
    {
        DoSomethingFrequent();
    }

A note about timing: Metrics will not attempt to handle any exceptions that occur in a
timed block or method. If an unhandled exception is thrown while
timing, a timer metric containing the time elapsed before the exception
occurred will be submitted.

Change Log
----------

To do once first release is out

Usage via the Statsd class:
---------------------------

In most cases, the static Metrics class is probably better to use.
However, the Statsd is useful when you want to queue up a number of metrics to be sent in
one UDP message (via the Add method).

    // NB: StatsdUDP is IDisposable and if not disposed, will leak resources
    StatsdUDP udp = new StatsdUDP(HOSTNAME, PORT);
    using (udp)
    {
      Statsd s = new Statsd(udp);

      // Incrementing a counter by 1
      s.Send<Statsd.Counting,int>("stat-name", 1);

      // Recording a gauge
      s.Send<Statsd.Gauge,double>("stat-name", 5,5);

      // Sampling a histogram
      s.Send<Statsd.Histogram,int>("stat-name", 1);

      // Send an element to a set
      s.Send<Statsd.Set,int>("stat-name", 1);

      // Send a timer
      s.Send<Statsd.Timing,double>("stat-name", 3.1337);

      // Time a method
      s.Send(() => MethodToTime(), "stat-name");

      // See note below on how exceptions in timed methods are handled

      // All types have optional sample rates and tags:
      s.Send<Statsd.Counting,int>("stat-name", 1, sampleRate: 1/10, tags: new[] {"tag1:true", "tag2"});

      // You can add combinations of messages which will be sent in one go:
      s.Add<Statsd.Counting,int>("stat-name", 1);
      s.Add<Statsd.Timing,int>("stat-name", 5, sampleRate: 1/10);
      s.Send(); // message will contain counter and will contain timer 10% of the time

      // All previous commands will be flushed after any Send
      // Any Adds will be ignored if using a Send directly
      s.Add<Statsd.Counting,int>("stat-name", 1);
      s.Send<Statsd.Timing,double>("stat-name", 4.4); // message will only contain Timer
      s.Send(); // the counter will not be sent by the command
     }

A note about timing: Statsd will not attempt to handle any exceptions that occur in a
timed method. If an unhandled exception is thrown while
timing, a timer metric containing the time elapsed before the exception
occurred will be sent or added to the send queue (depending on whether Send or
Add is being called).

Feedback
--------

To suggest a feature, report a bug, or general discussion, head over
[here](https://github.com/DataDog/statsd-csharp-client/issues).

Credits
-------

dogstatsd-csharp-client is forked from Goncalo Pereira's [original Statsd
client](https://github.com/goncalopereira/statsd-csharp-client).

Copyright (c) 2012 Goncalo Pereira and all contributors. See MIT-LICENCE.md for
further details.

Thanks to Goncalo Pereira, Anthony Steele, Darrell Mozingo, Antony Denyer, and Tim Skauge for their contributions to the original client.


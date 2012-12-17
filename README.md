C# Statsd Client
================

Installation
------------

You can [get the "StatsdClient" package on nuget](http://nuget.org/packages/StatsdClient).
Or you can get the source from here on giuthub and build it.

Usage
------

Via the static Metrics class:
-----------------------------

At start of your app, configure the `Metrics` class like this:

    var metricsConfig = new MetricsConfig
    {
      StatsdServerName = "host.name",
      Prefix = "myApp"
    };
    
    StatsdClient.Metrics.Configure(metricsConfig);
		
Where "host.name" is the name of the statsd server and "myApp" is an optional prefix that is prepended on all stats. How you set it up is up to you, but we read the server name and other settings from web.config and and generate a prefix out of the enviroment (e.g. "Local", "Uat" or "Live"), plus the app name and machine name, seperated with dots. 

Use it like this afterwards:

    Metrics.Counter("stat-name");
    Metrics.Gauge("gauge-name", gaugeValue);
  
 And timing around blocks of code:
 
    using (Metrics.StartTimer("stat-name"))
    {
      DoMagic();
    }

And adding a timer using a stat name and time value calculated by your code:

    Metrics.Timer("stat-name", (int)stopwatch.ElapsedMilliseconds);
	
And timing an action

    Metrics.Timer(() => DoMagic(), "stat-name");

or replace a method that returns a value

    var result = GetResult();

with a timed `Action<T>` that returns the same value

    var result = Metrics.Timer(() => GetResult(), "stat-name"); 

Via the Statsd class:
---------------------

    Statsd s = new Statsd(new StatsdUDP(HOSTNAME, PORT));

    //All the standard Statsd message types:
    s.Send<Statsd.Counting>("stat-name", 1); //counter had one hit
    s.Send<Statsd.Timing>("stat-name", 5); //timer had one hit of 5ms
    s.Send<Statsd.Gauge>("stat-name", 5); //gauge had one hit of value 5
    
    //All types have sample rate, which will be included in the message for Statsd's own stats crunching:
    s.Send<Statsd.Counting>("stat-name", 1, 1/10); //counter had one hit, this will be sent 10% of times it is called

    //You can add combinations of messages which will be sent in one go:
    s.Add<Statsd.Counting>("stat-name", 1);
    s.Add<Statsd.Timer>("stat-name", 5, 1/10);
    s.Send(); //message will contain counter and will contain timer 10% of the time
    
    //All previous commands will be flushed after any Send
    //Any Adds will be ignored if using a Send directly
    
    //Optional naming conventions:
    // environment named 'env'
    // application named 'app'
    // hostname named 'host'

    string name = Naming.withEnvironmentApplicationAndHostname("stat"); //== "env.app.stat.host"
    string anotherName  = Naming.withEnvironmentAndApplication("stat"); //== "env.app.stat"

    //You can also use Actions to easily time responses:

    s.Send(() => DoMagic(), "stat-name", 1/10); //log the response time for DoMagic call as a timer
    s.Send(() => DoMagic(), "stat-name"); //same with no sample rate
    s.Add(() => DoMagic(), "stat-name"); //you can just add it too

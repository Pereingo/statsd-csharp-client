using NUnit.Framework;

namespace StatsdClient.Core.Tests
{
    /// <summary>
    /// Metrics is static (not thread static), so to consistently test this before .Configure() is called just doing a cheeky A_* naming so it's first alphabetically.
    /// </summary>
    public class A_MetricsTests
    {
        [Test]
        public void defaults_to_null_statsd_to_not_blow_up_when_configure_is_not_called()
        {
            Assert.DoesNotThrow(async () => await Metrics.CounterAsync("stat"));
        }
    }
}
using System;
using NUnit.Framework;

namespace StatsdClient.Core.Tests
{
    public class MetricsTests
    {
        [Test]
        public void throws_when_configured_with_a_null_configuration()
        {
            Assert.Throws<ArgumentNullException>(async() => await Metrics.ConfigureAsync(null));
        }
    }
}
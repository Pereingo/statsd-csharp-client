using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
	public class MetricsTests
	{
		[Test]
		public void throws_when_configured_with_a_null_configuration()
		{
			Assert.Throws<ArgumentNullException>(() => Metrics.Configure(null));
		}
	}
}
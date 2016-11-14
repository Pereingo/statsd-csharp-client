using System;
using System.Reflection;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
	public class MetricsTests
	{
		/// <summary>
		/// Since Metrics is a static class withs static fields, in order to test functionality 
		/// of methods this cleanup method will clean up the static state of the Metrics class between each
		/// test iteration.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			foreach (var field in typeof(Metrics).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
			{
				field.SetValue(null, null);
			}
		}

		[Test]
		public void throws_when_configured_with_a_null_configuration()
		{
			Assert.Throws<ArgumentNullException>(() => Metrics.Configure(null));
		}

		[Test]
		public void IsConfigured_returns_false_when_configuration_is_null()
		{
			Assert.IsFalse(Metrics.IsConfigured());
		}

		[Test]
		public void IsConfigured_returns_true_when_configuration_is_not_null()
		{
			//Configure metrics
			Metrics.Configure(new MetricsConfig());

			//Validate
			Assert.IsFalse(Metrics.IsConfigured());
		}
	}
}
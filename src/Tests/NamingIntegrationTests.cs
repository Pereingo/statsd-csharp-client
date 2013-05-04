using StatsdClient.Configuration;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class NamingIntegrationTests
    {
        [Test]
        public void environment_name()
        {
            Assert.That(Naming.CurrentEnvironment,Is.EqualTo("environment"));
        }

        [Test]
        public void application_name()
        {
            Assert.That(Naming.CurrentApplication, Is.EqualTo("application"));
        }

        [Test]
        public void host_name()
        {
            Assert.That(Naming.CurrentHostname, Is.Not.Empty);
        }

        [Test]
        public void stat_with_environment_application()
        {
            Assert.That(Naming.withEnvironmentAndApplication("stat"),Is.EqualTo("environment.application.stat"));
        }

        [Test]
        public void stat_with_environment_application_and_host_name()
        {
            const string statNameStartsWith = "environment.application.stat.";
            Assert.That(Naming.withEnvironmentApplicationAndHostname("stat"), Is.StringStarting(statNameStartsWith));
            Assert.That(Naming.withEnvironmentApplicationAndHostname("stat").Length, Is.GreaterThan(statNameStartsWith.Length));
        }
    }
}

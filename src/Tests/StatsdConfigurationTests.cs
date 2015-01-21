using System;
using System.Threading;
using NUnit.Framework;
using StatsdClient;
using Tests.Helpers;


namespace Tests
{
    [TestFixture]
    public class StatsdConfigurationTest
    {
        private void testReceive(string testServerName, int testPort, string testCounterName,
                                 string expectedOutput)
        {
            UdpListener udpListener = new UdpListener(testServerName, testPort);
            Thread listenThread = new Thread(new ParameterizedThreadStart(udpListener.Listen));
            listenThread.Start();
            DogStatsd.Increment(testCounterName);
            while (listenThread.IsAlive) ;
            Assert.AreEqual(expectedOutput, udpListener.GetAndClearLastMessages()[0]);
            udpListener.Dispose();
        }

        [Test]
        public void throw_exception_when_no_config_provided()
        {
            StatsdConfig metricsConfig = null;
            Assert.Throws<ArgumentNullException>(() => StatsdClient.DogStatsd.Configure(metricsConfig));
        }

        [Test]
        public void throw_exception_when_no_hostname_provided()
        {
            var metricsConfig = new StatsdConfig { };
            Assert.Throws<ArgumentNullException>(() => StatsdClient.DogStatsd.Configure(metricsConfig));
        }

        [Test]
        public void default_port_is_8125()
        {
            var metricsConfig = new StatsdConfig
            {
                StatsdServerName = "127.0.0.1"
            };
            StatsdClient.DogStatsd.Configure(metricsConfig);
            testReceive("127.0.0.1", 8125, "test", "test:1|c");
        }

        [Test]
        public void setting_port()
        {
            var metricsConfig = new StatsdConfig
            {
                StatsdServerName = "127.0.0.1",
                StatsdPort = 8126
            };
            StatsdClient.DogStatsd.Configure(metricsConfig);
            testReceive("127.0.0.1", 8126, "test", "test:1|c");
        }

        [Test]
        public void setting_prefix()
        {
            var metricsConfig = new StatsdConfig
            {
                StatsdServerName = "127.0.0.1",
                StatsdPort = 8129,
                Prefix = "prefix"
            };
            StatsdClient.DogStatsd.Configure(metricsConfig);
            testReceive("127.0.0.1", 8129, "test", "prefix.test:1|c");
        }
    }
}

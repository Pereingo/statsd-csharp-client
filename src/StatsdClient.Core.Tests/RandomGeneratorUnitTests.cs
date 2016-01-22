using System;
using NUnit.Framework;

namespace StatsdClient.Core.Tests
{
    [TestFixture]
    public class RandomGeneratorUnitTests
    {
        private readonly RandomGenerator _randomGenerator = new RandomGenerator();
        private const int NumberOfTestsToRun = 1000;

        [Test]
        public void If_sample_rate_is_1_then_always_true()
        {
            for (var i = 0; i < NumberOfTestsToRun; i++)
            {
                Assert.True(_randomGenerator.ShouldSend(1));
            }
        }

        [Test]
        public void If_sample_rate_is_0_5_then_have_half_true()
        {
            var numberOfTrues = 0;
            var randomGenerator = new RandomGenerator();

            for (var i = 0; i < NumberOfTestsToRun; i++)
            {
                if (randomGenerator.ShouldSend(0.5))
                {
                    numberOfTrues++;
                }
            }

            Assert.That( Math.Round(numberOfTrues/(double)NumberOfTestsToRun,1),Is.EqualTo(0.5));
        }

        [Test]
        public void If_sample_rate_is_one_quarter_then_have_one_quarter_true()
        {
            var numberOfTrues = 0;
            var randomGenerator = new RandomGenerator();
            const int sampleRate = 1/4;

            for (var i = 0; i < NumberOfTestsToRun; i++)
            {
                if (randomGenerator.ShouldSend(sampleRate))
                {
                    numberOfTrues++;
                }
            }

            Assert.That(Math.Round(numberOfTrues / (double)NumberOfTestsToRun, 1), Is.EqualTo(sampleRate));
        }

        [Test]
        public void If_sample_rate_is_one_tenth_of_pct_then_have_one_tenth_of_pct()
        {
            var numberOfTrues = 0;
            var randomGenerator = new RandomGenerator();
            const int sampleRate = 1/1000;

            for (var i = 0; i < NumberOfTestsToRun; i++)
            {
                if (randomGenerator.ShouldSend(sampleRate))
                {
                    numberOfTrues++;
                }
            }

            Assert.That(Math.Round(numberOfTrues / (double)NumberOfTestsToRun, 1), Is.EqualTo(sampleRate));
        }
    }
}

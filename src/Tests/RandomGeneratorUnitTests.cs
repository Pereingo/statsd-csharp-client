using System;
using NUnit.Framework;
using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class RandomGeneratorUnitTests
    {
        private const int NumberOfTestsRan = 1000;

        [Test]
        public void If_sample_rate_is_1_then_always_true()
        {
            RandomGenerator randomGenerator = new RandomGenerator();
            for (int i = 0; i < NumberOfTestsRan; i++)
            {
                Assert.True(randomGenerator.ShouldSend(1));                
            }
        }

        [Test]
        public void If_sample_rate_is_0_5_then_have_half_true()
        {
            int numberOfTrues = 0;
            RandomGenerator randomGenerator = new RandomGenerator();
            for (int i = 0; i < NumberOfTestsRan; i++)
            {
                if (randomGenerator.ShouldSend(0.5))
                {
                    numberOfTrues++;
                }
            }

            Assert.That( Math.Round(numberOfTrues/(double)NumberOfTestsRan,1),Is.EqualTo(0.5));
        }

        [Test]
        public void If_sample_rate_is_one_quarter_then_have_one_quarter_true()
        {
            int numberOfTrues = 0;
            RandomGenerator randomGenerator = new RandomGenerator();
            const int sampleRate = 1/4;
            for (int i = 0; i < NumberOfTestsRan; i++)
            {
                if (randomGenerator.ShouldSend(sampleRate))
                {
                    numberOfTrues++;
                }
            }

            Assert.That(Math.Round(numberOfTrues / (double)NumberOfTestsRan, 1), Is.EqualTo(sampleRate));
        }

        [Test]
        public void If_sample_rate_is_one_tenth_of_pct_then_have_one_tenth_of_pct()
        {
            int numberOfTrues = 0;
            RandomGenerator randomGenerator = new RandomGenerator();
            const int sampleRate = 1/1000;
            for (int i = 0; i < NumberOfTestsRan; i++)
            {
                if (randomGenerator.ShouldSend(sampleRate))
                {
                    numberOfTrues++;
                }
            }

            Assert.That(Math.Round(numberOfTrues / (double)NumberOfTestsRan, 1), Is.EqualTo(sampleRate));
        }
    }
}

namespace DHI.Services.Test.GuardClauses
{
    using System;
    using Xunit;

    public class GuardAgainstNegativeOrZeroTest
    {
        [Fact]
        public void DoesNotThrowGivenPositiveValue()
        {
            Guard.Against.NegativeOrZero(1, "intPositive");
            Guard.Against.NegativeOrZero(1.0f, "floatPositive");
            Guard.Against.NegativeOrZero(1.0, "doublePositive");
        }

        [Fact]
        public void ThrowsGivenZeroValue()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(0, "intZero"));
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(0f, "floatZero"));
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(0.0, "doubleZero"));
        }

        [Fact]
        public void ThrowsGivenNegativeValue()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(-1, "intNegative"));
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(-1f, "floatNegative"));
            Assert.Throws<ArgumentException>(() => Guard.Against.NegativeOrZero(-1.0, "doubleNegative"));
        }

        [Fact]
        public void ReturnsExpectedWhenGivenPositiveValue()
        {
            Assert.Equal(1, Guard.Against.NegativeOrZero(1, "intPositive"));
            Assert.Equal(1.0f, Guard.Against.NegativeOrZero(1.0f, "floatPositive"));
            Assert.Equal(1.0, Guard.Against.NegativeOrZero(1.0, "doublePositive"));
        }
    }
}
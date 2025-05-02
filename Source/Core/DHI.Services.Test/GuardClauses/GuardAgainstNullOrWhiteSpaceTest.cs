namespace DHI.Services.Test.GuardClauses
{
    using System;
    using Xunit;

    public class GuardAgainstNullOrWhiteSpaceTest
    {
        [Theory]
        [InlineData("a")]
        [InlineData("1")]
        [InlineData("some text")]
        [InlineData(" leading whitespace")]
        [InlineData("trailing whitespace ")]
        public void DoesNotThrowGivenNonWhitespaceStringValue(string nonWhitespaceString)
        {
            Guard.Against.NullOrWhiteSpace(nonWhitespaceString, "string");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("  \n")]
        public void ThrowsGivenWhiteSpaceString(string whiteSpaceString)
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrWhiteSpace(whiteSpaceString, "whiteSpaceString"));
        }

        [Fact]
        public void ThrowsGivenEmptyString()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrWhiteSpace("", "emptyString"));
        }

        [Fact]
        public void ThrowsGivenNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.Against.NullOrWhiteSpace(null, "null"));
        }
    }
}

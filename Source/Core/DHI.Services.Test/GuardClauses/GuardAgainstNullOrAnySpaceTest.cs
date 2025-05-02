namespace DHI.Services.Test.GuardClauses
{
    using System;
    using Xunit;

    public class GuardAgainstNullOrAnySpaceTest
    {
        [Theory]
        [InlineData("a")]
        [InlineData("1")]
        [InlineData("123.456")]
        [InlineData("MyText")]
        public void DoesNotThrowGivenStringWithoutSpaces(string stringWithoutSpaces)
        {
            Guard.Against.NullOrAnySpace(stringWithoutSpaces, "string");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("  \n")]
        public void ThrowsGivenWhiteSpaceString(string whiteSpaceString)
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrAnySpace(whiteSpaceString, "whiteSpaceString"));
        }

        [Fact]
        public void ThrowsGivenEmptyString()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrAnySpace("", "emptyString"));
        }

        [Fact]
        public void ThrowsGivenNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.Against.NullOrAnySpace(null, "null"));
        }

        [Fact]
        public void ThrowsGivenStringWithSpaces()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrAnySpace("This string has spaces", "string"));
        }
    }
}

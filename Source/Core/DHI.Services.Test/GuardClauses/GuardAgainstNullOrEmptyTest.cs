namespace DHI.Services.Test.GuardClauses
{
    using System;
    using System.Linq;
    using Xunit;

    public class GuardAgainstNullOrEmptyTest
    {
        [Fact]
        public void DoesNotThrowGivenNonEmptyEnumerable()
        {
            Guard.Against.NullOrEmpty(new[] {"foo", "bar"}, "stringArray");
            Guard.Against.NullOrEmpty(new[] {1, 2}, "intArray");
        }

        [Fact]
        public void DoesNotThrowGivenNonEmptyStringValue()
        {
            Guard.Against.NullOrEmpty("a", "string");
            Guard.Against.NullOrEmpty("1", "aNumericString");
        }

        [Fact]
        public void ThrowsGivenEmptyEnumerable()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(Enumerable.Empty<string>(), "emptyStringEnumerable"));
        }

        [Fact]
        public void ThrowsGivenEmptyString()
        {
            Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty("", "emptyString"));
        }

        [Fact]
        public void ThrowsGivenNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.Against.NullOrEmpty(null, "null"));
        }
    }
}
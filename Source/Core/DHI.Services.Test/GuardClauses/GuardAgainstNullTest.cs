namespace DHI.Services.Test.GuardClauses
{
    using System;
    using Xunit;

    public class GuardAgainstNullTest
    {
        [Fact]
        public void DoesNotThrowGivenNonNullValue()
        {
            Guard.Against.Null("", "string");
            Guard.Against.Null("  ", "string");
            Guard.Against.Null(1, "int");
            Guard.Against.Null(Guid.Empty, "guid");
            Guard.Against.Null(DateTime.Now, "datetime");
            Guard.Against.Null(new object(), "object");
        }

        [Fact]
        public void ThrowsGivenNullValue()
        {
            object obj = null;
            Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(obj, "null"));
        }

        [Fact]
        public void ReturnsExpectedValueWhenGivenNonNullValue()
        {
            Assert.Equal("", Guard.Against.Null("", "string"));
            Assert.Equal(1, Guard.Against.Null(1, "int"));

            var guid = Guid.Empty;
            Assert.Equal(guid, Guard.Against.Null(guid, "guid"));

            var now = DateTime.Now;
            Assert.Equal(now, Guard.Against.Null(now, "datetime"));
        }
    }
}
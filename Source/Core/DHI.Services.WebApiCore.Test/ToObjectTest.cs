namespace DHI.Services.WebApiCore.Test
{
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ToObjectTest
    {
        [Theory]
        [InlineData("99", 99)]
        [InlineData("-99", -99)]
        [InlineData("99 ", 99)]
        [InlineData("9.9", 9.9)]
        [InlineData(" 9.9", 9.9)]
        [InlineData("9,999.9", 9999.9)]
        [InlineData("-9,999.9", -9999.9)]
        [InlineData("true", true)]
        [InlineData("LogLevel.Critical", LogLevel.Critical)]
        [InlineData("Critical", "Critical")]
        [InlineData("SomeString", "SomeString")]
        public void ToObjectIsOk(string stringValue, object value)
        {
            stringValue.ToObject().Should().Be(value);
        }

        [Fact]
        public void ToObjectForDateTimeIsOk()
        {
            "2019.12.24T08:30:00".ToObject().Should().Be(24.December(2019).At(8, 30));
            "2019-12-24".ToObject().Should().Be(24.December(2019));
        }
    }
}
namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using System;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ToObjectTest
    {
        [Fact]
        public void ToEmptyString()
        {
            var result = "".ToObject();

            Assert.Equal("", result);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("123", 123)]
        [InlineData("-123", -123)]
        public void ToInteger(string value, object expected)
        {
            var result = value.ToObject();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("0.00", 0.00d)]
        [InlineData("123.54", 123.54d)]
        [InlineData("-123.54", -123.54d)]
        public void ToDouble(string value, object expected)
        {
            var result = value.ToObject();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToDate()
        {
            var result = "2022-08-23".ToObject();

            Assert.Equal(new DateTime(2022, 08, 23, 0, 0, 0), result);
        }

        [Fact]
        public void ToDateTime()
        {
            var result = "2022-08-23T16:35:27".ToObject();

            Assert.Equal(new DateTime(2022, 08, 23, 16, 35, 27, 0, DateTimeKind.Unspecified), result);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("yes", "yes")]
        [InlineData("no", "no")]
        public void ToBool(string value, object expected)
        {
            var result = value.ToObject();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("LogLevel.Debug", LogLevel.Debug)]
        [InlineData("LogLevel.Information", LogLevel.Information)]
        [InlineData("LogLevel.Warning", LogLevel.Warning)]
        [InlineData("LogLevel.Error", LogLevel.Error)]
        [InlineData("LogLevel.Critical", LogLevel.Critical)]
        public void ToLogLevel(string value, object expected)
        {
            var result = value.ToObject();

            Assert.Equal(expected, result);
        }
    }
}

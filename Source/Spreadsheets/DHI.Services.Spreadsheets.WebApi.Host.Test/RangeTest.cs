namespace DHI.Services.Spreadsheets.WebApi.Host.Test
{
    using System;
    using Xunit;
    using Range = WebApi.Range;

    public class RangeTest
    {
        [Fact]
        public void ParseInvalidThrows()
        {
            Assert.Throws<ArgumentException>(() => Range.Parse("A1:H5"));
            Assert.Throws<ArgumentException>(() => Range.Parse("R1C1:R123C45"));
        }

        [Fact]
        public void ParseIsOk()
        {
            var range = Range.Parse("R0C0,R123C45");
            Assert.Equal(0, range.UpperLeft.Row);
            Assert.Equal(0, range.UpperLeft.Col);
            Assert.Equal(123, range.LowerRight.Row);
            Assert.Equal(45, range.LowerRight.Col);
        }
    }
}
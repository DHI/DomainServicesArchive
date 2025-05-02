namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using Spreadsheets;
    using Xunit;
    using Range = Range;

    public class RangeTest
    {
        [Fact]
        public void CreateInvalidThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Range(new Cell(-1, -1), new Cell(5, 12)));
        }

        [Fact]
        public void SerializeIsOk()
        {
            var range = new Range(new Cell(0, 0), new Cell(123, 45));
            Assert.Equal("R0C0:R123C45", range.Serialize());
        }
    }
}
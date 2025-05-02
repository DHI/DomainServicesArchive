namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using Spreadsheets;
    using Xunit;

    public class CellTest
    {
        [Theory]
        [InlineData(-1, 10)]
        [InlineData(-1, -10)]
        [InlineData(1, -10)]
        public void CreateInvalidThrows(int row, int col)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Cell(row, col));
        }

        [Theory]
        [InlineData(123, 45, "R123C45")]
        [InlineData(34, 4343, "R34C4343")]
        public void SerializeIsOk(int row, int col, string cellDescriptor)
        {
            var cell = new Cell(row, col);
            Assert.Equal(cell.Serialize(), cellDescriptor);
        }
    }
}
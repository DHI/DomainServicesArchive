namespace DHI.Services.Spreadsheets.WebApi.Host.Test
{
    using System;
    using Xunit;

    public class CellTest
    {
        [Theory]
        [InlineData("A1")]
        [InlineData(@"R[0]C[0]")]
        public void ParseInvalidThrows(string cellDescriptor)
        {
            Assert.Throws<ArgumentException>(() => Cell.Parse(cellDescriptor));
        }

        [Theory]
        [InlineData("R123C45", 123, 45)]
        [InlineData("R0C0", 0, 0)]
        [InlineData("R888C999", 888, 999)]
        public void ParseIsOk(string cellDescriptor, int row, int col)
        {
            var cell = Cell.Parse(cellDescriptor);
            Assert.Equal(row, cell.Row);
            Assert.Equal(col, cell.Col);
        }
    }
}
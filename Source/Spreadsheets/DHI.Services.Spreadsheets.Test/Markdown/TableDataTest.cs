namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.Linq;
    using Xunit;

    public class TableDataTest
    {
        [Theory]
        [InlineData(-1, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        public void RowIndexOutOfRangeThrows(int row, int col)
        {
            var table = new TableElement()
                .WithHeaders("ColumnA", "ColumnB")
                .WithRows("1", "2");
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => table[row, col]);
            Assert.Contains("(Parameter 'row')", e.Message);
            Assert.Contains($"Invalid request for row index [{row}]. Must be non-negative and less than the number of rows: {table.Rows.Count}.", e.Message);
        }

        [Theory]
        [InlineData(0, -1)]
        [InlineData(0, 2)]
        [InlineData(0, 3)]
        public void ColumnIndexOutOfRangeThrows(int row, int col)
        {
            var table = new TableElement()
                .WithHeaders("ColumnA", "ColumnB")
                .WithRows("1", "2");
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => table[row, col]);
            Assert.Contains("(Parameter 'column')", e.Message);
            Assert.Contains($"Invalid request for column index [{col}]. Must be non-negative and less than the number of headers: {table.Headers.Count}.", e.Message);
        }

        [Theory]
        [InlineData(0, 0, 123)]
        [InlineData(0, 1, 456)]
        public void FindDataIsOk(int row, int col, object expected)
        {
            var document = new MarkdownDocument();
            var table = new TableElement()
                .WithHeaders("a", "b")
                .WithRows("123", "456");
            document.Elements.Add(table);

            var data = table.FindDataAsObject(row, col);

            Assert.Equal(expected, data);
        }

        [Theory]
        [InlineData(999, 1)]
        [InlineData(1, 999)]
        [InlineData(999, 999)]
        [InlineData(-1, -1)]
        public void FindDataOutOfRangeIsOk(int row, int col)
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("a", "b").WithRows("123", "456");
            document.Elements.Add(table);

            var data = table.FindDataAsObject(row, col);

            Assert.Null(data);
        }

        [Fact]
        public void GetDataIsOk()
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("ColumnA", "ColumnB", "ColumnC")
                .WithRows("1", "3", "5")
                .WithRows("2", "4", "6");
            document.Elements.Add(table);

            var data = table.GetDataAsObjects();

            Assert.Equal(3, data.GetLength(0));
            Assert.Equal(3, data.GetLength(1));
            Assert.Equal("ColumnA", data[0, 0]);
            Assert.Equal(1, data[1, 0]);
            Assert.Equal(2, data[2, 0]);
            Assert.Equal("ColumnB", data[0, 1]);
            Assert.Equal(3, data[1, 1]);
            Assert.Equal(4, data[2, 1]);
            Assert.Equal("ColumnC", data[0, 2]);
            Assert.Equal(5, data[1, 2]);
            Assert.Equal(6, data[2, 2]);
        }

        [Fact]
        public void GetRangeIsOk()
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("ColumnA", "ColumnB", "ColumnC")
                .WithRows("1", "3", "5")
                .WithRows("2", "4", "6");
            document.Elements.Add(table);

            var data = table.GetRangeAsObjects(0, 0, 1, 1);

            Assert.Equal(3, data.GetLength(0));
            Assert.Equal(2, data.GetLength(1));
            Assert.Equal("ColumnA", data[0, 0]);
            Assert.Equal(1, data[1, 0]);
            Assert.Equal(2, data[2, 0]);
            Assert.Equal("ColumnB", data[0, 1]);
            Assert.Equal(3, data[1, 1]);
            Assert.Equal(4, data[2, 1]);
        }

        [Fact]
        public void GetRangeByNameIsOk()
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("ColumnA", "ColumnB", "ColumnC")
                .WithRows("1", "3", "5")
                .WithRows("2", "4", "6");
            document.Elements.Add(table);

            var data = table.GetRangeAsObjects("ColumnA");

            Assert.Equal(3, data.GetLength(0));
            Assert.Equal(1, data.GetLength(1));
            Assert.Equal("ColumnA", data[0, 0]);
            Assert.Equal(1, data[1, 0]);
            Assert.Equal(2, data[2, 0]);
        }
    }
}

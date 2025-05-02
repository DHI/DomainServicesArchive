namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using System.Linq;
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using Xunit;

    public class TableTest
    {
        [Fact]
        public void ParseSimpleTableIsOk()
        {
            var str = @"
| a |
| - |
| 1 |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.NotNull(table);
            Assert.Equal(1, table?.Headers.Count);
            Assert.Equal(1, table?.Rows.Count);
            Assert.Equal("1", table?[0, 0]);
        }

        [Fact]
        public void BuildSimpleTableIsOk()
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("a").WithRows("1");
            document.Elements.Add(table);

            var str = MarkdownBuilder.Build(document);

            var expected = @"
|a|
|-|
|1|";

            Assert.NotNull(str);
            Assert.Equal(expected.TrimNewLineAtStart(), str);
        }


        [Fact]
        public void ParseTableDividerHasMoreElementsThanHeaderIsOk()
        {
            var str = @"
| a |
| - | - |
| 1 | 2 |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.Equal(2, table?.Headers.Count);
            Assert.Null(table?.Headers.Skip(1).First().Name);
        }

        [Fact]
        public void ParseTableHeaderHasMoreElementsThanDividerIsOk()
        {
            var str = @"
| a | b |
| - |
| 1 | 2 |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.Equal(2, table?.Headers.Count);
        }


        [Fact]
        public void ParseTableHeaderOnlyIsOk()
        {
            var str = @"
| a |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            var text = document.Elements.FirstOrDefault() as TextElement;
            Assert.Null(table);
            Assert.NotNull(text);
            Assert.Equal("| a |", text?.Text);
        }


        [Fact]
        public void ParseTableWithNoRowsIsOk()
        {
            var str = @"
| a |
| - |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.Equal(0, table?.Rows.Count);
        }


        [Fact]
        public void ParseWithNoSpaceIsOk()
        {
            var str = @"
| Column 1 | Column 2 |
|---|--|
||b|";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.Equal(2, table?.Headers.Count);
            Assert.Equal(1, table?.Rows.Count);

            Assert.Equal("", table[0, 0]);
            Assert.Equal("b", table[0, 1]);

        }


        [Fact]
        public void ParseWithNoEndPipeIsOk()
        {
            var str = @"
|Column 1|
|-|
|a";
            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;

            Assert.Equal("a", table[0, 0]);
        }


        [Fact]
        public void BuildTableWithNoRowsIsOk()
        {
            var document = new MarkdownDocument();
            var table = new TableElement().WithHeaders("a");
            document.Elements.Add(table);

            var str = MarkdownBuilder.Build(document);

            var expected = @"
|a|
|-|";

            Assert.NotNull(str);
            Assert.Equal(expected.TrimNewLineAtStart(), str);
        }


        [Fact]
        public void ParseTableHasJaggedCellValueIsOk()
        {
            var str = @"
| a | b
| - | - |
| 1 |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.NotNull(table);
            Assert.Null(table?[0, 1]);
        }

        [Fact]
        public void ParseTableWithoutDividerNotModelledIsOk()
        {
            var str = @"
| a |
| 1 |";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var element = document.Elements.FirstOrDefault() as TableElement;
            var text1 = document.Elements.FirstOrDefault() as TextElement;
            var text2 = document.Elements.Skip(1).FirstOrDefault() as TextElement;
            Assert.Null(element);
            Assert.NotNull(text1);
            Assert.Equal("| a |", text1?.Text);
            Assert.NotNull(text2);
            Assert.Equal("| 1 |", text2?.Text);
        }

        [Fact]
        public void BuildWithNullsIsOk()
        {
            var document = new MarkdownDocument()
                .WithTable(new TableElement().WithHeader(null).WithRow(null));

            var str = MarkdownBuilder.Build(document);

            var expected = @"
||
|-|
||";
            Assert.Equal(expected.TrimNewLineAtStart(), str);
        }

        // 2023-01-23: We are returning the escaped pipe as a pipe (without the escape character) as requested by Franz.
        [Fact]
        public void ParsePipeInACellIsOk()
        {
            var str = @"
|a|
|-|
|\||";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var table = document.Elements.FirstOrDefault() as TableElement;
            Assert.Equal(@"|", table?[0, 0]);
        }

        [Fact]
        public void BuildPipeInACellIsOk()
        {
            var document = new MarkdownDocument()
                .WithTable(new TableElement().WithHeaders("a").WithRows("|"));

            var str = MarkdownBuilder.Build(document);

            var expected = @"
|a|
|-|
|\||";
            Assert.Equal(expected.TrimNewLineAtStart(), str);
        }
    }
}

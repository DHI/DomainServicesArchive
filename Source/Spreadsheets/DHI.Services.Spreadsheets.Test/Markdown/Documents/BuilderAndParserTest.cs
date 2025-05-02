namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using System.Linq;
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using Xunit;

    public class BuilderAndParserTest
    {
        [Fact]
        public void ParseAndBuildIsOk()
        {
            var str = "# My Heading";
            var document = MarkdownParser.Parse(str);

            var result = MarkdownBuilder.Build(document);

            Assert.Equal(str, result);
        }

        [Fact]
        public void ParseHeadingAndTableIsOk()
        {
            var str = @"# My Heading

|a|
|-|
|1|";
            var document = MarkdownParser.Parse(str);

            var heading = document.Elements.FirstOrDefault() as HeadingElement;
            var table = document.Elements.Skip(1).FirstOrDefault() as TableElement;
            Assert.NotNull(heading);
            Assert.NotNull(table);
        }

        [Fact]
        public void BuildHeadingAndTableIsOk()
        {
            var document = new MarkdownDocument();
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading1, "My Heading"));
            document.Elements.Add(new TextElement(""));
            var table = new TableElement().WithHeaders("a").WithRows("1");
            document.Elements.Add(table);

            var str = MarkdownBuilder.Build(document);

            var expected = @"# My Heading

|a|
|-|
|1|";
            Assert.Equal(expected, str);
        }
    }
}
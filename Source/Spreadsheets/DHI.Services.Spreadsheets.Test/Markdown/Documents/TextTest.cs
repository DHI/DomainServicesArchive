namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using System.Linq;
    using Xunit;

    public class TextTest
    {
        [Fact]
        public void ParseTextIsOk()
        {
            var str = "This is just a bit of text.";

            var document = MarkdownParser.Parse(str);

            var text = document.Elements.FirstOrDefault() as TextElement;
            Assert.NotNull(text);
            Assert.Equal("This is just a bit of text.", text?.Text);
        }

        [Fact]
        public void BuildTextIsOk()
        {
            var document = new MarkdownDocument();
            document.Elements.Add(new TextElement("This is just a bit of text."));

            var str = MarkdownBuilder.Build(document);

            Assert.NotNull(str);
            Assert.Equal($"This is just a bit of text.", str);
        }
    }
}

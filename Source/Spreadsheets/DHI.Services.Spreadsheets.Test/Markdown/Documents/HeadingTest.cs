namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using System.Linq;
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using Xunit;

    public class HeadingTest
    {
        [Fact]
        public void ParseHeadingIsOk()
        {
            var document = MarkdownParser.Parse("# My Heading");

            var heading = document.Elements.First() as HeadingElement;

            Assert.NotNull(heading);
            Assert.Equal("My Heading", heading?.Title);
        }

        [Fact]
        public void BuildHeadingIsOk()
        {
            var document = new MarkdownDocument();
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading1, "My Heading"));

            var str = MarkdownBuilder.Build(document);

            Assert.NotNull(str);
            Assert.Equal("# My Heading", str);
        }

        [Fact]
        public void ParseAllHeadingsIsOk()
        {
            var str = @"
# My Heading 1
## My Heading 2
### My Heading 3
#### My Heading 4
##### My Heading 5
###### My Heading 6";

            var document = MarkdownParser.Parse(str.TrimNewLineAtStart());

            var heading1 = document.Elements.Skip(0).FirstOrDefault() as HeadingElement;
            var heading2 = document.Elements.Skip(1).FirstOrDefault() as HeadingElement;
            var heading3 = document.Elements.Skip(2).FirstOrDefault() as HeadingElement;
            var heading4 = document.Elements.Skip(3).FirstOrDefault() as HeadingElement;
            var heading5 = document.Elements.Skip(4).FirstOrDefault() as HeadingElement;
            var heading6 = document.Elements.Skip(5).FirstOrDefault() as HeadingElement;

            Assert.Equal("My Heading 1", heading1?.Title);
            Assert.Equal("My Heading 2", heading2?.Title);
            Assert.Equal("My Heading 3", heading3?.Title);
            Assert.Equal("My Heading 4", heading4?.Title);
            Assert.Equal("My Heading 5", heading5?.Title);
            Assert.Equal("My Heading 6", heading6?.Title);
        }

        [Fact]
        public void BuildAllHeadingsIsOk()
        {
            var document = new MarkdownDocument();
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading1, "My Heading 1"));
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading2, "My Heading 2"));
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading3, "My Heading 3"));
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading4, "My Heading 4"));
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading5, "My Heading 5"));
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading6, "My Heading 6"));

            var str = MarkdownBuilder.Build(document);

            var expected = @"# My Heading 1
## My Heading 2
### My Heading 3
#### My Heading 4
##### My Heading 5
###### My Heading 6";

            Assert.Equal(expected, str);
        }
    }
}

namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using Xunit;

    public class DocumentTest
    {
        [Fact]
        public void ContainsHeadingIsOk()
        {
            var document = new MarkdownDocument()
                .WithHeading("Test Heading")
                .WithTable(new TableElement().WithHeaders("a").WithRows("1"));

            var exists = document.ContainsHeading("Test Heading");

            Assert.True(exists);
        }

        [Fact]
        public void DoesntContainHeadingIsOk()
        {
            var document = new MarkdownDocument()
                .WithHeading("Test Heading")
                .WithTable(new TableElement().WithHeaders("a").WithRows("1"));

            var exists = document.ContainsHeading("This heading doesnt exist I hope");

            Assert.False(exists);
        }

        [Fact]
        public void FindTableIsOk()
        {
            var document = new MarkdownDocument()
                .WithHeading("Test Heading")
                .WithTable(new TableElement().WithHeaders("a").WithRows("1"));

            var table = document.FindTable("Test Heading");

            Assert.NotNull(table);
        }

        [Fact]
        public void FindTableThatDoesntExistIsOk()
        {
            var document = new MarkdownDocument()
                .WithHeading("Test Heading")
                .WithTable(new TableElement().WithHeaders("a").WithRows("1"));

            var table = document.FindTable("Wont be able to find the table with this heading...");

            Assert.Null(table);
        }

        [Fact]
        public void FindTableWhenHeadingEmptyIsOk()
        {
            var document = new MarkdownDocument()
                .WithHeading("Test Heading")
                .WithHeading("The Tables Heading")
                .WithTable(new TableElement().WithHeaders("a").WithRows("1"));

            var table = document.FindTable("Test Heading");

            Assert.Null(table);
        }
    }
}

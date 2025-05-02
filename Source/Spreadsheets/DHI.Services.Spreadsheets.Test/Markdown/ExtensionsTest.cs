namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using Xunit;

    public class ExtensionsTest
    {
        [Fact]
        public void SplitOnPipeIsOk()
        {
            var str = "| a | b |";

            var result = str.SplitOnPipe();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(" a ", result[0]);
            Assert.Equal(" b ", result[1]);
        }

        [Fact]
        public void SplitOnEscapedPipeIsOk()
        {
            var str = @"| \| | b |";

            var result = str.SplitOnPipe();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(" \\| ", result[0]);
            Assert.Equal(" b ", result[1]);
        }
    }
}

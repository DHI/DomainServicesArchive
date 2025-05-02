namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class RepositoryStreamTest : RepositoryTestBase
    {
        [Fact]
        public void AddStreamIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");

            var document = new MarkdownDocument()
                .WithHeading("Sheet 1")
                .WithTable(new TableElement().WithHeaders("ColA", "ColB").WithRows("a", "b"));

            var text = MarkdownBuilder.Build(document);

            // Act
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                repository.AddStream(id.ToString(), id.Name, id.Group, stream);
            }

            // Assert
            var sheet2 = repository.Get(id.ToString()).Value;
            Assert.NotNull(sheet2);

            Assert.True(sheet2.Metadata.ContainsKey("SheetNames"));
            Assert.Equal(new List<string> { "Sheet 1" }, (List<string>)sheet2.Metadata["SheetNames"]);

            Assert.NotNull(sheet2.Data.FirstOrDefault());
            Assert.Equal(new object[,] { { "ColA", "ColB" }, { "a", "b" } }, sheet2.Data.First());

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetStreamIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var id = new MarkdownId($"Test.md");

            Stream streamReference = null;
            try
            {
                var (stream, fileType, fileName) = repository.GetStream(id.ToString());
                streamReference = stream.Value;

                using var reader = new StreamReader(stream.Value);
                string text = reader.ReadToEnd();

                // Assert
                Assert.Equal(@"# Table 1
| Column 1 | Column 2 |
| - | - |
| a | b |
", text);
            }
            finally
            {
                streamReference.Dispose(); // Maybe we shouldn't put streams in tuples... :)
            }
        }
    }
}

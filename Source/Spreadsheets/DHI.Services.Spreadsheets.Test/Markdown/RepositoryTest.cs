namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Extensions;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class RepositoryTest : RepositoryTestBase
    {
        [Fact]
        public void AddIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] { 
                        { "ColA", "ColB" }, 
                        { "a", "b" } 
                    }
                );

            // Act
            repository.Add(spreadsheet);

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
        public void UpdateIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "x", "x" },
                        { "x", "x" }
                    }
                );
            repository.Add(spreadsheet);

            var data = spreadsheet.Data[0];
            data[0, 0] = "ColA";
            data[0, 1] = "ColB";
            data[1, 0] = 1; // Test to see it handles non-string values
            data[1, 1] = "2";

            // Act
            repository.Update(spreadsheet);

            // Assert
            var sheet2 = repository.Get(id.ToString()).Value;
            Assert.NotNull(sheet2);

            Assert.True(sheet2.Metadata.ContainsKey("SheetNames"));
            Assert.Equal(new List<string> { "Sheet 1" }, (List<string>)sheet2.Metadata["SheetNames"]);

            Assert.NotNull(sheet2.Data.FirstOrDefault());
            Assert.Equal(new object[,] { { "ColA", "ColB" }, { 1, 2 } }, sheet2.Data.First()); // Note: If it looks like a number it will convert it to a number.

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }


        [Fact]
        public void RemoveIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "ColA", "ColB" },
                        { "a", "b" }
                    }
                );
            repository.Add(spreadsheet);

            // Act
            repository.Remove(id.ToString());

            // Assert
            var sheet2 = repository.Get(id.ToString()).Value;
            Assert.Null(sheet2);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void CountOneIsOk()
        {
            // Arrange
            var folderPath = Path.Combine(GetDataFolder(), Guid.NewGuid().ToString());
            var repository = new MarkdownRepository(folderPath);
            var id = new MarkdownId($"Group_{Guid.NewGuid()}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "ColA", "ColB" },
                        { "a", "b" }
                    }
                );
            repository.Add(spreadsheet);

            // Act
            var count = repository.Count();

            // Assert
            Assert.Equal(1, count);

            // Cleanup
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void CountNoneThrows()
        {
            // Arrange
            var folderPath = Path.Combine(GetDataFolder(), Guid.NewGuid().ToString());
            var repository = new MarkdownRepository(folderPath);

            // Act
            Assert.Throws<DirectoryNotFoundException>(() => repository.Count());

            // Assert - Throws an exception
        }
    }
}

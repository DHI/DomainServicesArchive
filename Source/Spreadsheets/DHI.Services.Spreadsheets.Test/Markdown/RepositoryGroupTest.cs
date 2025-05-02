namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class RepositoryGroupTest : RepositoryTestBase
    {
        [Fact]
        public void GetByGroupIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            CreateSpreadsheet(repository, group);
            CreateSpreadsheet(repository, group);

            // Act
            var spreadsheets = repository.GetByGroup(group, null).ToList();

            // Assert
            Assert.Equal(2, spreadsheets.Count);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetFullNamesIsOkIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id1 = CreateSpreadsheet(repository, group);
            var id2 = CreateSpreadsheet(repository, group);

            // Act
            var fullNames = repository.GetFullNames(group).ToList();

            // Assert
            Assert.Contains(fullNames, x => x == id1.FullName);
            Assert.Contains(fullNames, x => x == id2.FullName);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void ContainsGroupIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            CreateSpreadsheet(repository, group);

            // Act
            var result = repository.ContainsGroup(group);

            // Assert
            Assert.True(result);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void RemoveByGroupIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            CreateSpreadsheet(repository, group);

            // Act
            repository.RemoveByGroup(group);

            // Assert
            var exits = repository.ContainsGroup(group);
            Assert.False(exits);

            // Cleanup - Nothing to cleanup
        }
    }
}

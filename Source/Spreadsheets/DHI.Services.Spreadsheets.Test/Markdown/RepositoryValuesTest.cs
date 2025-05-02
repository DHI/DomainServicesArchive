namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Extensions;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class RepositoryValuesTest : RepositoryTestBase
    {
        [Fact]
        public void GetFullNamesIsOk()
        {
            // Arrange
            var folderPath = Path.Combine(GetDataFolder(), $"Group_{Guid.NewGuid()}");
            Directory.CreateDirectory(folderPath);
            var repository = new MarkdownRepository(folderPath);

            var id1 = CreateSpreadsheet(repository);
            var id2 = CreateSpreadsheet(repository);

            // Act
            var fullNames = repository.GetFullNames().ToList();

            // Assert
            Assert.Contains(fullNames, x => x == id1.FullName);
            Assert.Contains(fullNames, x => x == id2.FullName);

            // Cleanup
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetAllIsOk()
        {
            // Arrange
            var folderPath = Path.Combine(GetDataFolder(), $"Group_{Guid.NewGuid()}");
            Directory.CreateDirectory(folderPath);
            var repository = new MarkdownRepository(folderPath);

            var id1 = CreateSpreadsheet(repository);
            var id2 = CreateSpreadsheet(repository);

            // Act
            var spreadsheets = repository.GetAll().ToList();

            // Assert
            Assert.Contains(spreadsheets, x => x.FullName == id1.FullName);
            Assert.Contains(spreadsheets, x => x.FullName == id2.FullName);

            // Cleanup
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetIdsIsOk()
        {
            // Arrange
            var folderPath = Path.Combine(GetDataFolder(), $"Group_{Guid.NewGuid()}");
            Directory.CreateDirectory(folderPath);
            var repository = new MarkdownRepository(folderPath);

            var id1 = CreateSpreadsheet(repository);
            var id2 = CreateSpreadsheet(repository);

            // Act
            var idStrings = repository.GetIds().ToList();

            // Assert
            Assert.Contains(idStrings, x => x == id1.FullName);
            Assert.Contains(idStrings, x => x == id2.FullName);

            // Cleanup
            Directory.Delete(folderPath, true);
        }


        [Fact]
        public void ContainsSheetIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{ Guid.NewGuid()}";
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
            var contains = repository.ContainsSheet(id.ToString(), "Sheet 1");

            // Assert
            Assert.True(contains);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetCellValueIsOk()
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
            var value = repository.GetCellValue(id.ToString(), "Sheet 1", new Cell(0, 1));
            var e = Assert.Throws<ArgumentException>(() => repository.GetRange(id.ToString(), "NonExistingSheet", new Spreadsheets.Range(new Cell(0, 0), new Cell(1, 1))));
            Assert.Contains("Invalid sheet name.", e.Message);

            // Assert
            Assert.Equal("b", value.Value);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetRangeIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "ColA", "ColB", "ColC" },
                        { "1", "4", "7" },
                        { "2", "5", "8" },
                        { "3", "6", "9" }
                    });

            repository.Add(spreadsheet);

            // Act
            var data = repository.GetRange(id.ToString(), "Sheet 1", new Spreadsheets.Range(new Cell(0, 0), new Cell(1, 1)));

            // Assert
            Assert.Equal(new object[,] {
                        { "ColA", "ColB" },
                        { 1, 4 },
                        { 2, 5 }
                    }, data.Value);
            var e = Assert.Throws<ArgumentException>(() => repository.GetRange(id.ToString(), "NonExistingSheet", new Spreadsheets.Range(new Cell(0, 0), new Cell(1, 1))));
            Assert.Contains("Invalid sheet name.", e.Message);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetUsedRangeIsOk()
        {
            // Arrange
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "ColA", "ColB", "" },
                        { "1", "3", "" },
                        { "2", "4", "" },
                        { "", "", "" }
                    });

            repository.Add(spreadsheet);

            // Act
            var data = repository.GetUsedRange(id.ToString(), "Sheet 1");
            var e = Assert.Throws<ArgumentException>(() => repository.GetUsedRange(id.ToString(), "NonExistingSheet"));
            Assert.Contains("Invalid sheet name.", e.Message);

            // Assert
            Assert.Equal(new object[,] {
                        { "ColA", "ColB" },
                        { 1, 3 },
                        { 2, 4 }
                    }, data.Value);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }

        [Fact]
        public void GetNamedRangeThrows()
        {
            var repository = new MarkdownRepository(GetDataFolder());
            var group = $"Group_{Guid.NewGuid()}";
            var id = new MarkdownId($"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}");
            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "A", "B", "C" },
                        { "1", "2", "3" }
                    });
            repository.Add(spreadsheet);

            Assert.Throws<NotSupportedException>(() => repository.GetNamedRange(id.ToString(), "Sheet 1", "A1"));
        }

        [Fact]
        public void GetUsedRangeFormatsIsOk()
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
                        { "1", "4" },
                        { "2", "5" }
                    });

            repository.Add(spreadsheet);

            // Act
            var data = repository.GetUsedRangeFormats(id.ToString(), "Sheet 1").Value;
            var e = Assert.Throws<ArgumentException>(() => repository.GetUsedRangeFormats(id.ToString(), "NonExistingSheet"));
            Assert.Contains("Invalid sheet name.", e.Message);

            // Assert
            Assert.NotNull(data);
            Assert.Equal(2, data.GetLength(0));
            Assert.Equal(2, data.GetLength(1));
            Assert.Equal(CellFormat.Text, data[0, 0]);
            Assert.Equal(CellFormat.Text, data[1, 0]);
            Assert.Equal(CellFormat.Text, data[0, 1]);
            Assert.Equal(CellFormat.Text, data[1, 1]);

            // Cleanup
            var folderPath = Path.Combine(GetDataFolder(), group);
            Directory.Delete(folderPath, true);
        }
    }
}

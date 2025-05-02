namespace DHI.Services.Spreadsheets.Test.Markdown
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Xunit;

    public class FileTest
    {
        /// <summary>Tests some of the provided sample data.</summary>
        [Fact]
        public void OpenIsOk()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePathName = Path.Combine(directory, "Data", "Test.md");

            var document = MarkdownFile.Open(filePathName);

            Assert.Equal(2, document.Elements.Count);
            var table = document.FindTable("Table 1");
            Assert.NotNull(table);
            Assert.Equal("Column 2", table.Headers[1].Name);
            Assert.Equal("b", table[0, 1]);
        }

        [Fact]
        public void OpenFormattedIsOk()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePathName = Path.Combine(directory, "Data", "DataImportPreProcessing.md");

            var document = MarkdownFile.Open(filePathName);

            Assert.Equal(4, document.Elements.Count);
            var table = document.FindTable("WeatherZone");
            Assert.NotNull(table);
            Assert.Equal("DestinationId", table.Headers[2].Name);
            Assert.Equal("/Forecast/WeatherZone/WindSpeed-mps-UTC/AdelaideOuterHarbour", table[2, 2]);
        }

        /// <summary>Tests some of the provided sample data.</summary>
        [Fact]
        public void OpenWithNoExtensionIsOk()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePathName = Path.Combine(directory, "Data", "Test");

            var document = MarkdownFile.Open(filePathName);

            Assert.Equal(2, document.Elements.Count);
        }

        [Fact]
        public void SaveIsOk()
        {
            // Arrange
            var tempFolder = $"Group_{Guid.NewGuid()}";
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var folderPath = Path.Combine(directory, "Data", tempFolder);
            var filePathName = Path.Combine(folderPath, "Test.md");

            Directory.CreateDirectory(folderPath);

            var document = new MarkdownDocument()
                .WithHeading("Table 1")
                .WithTable
                (
                    new TableElement().WithHeaders("Column 1", "Column 2", "Column 3")
                    .WithRows("1", "4", "7")
                    .WithRows("2", "5", "8")
                    .WithRows("3", "6", "9")
                )
                .WithHeading("Document Heading")
                .WithHeading("Table 2")
                .WithTable
                (
                    new TableElement().WithHeaders("Column 1", "Column 2", "Column 3")
                    .WithRows("a", "d", "g")
                    .WithRows("b", "e", "h")
                    .WithRows("c", "f", "i")
                );


            // Act
            MarkdownFile.Save(filePathName, document);

            // Assert
            var document2 = MarkdownFile.Open(filePathName);
            Assert.True(document2.ContainsHeading("Table 1"));
            Assert.True(document2.ContainsHeading("Table 2"));

            var table1 = document2.FindTable("Table 1");
            Assert.Equal(3, table1.Headers.Count);
            Assert.Equal(3, table1.Rows.Count);
            Assert.Equal(new List<string> { "1", "4", "7" }, table1.Rows[0].Columns);
            Assert.Equal(new List<string> { "2", "5", "8" }, table1.Rows[1].Columns);
            Assert.Equal(new List<string> { "3", "6", "9" }, table1.Rows[2].Columns);

            var table2 = document2.FindTable("Table 2");
            Assert.Equal(3, table2.Headers.Count);
            Assert.Equal(3, table2.Rows.Count);
            Assert.Equal(new List<string> { "a", "d", "g" }, table2.Rows[0].Columns);
            Assert.Equal(new List<string> { "b", "e", "h" }, table2.Rows[1].Columns);
            Assert.Equal(new List<string> { "c", "f", "i" }, table2.Rows[2].Columns);

            // Cleanup
            Directory.Delete(folderPath, true);
        }

        /// <summary>
        /// This test is to make sure we can always load and save a big document model in less than... Lets say a second.
        /// </summary>
        [Fact]
        public void PerformanceTestIsOk()
        {
            // Arrange
            const long MaxMilliseconds = 1000;
            const int TableCount = 100;
            const int ColumnCount = 20;
            const int RowCount = 100;

            var document = new MarkdownDocument();

            // Tables
            for (int i = 0; i < TableCount; i++)
            {
                document.WithHeading($"Heading {i}");

                // Columns
                var table = new TableElement();
                for (int c = 0; c < ColumnCount; c++)
                {
                    table.Headers.Add(new TableHeader($"Header {c}"));
                }

                // Rows
                for (int r = 0; r < RowCount; r++)
                {
                    var values = new List<string>();

                    for (int v = 0; v < ColumnCount; v++)
                    {
                        values.Add($"{Guid.NewGuid()}");
                    }
                    table.WithRows(values.ToArray());
                }
                document.WithTable(table);
            }

            var tempFolder = $"Group_{Guid.NewGuid()}";
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var folderPath = Path.Combine(directory, "Data", tempFolder);
            Directory.CreateDirectory(folderPath);
            var filePathName = Path.Combine(folderPath, "Test.md");

            // Act
            long tickStart = DateTime.UtcNow.Ticks;

            MarkdownFile.Save(filePathName, document);
            var document2 = MarkdownFile.Open(filePathName);

            long tickEnd = DateTime.UtcNow.Ticks;

            // Assert
            var timeSpan = TimeSpan.FromTicks(tickEnd - tickStart);
            Assert.True(timeSpan.Milliseconds < MaxMilliseconds); 
            Assert.Equal(TableCount, document2.GetHeadings().Count);
            Assert.Equal(TableCount, document2.GetTables().Count);

            var table2 = document2.FindTable("Heading 1");
            Assert.Equal(ColumnCount, table2.Headers.Count);
            Assert.Equal(RowCount, table2.Rows.Count);

            Debug.WriteLine("Time taken: {0}ms", timeSpan.Milliseconds);

            // Cleanup
            Directory.Delete(folderPath, true);
        }
    }
}

namespace DHI.Services.Spreadsheets.Test.Helpers
{
    using DHI.Services.Spreadsheets.Markdown;
    using DHI.Services.Spreadsheets.Test.Extensions;
    using System;
    using System.IO;

    public abstract class RepositoryTestBase
    {
        public static string GetDataFolder()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var dataFolder = Path.Combine(directory, "Data");
            return dataFolder;
        }

        internal static MarkdownId CreateSpreadsheet(MarkdownRepository repository, string group = null)
        {
            var filePathName = $"{Guid.NewGuid()}_Test{MarkdownId.FileExtension}";
            if (group != null)
            {
                filePathName = $"{group}/{Guid.NewGuid()}_Test{MarkdownId.FileExtension}";
            }
            var id = new MarkdownId(filePathName);

            var spreadsheet = new Spreadsheet<string>(id.ToString(), id.FileName, id.Group)
                .WithSheets("Sheet 1")
                .WithData(
                    new object[,] {
                        { "ColA", "ColB" },
                        { "a", "b" }
                    }
                );
            repository.Add(spreadsheet, null);

            return id;
        }

    }
}

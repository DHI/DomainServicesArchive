namespace DHI.Services.Spreadsheets.Markdown
{
    using System.IO;

    /// <summary>
    /// Provides file access to Markdown files.
    /// </summary>
    public class MarkdownFile
    {
        public static MarkdownDocument Open(string filePathName)
        {
            filePathName = ApplyExtension(filePathName);

            using var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using var reader = new StreamReader(fileStream);

            var text = reader.ReadToEnd();

            var document = MarkdownParser.Parse(text);

            return document;
        }

        public static void Save(string filePathName, MarkdownDocument document)
        {
            filePathName = ApplyExtension(filePathName);

            var text = MarkdownBuilder.Build(document);

            File.WriteAllText(filePathName, text);
        }

        public static bool Exists(string filePathName)
        {
            filePathName = ApplyExtension(filePathName);

            return File.Exists(filePathName);
        }

        /// <summary>Assures that the filename has a markdown extension. This is so people can call the repository without providing the extension in the identifier.</summary>
        public static string ApplyExtension(string filePathName)
        {
            if (!Path.HasExtension(filePathName) || !Path.GetExtension(filePathName).Equals(MarkdownId.FileExtension))
            {
                filePathName += MarkdownId.FileExtension;
            }

            return filePathName;
        }
    }
}
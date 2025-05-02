using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Spreadsheets.Test")]
namespace DHI.Services.Spreadsheets.Markdown
{
    using System;
    using System.IO;

    /// <summary>
    /// A clever way to define a path and filename. 
    /// </summary>
    // This was duplicated from the Spreadsheet Repository.
    internal class MarkdownId : BaseGroupedFileEntityId
    {
        
        public static string FileExtension = ".md";

        public MarkdownId(string relativeFilePath)
            : base(relativeFilePath)
        {
            if (Path.GetExtension(relativeFilePath) != FileExtension)
            {
                throw new ArgumentException($"The given file must be of type '*{FileExtension}'.", relativeFilePath);
            }
        }
    }
}

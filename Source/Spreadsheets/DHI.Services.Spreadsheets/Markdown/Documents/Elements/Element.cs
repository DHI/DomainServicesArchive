namespace DHI.Services.Spreadsheets.Markdown
{
    /// <summary>
    /// Represents a single thing of interest in markdown such as a Heading or a Table or just a block of Text.
    /// </summary>
    public abstract class Element
    {
        public MarkdownType Type { get; set; }


        protected Element(MarkdownType type)
        {
            Type = type;
        }
    }
}

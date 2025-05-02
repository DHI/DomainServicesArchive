namespace DHI.Services.Spreadsheets.Markdown
{
    public enum MarkdownHeadingType
    {
        Heading1 = MarkdownType.Heading1,
        Heading2 = MarkdownType.Heading2,
        Heading3 = MarkdownType.Heading3,
        Heading4 = MarkdownType.Heading4,
        Heading5 = MarkdownType.Heading5,
        Heading6 = MarkdownType.Heading6
    }

    /// <summary>
    /// This is a single heading or title in the markdown document.
    /// e.g. # This is my heading
    /// </summary>
    public class HeadingElement : Element
    {
        public string Title { get; set; }


        internal HeadingElement(MarkdownType markdownType, string title) : base(markdownType)
        {
            Title = title;
        }


        public HeadingElement(MarkdownHeadingType markdownHeadingType, string title) : base((MarkdownType)markdownHeadingType)
        {
            Title = title;
        }
    }
}

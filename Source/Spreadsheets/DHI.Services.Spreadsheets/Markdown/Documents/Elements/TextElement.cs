namespace DHI.Services.Spreadsheets.Markdown
{
    /// <summary>
    /// This is used as a placeholder for all the elements that are not a heading or a table in the markdown document.
    /// </summary>
    public class TextElement : Element
    {
        public string Text { get; set; }


        public TextElement(string text) : base(MarkdownType.Text)
        {
            Text = text;
        }
    }
}

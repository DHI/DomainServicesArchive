namespace DHI.Services.Spreadsheets.Markdown
{
    public enum TableHeaderAlignment
    {
        Left,
        Center,
        Right,
    }

    /// <summary>
    /// This is a single column header in a table. A table will have one of these for each column.
    /// </summary>
    public class TableHeader
    {
        public string Name { get; set; }

        public TableHeaderAlignment Alignment { get; set; }


        public TableHeader(string name, TableHeaderAlignment alignment = TableHeaderAlignment.Left)
        {
            Name = name;
            Alignment = alignment;
        }
    }
}

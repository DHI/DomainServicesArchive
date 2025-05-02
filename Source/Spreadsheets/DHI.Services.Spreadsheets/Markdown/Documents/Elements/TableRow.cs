namespace DHI.Services.Spreadsheets.Markdown
{
    using System.Collections.Generic;

    /// <summary>
    /// A single line within a table, it will have 1 or more column values.
    /// </summary>
    public class TableRow
    {
        public List<string> Columns { get; set; }


        public TableRow(List<string> columns)
        {
            Columns = columns;
        }
    }
}

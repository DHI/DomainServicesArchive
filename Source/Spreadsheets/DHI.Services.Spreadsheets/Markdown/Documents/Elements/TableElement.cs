namespace DHI.Services.Spreadsheets.Markdown
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This is a single table in the markdown document.
    /// The table may consist of a TableHeader and zero or more TableRows.
    /// e.g.
    /// | Name | Age |
    /// | John | 25  |
    /// | Mary | 24  |
    /// </summary>
    public class TableElement : Element
    {
        public List<TableHeader> Headers { get; set; }

        public List<TableRow> Rows { get; set; }

        public TableElement() : base(MarkdownType.Table)
        {
            Headers = new List<TableHeader>();
            Rows = new List<TableRow>();
        }

        public string this[int row, int column] => Data(row, column);

        private string Data(int row, int column)
        {
            if (Rows.Count <= row || 0 > row)
            {
                throw new ArgumentOutOfRangeException(nameof(row), $"Invalid request for row index [{row}]. Must be non-negative and less than the number of rows: {Rows.Count}.");
            }

            if (Headers.Count <= column || 0 > column)
            {
                throw new ArgumentOutOfRangeException(nameof(column), $"Invalid request for column index [{column}]. Must be non-negative and less than the number of headers: {Headers.Count}.");
            }

            return Rows[row].Columns.Count - 1 < column ? null : Rows[row].Columns[column];
        }
    }
}

namespace DHI.Services.Spreadsheets.Test.Helpers
{
    using DHI.Services.Spreadsheets.Markdown;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// These are fluent extension methods to make reading the tests easier on the eyes.
    /// </summary>
    internal static class DocumentExtensions
    {
        internal static MarkdownDocument WithHeading(this MarkdownDocument document, string heading)
        {
            document.Elements.Add(new HeadingElement(MarkdownHeadingType.Heading1, heading));

            return document;
        }

        internal static MarkdownDocument WithTable(this MarkdownDocument document, TableElement table)
        {
            document.Elements.Add(table);

            return document;
        }

        internal static TableElement WithHeader(this TableElement table, string header)
        {
            table.Headers.Add(new TableHeader(header));
            return table;
        }

        internal static TableElement WithHeaders(this TableElement table, params string[] headers)
        {
            foreach (var header in headers)
            {
                table.Headers.Add(new TableHeader(header));
            }

            return table;
        }

        internal static TableElement WithRow(this TableElement table, string column)
        {
            table.Rows.Add(new TableRow(new List<string> { column }));

            return table;
        }

        internal static TableElement WithRows(this TableElement table, params string[] columns)
        {
            table.Rows.Add(new TableRow(columns.ToList()));

            return table;
        }
    }
}

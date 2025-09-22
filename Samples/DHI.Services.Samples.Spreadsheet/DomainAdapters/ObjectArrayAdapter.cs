namespace DHI.Services.Samples.Spreadsheet.DomainAdapters
{
    using System;
    using System.Data;

    public static class ObjectArrayAdapter
    {
        /// <summary>Converts a 2D object array to a DataTable for easy WPF DataGrid binding.</summary>
        public static DataTable ToDataTable(object?[,]? cells)
        {
            var table = new DataTable();
            if (cells == null || cells.Length == 0) return table;

            var rows = cells.GetLength(0);
            var cols = cells.GetLength(1);

            for (int c = 0; c < cols; c++)
            {
                var header = cells[0, c]?.ToString();
                if (string.IsNullOrWhiteSpace(header)) header = $"C{c + 1}";
                if (table.Columns.Contains(header))
                    header = $"{header}_{c + 1}";
                table.Columns.Add(header);
            }

            for (int r = 1; r < rows; r++)
            {
                var dr = table.NewRow();
                for (int c = 0; c < cols; c++)
                {
                    dr[c] = cells[r, c] ?? DBNull.Value;
                }
                table.Rows.Add(dr);
            }

            return table;
        }

        /// <summary>Converts a 2D enum array to a DataTable of strings.</summary>
        public static DataTable ToDataTable<TEnum>(TEnum[,]? cells) where TEnum : struct, Enum
        {
            var table = new DataTable();
            if (cells == null || cells.Length == 0) return table;

            var rows = cells.GetLength(0);
            var cols = cells.GetLength(1);

            for (int c = 0; c < cols; c++)
                table.Columns.Add($"C{c + 1}");

            for (int r = 0; r < rows; r++)
            {
                var dr = table.NewRow();
                for (int c = 0; c < cols; c++)
                    dr[c] = cells[r, c].ToString() ?? "";
                table.Rows.Add(dr);
            }

            return table;
        }
    }
}

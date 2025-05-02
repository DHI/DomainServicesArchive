namespace DHI.Services.Spreadsheets
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A Sheet is a representation of a single sheet in a spreadsheet.
    ///     Each row is represented by a Dictionary, where the key is the column header.
    /// </summary>
    public class Sheet : List<Dictionary<string, object>>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Sheet" /> class.
        /// </summary>
        /// <param name="data">The object array data.</param>
        public Sheet(object[,] data)
        {
            for (var row = 1; row < data.GetLength(0); row++)
            {
                var dictionary = new Dictionary<string, object>();
                for (var col = 0; col < data.GetLength(1); col++)
                {
                    var name = data[0, col] == null ? string.Empty : data[0, col].ToString();
                    var value = data[row, col];
                    if (!string.IsNullOrEmpty(name) && value is not null && value as string != string.Empty)
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            throw new ArgumentException($"The column with name '{name}' has already been added to the column list.", nameof(data));
                        }

                        dictionary.Add(name, value);
                    }
                }

                if (dictionary.Count != 0)
                {
                    Add(dictionary);
                }
            }
        }

        /// <summary>
        ///     Gets the value for the given column header in the given row.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="header">The column header.</param>
        public object GetValue(int row, string header)
        {
            return this[row][header];
        }
    }
}
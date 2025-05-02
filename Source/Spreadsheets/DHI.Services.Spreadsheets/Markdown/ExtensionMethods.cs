namespace DHI.Services.Spreadsheets.Markdown
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the table cell's value or null if it is out of range.
        /// </summary>
        public static object FindDataAsObject(this TableElement table, int row, int column)
        {
            if (row < 0 || column < 0 || row >= table.Rows.Count || column >= table.Rows[row].Columns.Count)
            {
                return null;
            }

            return table[row, column].ToObject();
        }

        public static object[,] GetRangeAsObjects(this TableElement table, string headerName)
        {
            var columnIndex = table.Headers.FindIndex(x => String.Compare(x.Name, headerName, true) == 0);

            return table.GetRangeAsObjects(0, columnIndex, table.Rows.Count - 1, columnIndex);
        }

        /// <summary>
        /// Returns all the values of a table in an multi-dimensional array.
        /// Format: [row, column]
        /// </summary>
        public static object[,] GetDataAsObjects(this TableElement table)
        {
            var rowCount = table.Rows.Count;
            var columnCount = table.Headers.Count;
            var data = new object[rowCount + 1, columnCount];

            // Note we need to invert the multidimensional array's 'rows' and 'columns' 
            // as this is what the OpenXml repository was doing...
            for (var c = 0; c < table.Headers.Count; c++)
            {
                data[0, c] = table.Headers[c].Name;

                for (var r = 0; r < table.Rows.Count; r++)
                {
                    data[r + 1, c] = table.Rows[r].Columns[c].ToObject();
                }
            }

            return data;
        }

        /// <summary>
        /// Returns all the values of a table within the specified range.
        /// Format: [row, column]
        /// </summary>
        public static object[,] GetRangeAsObjects(this TableElement table, int row1, int column1, int row2, int column2)
        {
            var rowCount = (row2 - row1) + 1;
            var columnCount = (column2 - column1) + 1;
            var data = new object[rowCount + 1, columnCount];

            var rows = table.Rows.GetRange(row1, rowCount);

            var columnOffset = column1;
            for (var c = 0; c < columnCount; c++)
            {
                data[0, c] = table.Headers[c + columnOffset].Name;

                for (var r = 0; r < rows.Count; r++)
                {
                    data[r + 1, c] = rows[r].Columns[c + columnOffset].ToObject();
                }
            }

            return data;
        }

        /// <summary>
        ///     Converts a string representation of a value object to an object.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        public static object ToObject(this string stringValue)
        {
            // This function was copied from DomainServices/Source/Core/DHI.Services.WebApiCore/ExtensionMethods.cs
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                return intValue;
            }

            if (double.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue))
            {
                return doubleValue;
            }

            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            if (bool.TryParse(stringValue, out var boolValue))
            {
                return boolValue;
            }

            if (stringValue.StartsWith("LogLevel."))
            {
                if (Enum.TryParse(stringValue.Remove(0, "LogLevel.".Length), out LogLevel logLevel))
                {
                    return logLevel;
                }
            }

            return stringValue;
        }

        internal static Spreadsheet<string> ToSpreadsheet(this MarkdownDocument document, MarkdownId markdownId)
        {
            var tableNames = new List<string>();
            var datas = new List<object[,]>();

            string tableName = null;
            foreach (var element in document.Elements)
            {
                if (element is HeadingElement heading)
                {
                    tableName = heading.Title;
                }
                else if (element is TableElement table)
                {
                    tableNames.Add(tableName);
                    datas.Add(table.GetDataAsObjects());
                    tableName = null;
                }
                else
                {
                    // We don't really care about the other elements
                }
            }

            var spreadsheet = new Spreadsheet<string>(markdownId.FullName, markdownId.Name, markdownId.Group);
            spreadsheet.Metadata.Add("SheetNames", tableNames);
            spreadsheet.Metadata.Add("DefinedNames", new string[] { "values" }); // I don't really know, but this is what OpenXml populated from their sheets.

            foreach (var data in datas)
            {
                spreadsheet.Data.Add(data);
            }

            return spreadsheet;
        }

        internal static MarkdownDocument ToMarkdownDocument(this Spreadsheet<string> spreadsheet)
        {
            var document = new MarkdownDocument();
            List<string> tableNames = null;
            if (spreadsheet.Metadata.ContainsKey("SheetNames"))
            {
                tableNames = spreadsheet.Metadata["SheetNames"] as List<string>;
            }
            var datas = spreadsheet.Data;

            for (int i = 0; i < tableNames.Count; i++)
            {
                var tableName = tableNames[i];
                var data = datas[i];

                document.Elements.Add(new HeadingElement(MarkdownType.Heading1, tableName));

                var table = new TableElement();
                document.Elements.Add(table);

                var columnCount = data.GetLength(1);
                for (int c = 0; c < columnCount; c++)
                {
                    var columnName = "";

                    if (data[0, c] != null)
                    {
                        columnName = data[0, c].ToString();
                    }
                    table.Headers.Add(new TableHeader(columnName));
                }

                var rowCount = data.GetLength(0) - 1; // The first element in the row is the header
                for (int r = 0; r < rowCount; r++)
                {
                    var cols = new List<string>();
                    for (int c = 0; c < columnCount; c++)
                    {
                        if (data[r + 1, c] != null)
                        {
                            cols.Add(data[r + 1, c].ToString());
                        }
                        else
                        {
                            cols.Add("");
                        }
                    }

                    table.Rows.Add(new TableRow(cols));
                }
            }

            return document;
        }

        public static string TrimStartAndEndingPipes(this string str)
        {
            if (str == "|")
            {
                return "";
            }

            if (str.StartsWith("|") && str.EndsWith("|"))
            {
                return str.Substring(1, str.Length - 2);
            }
            if (str.StartsWith("|"))
            {
                return str.Remove(0, 1);
            }
            if (str.EndsWith("|"))
            {
                return str.Substring(0, str.Length - 1);
            }

            return str;
        }

        /// <summary>
        /// Splits a string on Pipe characters but ignores escaped pipes e.g. '\|'
        /// </summary>
        public static List<string> SplitOnPipe(this string str)
        {
            var contentString = str.TrimStartAndEndingPipes();
            var list = Regex.Split(contentString, @"(?<!\\)\|").ToList(); // Split on a pipe but not an escaped pipe.
            return list;
        }

        // Copied from DHI.Services.Provider.OpenXML.ExtensionMethods
        internal static object[,] RemoveEmptyColumns(this object[,] array)
        {
            var deleteCandidates = new List<int>();

            for (var c = 0; c < array.GetLength(1); c++)
            {
                for (var r = 0; r < array.GetLength(0); r++)
                {
                    if (array[r, c] != null && array[r, c].ToString() != string.Empty)
                    {
                        break;
                    }

                    if (r == array.GetLength(0) - 1)
                    {
                        deleteCandidates.Add(c);
                    }
                }
            }

            var minCol = -1;
            for (var c = array.GetLength(1) - 1; c >= 0; c--)
            {
                if (deleteCandidates.Contains(c))
                {
                    minCol = c;
                }
                else
                {
                    break;
                }
            }

            if (minCol == 0)
            {
                return new object[0, 0];
            }

            if (minCol > 0)
            {
                ////array = (object[,])ResizeArray(array, new int[] { array.GetLength(0), minCol });
                var newArray = new object[array.GetLength(0), minCol];
                for (var r = 0; r < array.GetLength(0); r++)
                {
                    for (var c = 0; c < minCol; c++)
                    {
                        newArray[r, c] = array[r, c];
                    }
                }

                return newArray;
            }

            return array;
        }

        // Copied from DHI.Services.Provider.OpenXML.ExtensionMethods
        internal static object[,] RemoveEmptyRows(this object[,] array)
        {
            var deleteCandidates = new List<int>();

            for (var r = 0; r < array.GetLength(0); r++)
            {
                for (var c = 0; c < array.GetLength(1); c++)
                {
                    if (array[r, c] != null && array[r, c].ToString() != string.Empty)
                    {
                        break;
                    }

                    if (c == array.GetLength(1) - 1)
                    {
                        deleteCandidates.Add(r);
                    }
                }
            }

            var minRow = -1;
            for (var r = array.GetLength(0) - 1; r >= 0; r--)
            {
                if (deleteCandidates.Contains(r))
                {
                    minRow = r;
                }
                else
                {
                    break;
                }
            }

            if (minRow == 0)
            {
                return new object[0, 0];
            }

            if (minRow > 0)
            {
                array = (object[,])ResizeArray(array, new[] { minRow, array.GetLength(1) });
            }

            return array;
        }

        // Copied from DHI.Services.Provider.OpenXML.ExtensionMethods
        public static Array ResizeArray(Array array, int[] newSizes)
        {
            if (newSizes.Length != array.Rank)
            {
                throw new ArgumentException("array must have the same number of dimensions as there are elements in newSizes", nameof(newSizes));
            }

            var temp = Array.CreateInstance(array.GetType().GetElementType(), newSizes);
            var length = array.Length <= temp.Length ? array.Length : temp.Length;
            Array.ConstrainedCopy(array, 0, temp, 0, length);
            return temp;
        }
    }
}

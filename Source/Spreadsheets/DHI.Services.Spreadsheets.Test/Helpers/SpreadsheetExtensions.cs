namespace DHI.Services.Spreadsheets.Test.Extensions
{
    using System.Linq;

    internal static class SpreadsheetExtensions
    {
        public static Spreadsheet<string> WithSheets(this Spreadsheet<string> spreadsheet, params string[] sheetNames)
        {
            spreadsheet.Metadata["SheetNames"] = sheetNames.ToList();

            return spreadsheet;
        }

        public static Spreadsheet<string> WithData(this Spreadsheet<string> spreadsheet, object[,] data)
        {
            spreadsheet.Data.Add(data);

            return spreadsheet;
        }
    }
}

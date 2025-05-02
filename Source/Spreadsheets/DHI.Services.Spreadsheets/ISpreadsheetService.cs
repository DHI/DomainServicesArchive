namespace DHI.Services.Spreadsheets
{
    using System.IO;
    using System.Security.Claims;

    public interface ISpreadsheetService<TId> : IService<Spreadsheet<TId>, TId>,
        IDiscreteService<Spreadsheet<TId>, TId>,
        IUpdatableService<Spreadsheet<TId>, TId>,
        IGroupedService<Spreadsheet<TId>>,
        IStreamableService<TId>
    {
        object GetCellValue(TId id, string sheetName, Cell cell, ClaimsPrincipal user = null);

        object[,] GetNamedRange(TId id, string sheetName, string rangeName, ClaimsPrincipal user = null);

        object[,] GetRange(TId id, string sheetName, Range range, ClaimsPrincipal user = null);

        object[,] GetUsedRange(TId id, string sheetName, ClaimsPrincipal user = null);

        CellFormat[,] GetUsedRangeFormats(TId id, string sheetName, ClaimsPrincipal user = null);

        bool SheetExists(TId id, string sheetName, ClaimsPrincipal user = null);

        void AddStream(TId id, string name, string group, Stream stream, ClaimsPrincipal user = null);
    }
}
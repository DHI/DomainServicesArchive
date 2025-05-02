namespace DHI.Services.Spreadsheets
{
    using System.IO;
    using System.Security.Claims;

    public interface ISpreadsheetRepository<TId> : IRepository<Spreadsheet<TId>, TId>,
        IDiscreteRepository<Spreadsheet<TId>, TId>,
        IUpdatableRepository<Spreadsheet<TId>, TId>,
        IGroupedRepository<Spreadsheet<TId>>,
        IStreamableRepository<TId>
    {
        Maybe<object> GetCellValue(TId id, string sheetName, Cell cell, ClaimsPrincipal user = null);

        Maybe<object[,]> GetNamedRange(TId id, string sheetName, string rangeName, ClaimsPrincipal user = null);

        Maybe<object[,]> GetRange(TId id, string sheetName, Range range, ClaimsPrincipal user = null);

        Maybe<object[,]> GetUsedRange(TId id, string sheetName, ClaimsPrincipal user = null);

        Maybe<CellFormat[,]> GetUsedRangeFormats(TId id, string sheetName, ClaimsPrincipal user = null);

        bool ContainsSheet(TId id, string sheetName, ClaimsPrincipal user = null);

        void AddStream(TId id, string name, string group, Stream stream, ClaimsPrincipal user = null);
    }
}
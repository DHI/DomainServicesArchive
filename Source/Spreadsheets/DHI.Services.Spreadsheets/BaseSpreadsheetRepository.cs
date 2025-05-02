namespace DHI.Services.Spreadsheets
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseSpreadsheetRepository<TId> : BaseDiscreteRepository<Spreadsheet<TId>, TId>, ISpreadsheetRepository<TId>
    {
        public abstract void Add(Spreadsheet<TId> entity, ClaimsPrincipal user = null);

        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        public abstract IEnumerable<Spreadsheet<TId>> GetByGroup(string group, ClaimsPrincipal user = null);

        public abstract Maybe<object> GetCellValue(TId id, string sheetName, Cell cell, ClaimsPrincipal user = null);

        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(s => s.FullName).ToArray();
        }

        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(s => s.FullName).ToArray();
        }

        public abstract Maybe<object[,]> GetNamedRange(TId id, string sheetName, string rangeName, ClaimsPrincipal user = null);

        public abstract Maybe<object[,]> GetRange(TId id, string sheetName, Range range, ClaimsPrincipal user = null);

        public abstract Maybe<object[,]> GetUsedRange(TId id, string sheetName, ClaimsPrincipal user = null);

        public abstract Maybe<CellFormat[,]> GetUsedRangeFormats(TId id, string sheetName, ClaimsPrincipal user = null);

        public abstract void Remove(TId id, ClaimsPrincipal user = null);

        public abstract bool ContainsSheet(TId id, string sheetName, ClaimsPrincipal user = null);

        public abstract void Update(Spreadsheet<TId> entity, ClaimsPrincipal user = null);

        public abstract void AddStream(TId id, string name, string group, Stream stream, ClaimsPrincipal user = null);

        public abstract (Maybe<Stream>, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user = null);
    }
}
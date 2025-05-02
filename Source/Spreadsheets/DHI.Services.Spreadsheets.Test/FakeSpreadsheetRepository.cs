namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Spreadsheets;
    using Range = Range;

    internal class FakeSpreadsheetRepository<TId> : FakeGroupedRepository<Spreadsheet<TId>, TId>, ISpreadsheetRepository<TId>, IGroupedUpdatableRepository
    {
        public FakeSpreadsheetRepository(List<Spreadsheet<TId>> spreadsheetList)
            : base(spreadsheetList)
        {
        }

        public Maybe<object> GetCellValue(TId id, string sheetName, Cell cell, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<object[,]> GetNamedRange(TId id, string sheetName, string name, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<object[,]> GetRange(TId id, string sheetName, Range range, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<object[,]> GetUsedRange(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<CellFormat[,]> GetUsedRangeFormats(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public bool ContainsSheet(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public void AddStream(TId id, string name, string group, Stream stream, ClaimsPrincipal user = null)
        {
            Add(new Spreadsheet<TId>(id, name, group));
        }

        public (Maybe<Stream>, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id);
            var stream = maybe.HasValue? new MemoryStream().ToMaybe<Stream>() : Maybe.Empty<Stream>();
            return (stream, string.Empty, string.Empty);
        }

        public void RemoveByGroup(string group, ClaimsPrincipal user = null)
        {
            var spreadsheets = GetByGroup(group).ToArray();
            foreach (var spreadsheet in spreadsheets)
            {
                Entities.Remove(spreadsheet.Id);
            }
        }
    }
}
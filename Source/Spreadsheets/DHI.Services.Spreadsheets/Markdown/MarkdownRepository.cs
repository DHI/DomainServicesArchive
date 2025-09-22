namespace DHI.Services.Spreadsheets.Markdown
{
    using Spreadsheets;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;

    // This was copied from the DHI.Services.Provider.OpenXML.SpreadsheetRepository then adapted for Markdowns
    public class MarkdownRepository : BaseSpreadsheetRepository<string>, IGroupedUpdatableRepository
    {
        private readonly string _rootFolder;

        public MarkdownRepository(string rootFolder)
        {
            Guard.Against.NullOrEmpty(rootFolder, nameof(rootFolder));

            _rootFolder = rootFolder.EndsWith("\\") ? rootFolder : rootFolder + "\\";
        }

        public void RemoveByGroup(string group, ClaimsPrincipal user = null)
        {
            Directory.Delete(Path.Combine(_rootFolder, group), true);
        }

        public override int Count(ClaimsPrincipal user = null)
        {
            return _GetSpreadsheetIds(_rootFolder).Count;
        }

        public override void Add(Spreadsheet<string> spreadsheet, ClaimsPrincipal user = null)
        {
            var path = spreadsheet.Group is null ? _rootFolder : Path.Combine(_rootFolder, spreadsheet.Group);
            Directory.CreateDirectory(path);

            var filePathName = Path.Combine(_rootFolder, spreadsheet.FullName);
            var document = spreadsheet.ToMarkdownDocument();

            MarkdownFile.Save(filePathName, document);
        }

        public override bool Contains(string id, ClaimsPrincipal user = null)
        {
            return _GetSpreadsheetIds(_rootFolder).Select(sid => sid.FullName).Contains(id);
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            var folder = Path.Combine(_rootFolder, group);
            return Directory.Exists(folder);
        }

        public override bool ContainsSheet(string id, string sheetName, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var document = MarkdownFile.Open(filePathName);

            return document.ContainsHeading(sheetName);
        }

        public override Maybe<Spreadsheet<string>> Get(string id, ClaimsPrincipal user = null)
        {
            var spreadsheet = _GetSpreadsheet(id);
            return spreadsheet?.ToMaybe() ?? Maybe.Empty<Spreadsheet<string>>();
        }

        public override IEnumerable<Spreadsheet<string>> GetAll(ClaimsPrincipal user = null)
        {
            return _GetSpreadsheets(_rootFolder);
        }

        public override IEnumerable<Spreadsheet<string>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            var folder = Path.Combine(_rootFolder, group);
            return _GetSpreadsheets(folder);
        }

        public override Maybe<object> GetCellValue(string id, string sheetName, Cell cell, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var document = MarkdownFile.Open(filePathName);

            var table = document.FindTable(sheetName);

            if (table == null)
            {
                throw new ArgumentException("Invalid sheet name.", nameof(sheetName));
            }

            var result = table.FindDataAsObject(cell.Row, cell.Col);

            return result.ToMaybe();
        }

        public override IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            var folder = Path.Combine(_rootFolder, group);
            var spreadsheetIds = _GetSpreadsheetIds(folder);
            return spreadsheetIds.Select(sid => sid.FullName).ToArray();
        }

        public override IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return _GetSpreadsheetIds(_rootFolder).Select(sid => sid.FullName).ToArray();
        }

        public override IEnumerable<string> GetIds(ClaimsPrincipal user = null)
        {
            return _GetSpreadsheetIds(_rootFolder).Select(sid => sid.FullName).ToArray();
        }

        // Note: Technically we don't have defined ranges in Markdown.
        public override Maybe<object[,]> GetNamedRange(string id, string sheetName, string rangeName, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("Markdown files don't have named ranges.");
        }

        public override Maybe<object[,]> GetRange(string id, string sheetName, DHI.Services.Spreadsheets.Range range, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var document = MarkdownFile.Open(filePathName);

            var table = document.FindTable(sheetName);
            if (table == null)
            {
                throw new ArgumentException("Invalid sheet name.", nameof(sheetName));
            }

            var values = table.GetRangeAsObjects(range.UpperLeft.Row, range.UpperLeft.Col, range.LowerRight.Row, range.LowerRight.Col);

            return values.ToMaybe();
        }

        public override Maybe<object[,]> GetUsedRange(string id, string sheetName, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var document = MarkdownFile.Open(filePathName);

            var table = document.FindTable(sheetName);
            if (table == null)
            {
                throw new ArgumentException("Invalid sheet name.", nameof(sheetName));
            }

            var values = table.GetDataAsObjects();

            return values.RemoveEmptyRows().RemoveEmptyColumns().ToMaybe();
        }

        public override Maybe<CellFormat[,]> GetUsedRangeFormats(string id, string sheetName, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var document = MarkdownFile.Open(filePathName);

            var table = document.FindTable(sheetName);

            if (table == null)
            {
                throw new ArgumentException("Invalid sheet name.", nameof(sheetName));
            }

            // It's strings all the way down
            var cellFormats = new CellFormat[table.Rows.Count, table.Headers.Count];

            for (int r = 0; r < table.Rows.Count; r++)
            {
                for (int c = 0; c < table.Headers.Count; c++)
                {
                    cellFormats[r, c] = CellFormat.Text;
                }
            }

            return cellFormats.ToMaybe();
        }

        public override void Remove(string id, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            filePathName = MarkdownFile.ApplyExtension(filePathName);
            File.Delete(filePathName);
        }

        public override void Update(Spreadsheet<string> spreadsheet, ClaimsPrincipal user = null)
        {
            Remove(spreadsheet.Id, user);
            Add(spreadsheet, user);
        }

        public override void AddStream(string id, string name, string group, Stream stream, ClaimsPrincipal user = null)
        {
            var filePathName = Path.Combine(_rootFolder, id);
            var folderPath = Path.GetDirectoryName(filePathName);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(_rootFolder, id);
            filePath = MarkdownFile.ApplyExtension(filePath);

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            File.WriteAllBytes(filePath, memoryStream.ToArray());
        }

        public override (Maybe<Stream>, string fileType, string fileName) GetStream(string id, ClaimsPrincipal user = null)
        {
            var filePath = Path.Combine(_rootFolder, id);
            filePath = MarkdownFile.ApplyExtension(filePath);

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return (Maybe.Empty<Stream>(), fileInfo.Extension.Substring(1), fileInfo.FullName);
            }
            return (new Maybe<Stream>(new MemoryStream(File.ReadAllBytes(filePath))), fileInfo.Extension.Substring(1), fileInfo.Name);
        }

        private Spreadsheet<string> _GetSpreadsheet(string id)
        {
            var markdownId = new MarkdownId(id);
            var filePathName = Path.Combine(_rootFolder, id);
            if (!MarkdownFile.Exists(filePathName))
            {
                return null;
            }

            var document = MarkdownFile.Open(filePathName);

            var spreadsheet = document.ToSpreadsheet(markdownId);

            return spreadsheet;
        }

        private List<MarkdownId> _GetSpreadsheetIds(string folder)
        {
            var spreadsheetIdList = new List<MarkdownId>();
            var files = Directory.GetFiles(folder, $"*{MarkdownId.FileExtension}", SearchOption.AllDirectories);
            var uriRoot = new Uri(_rootFolder);

            foreach (var filePath in files)
            {
                var uri = new Uri(filePath);
                var relativeUri = uriRoot.MakeRelativeUri(uri);
                var spreadsheetId = new MarkdownId(WebUtility.UrlDecode(relativeUri.ToString()));
                spreadsheetIdList.Add(spreadsheetId);
            }

            return spreadsheetIdList;
        }

        private List<Spreadsheet<string>> _GetSpreadsheets(string folder)
        {
            var spreadsheetIds = _GetSpreadsheetIds(folder);
            return spreadsheetIds.Select(spreadsheetId => _GetSpreadsheet(spreadsheetId.FullName)).ToList();
        }
    }
}
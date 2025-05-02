namespace DHI.Services.Spreadsheets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;

    public class SpreadsheetService<TId> : BaseGroupedUpdatableDiscreteService<Spreadsheet<TId>, TId>, ISpreadsheetService<TId>
    {
        private readonly ISpreadsheetRepository<TId> _repository;

        public SpreadsheetService(ISpreadsheetRepository<TId> repository)
            : base(repository)
        {
            _repository = repository;
        }
        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<ISpreadsheetRepository<TId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<ISpreadsheetRepository<TId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<ISpreadsheetRepository<TId>>(path, searchPattern);
        }

        public object GetCellValue(TId id, string sheetName, Cell cell, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetCellValue(id, sheetName, cell, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        public object[,] GetNamedRange(TId id, string sheetName, string rangeName, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetNamedRange(id, sheetName, rangeName, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        public object[,] GetRange(TId id, string sheetName, Range range, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetRange(id, sheetName, range, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        public object[,] GetUsedRange(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetUsedRange(id, sheetName, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        public CellFormat[,] GetUsedRangeFormats(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetUsedRangeFormats(id, sheetName, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        public bool SheetExists(TId id, string sheetName, ClaimsPrincipal user = null)
        {
            if (!Exists(id))
            {
                throw new KeyNotFoundException($"The spreadsheet with id '{id}' was not found.");
            }

            return _repository.ContainsSheet(id, sheetName, user);
        }

        public void AddStream(TId id, string name, string group, Stream stream, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(id, user))
            {
                var spreadsheet = new Spreadsheet<TId>(id, name, group);
                var cancelEventArgs = new CancelEventArgs<Spreadsheet<TId>>(spreadsheet);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.AddStream(id, name, group, stream, user);
                    OnAdded(spreadsheet);
                }
            }
            else
            {
                throw new ArgumentException($"'{typeof(Spreadsheet<TId>)}' with id '{id}' already exists.");
            }
        }
        public (Stream, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user=null)
        {
            var (maybe, fileType, fileName) = _repository.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"'{typeof(Spreadsheet<TId>)}' with id '{id}' was not found.");
            }

            return (maybe.Value, fileType, fileName);
        }
    }

    public class SpreadsheetService : SpreadsheetService<string>
    {
        public SpreadsheetService(ISpreadsheetRepository<string> repository)
            : base(repository)
        {
        }
    }
}
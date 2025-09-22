namespace DHI.Services.Samples.Spreadsheet.Composition
{
    using System;
    using System.IO;
    using DHI.Services.Spreadsheets;

    /// <summary>Composition root that constructs services with the real repositories.</summary>
    public static class CompositionRoot
    {
        /// <summary>
        /// Create a SpreadsheetService from either a folder OR a single .xlsx file path.
        /// If a single file is provided, <paramref name="singleFileId"/> will be the relative id (usually just the file name).
        /// </summary>
        public static SpreadsheetService<string> CreateSpreadsheetService(
            string repositoryTypeName,
            string? rootOrFile,
            out string? singleFileId)
        {
            var repoType = ResolveType(repositoryTypeName);

            var (rootFolder, id) = NormalizeRootOrFile(rootOrFile);
            EnsureFolder(rootFolder);
            singleFileId = id;

            var repo = (ISpreadsheetRepository<string>)Activator.CreateInstance(repoType, rootFolder)!;
            return new SpreadsheetService<string>(repo);
        }

        public static Type[] GetSpreadsheetRepositoryTypes() =>
            SpreadsheetService<string>.GetRepositoryTypes();

        // -------- helpers --------
        private static string BaseDir => AppContext.BaseDirectory;

        private static void EnsureFolder(string folder)
        {
            var abs = Path.GetFullPath(folder);
            if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);
        }

        /// <summary>Resolve "Namespace.Type, Assembly" or full name across loaded assemblies.</summary>
        private static Type ResolveType(string typeName)
        {
            var t = Type.GetType(typeName, throwOnError: false);
            if (t != null) return t;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(typeName, throwOnError: false);
                if (t != null) return t;
            }

            throw new TypeLoadException(
                $"Could not resolve type '{typeName}'. Ensure the provider assembly is referenced and the name is correct.");
        }

        /// <summary>
        /// Turns a possibly-relative folder/file path into absolute.
        /// If empty, defaults to BaseDir\App_Data\Spreadsheets (folder mode).
        /// If the path is an existing .xlsx file, returns (rootFolder = file's directory, singleFileId = file name).
        /// Otherwise returns (rootFolder = resolved folder, singleFileId = null).
        /// </summary>
        public static (string rootFolder, string? singleFileId) NormalizeRootOrFile(string? path)
        {
            var defaultFolder = Path.Combine(BaseDir, "App_Data", "Spreadsheets");

            if (string.IsNullOrWhiteSpace(path) || path == "." || path == ".\\" || path == "./")
                return (Path.GetFullPath(defaultFolder), null);

            var abs = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(BaseDir, path));

            if (File.Exists(abs) && string.Equals(Path.GetExtension(abs), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                var root = Path.GetDirectoryName(abs)!;
                var id = Path.GetFileName(abs);
                return (root, id);
            }

            return (abs, null);
        }
    }
}

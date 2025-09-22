namespace DHI.Services.Documents
{
    using DHI.Services.Documents.Converters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    public class FileDocumentRepository : BaseGroupedDocumentRepository<string>
    {
        private readonly string _rootDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileDocumentRepository(string rootDirectory)
        {
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            Directory.CreateDirectory(_rootDirectory);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new DocumentConverter<string>() }
            };
        }

        public override void Add(Stream stream, string id, Parameters metadata, ClaimsPrincipal user = null)
        {
            var fullName = FullName.Parse(id);
            var filePath = GetFilePath(fullName);
            var metadataPath = filePath + ".metadata.json";

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var fileStream = File.Create(filePath);
            stream.CopyTo(fileStream);

            var meta = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            File.WriteAllText(metadataPath, JsonSerializer.Serialize(meta, _jsonOptions));
        }

        public override (Stream stream, string fileType, string fileName) Get(string id, ClaimsPrincipal user = null)
        {
            var fullName = FullName.Parse(id);
            var filePath = GetFilePath(fullName);

            if (!File.Exists(filePath))
                return (null, null, null);

            var extension = Path.GetExtension(filePath).TrimStart('.');
            return (File.OpenRead(filePath), extension, Path.GetFileName(filePath));
        }

        public override void Remove(string id, ClaimsPrincipal user = null)
        {
            var fullName = FullName.Parse(id);
            var filePath = GetFilePath(fullName);
            var metadataPath = filePath + ".metadata.json";

            if (File.Exists(filePath))
                File.Delete(filePath);
            if (File.Exists(metadataPath))
                File.Delete(metadataPath);
        }

        public override IDictionary<string, string> GetMetadata(string id, ClaimsPrincipal user = null)
        {
            var fullName = FullName.Parse(id);
            var metadataPath = GetFilePath(fullName) + ".metadata.json";

            if (!File.Exists(metadataPath))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(metadataPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions);
        }

        public override IEnumerable<string> GetIds(ClaimsPrincipal user = null)
        {
            return Directory.EnumerateFiles(_rootDirectory, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".metadata.json"))
                .Select(f => ToIdFromPath(f));
        }

        public override bool Contains(string id, ClaimsPrincipal user = null)
        {
            var path = GetFilePath(FullName.Parse(id));
            return File.Exists(path);
        }

        public override int Count(ClaimsPrincipal user = null)
        {
            return GetIds(user).Count();
        }

        public override IDictionary<string, IDictionary<string, string>> GetAllMetadata(ClaimsPrincipal user = null)
        {
            var result = new Dictionary<string, IDictionary<string, string>>();
            foreach (var id in GetIds(user))
            {
                result[id] = GetMetadata(id, user);
            }
            return result;
        }

        public override IDictionary<string, IDictionary<string, string>> GetMetadataByFilter(string filter, Parameters parameters = null, ClaimsPrincipal user = null)
        {
            var all = GetAllMetadata(user);
            if (string.IsNullOrWhiteSpace(filter)) return all;

            var tokens = filter.ToLowerInvariant().Split(' ');
            return all
                .Where(kvp =>
                    tokens.All(t => kvp.Value.Values.Any(v => v != null && v.ToLowerInvariant().Contains(t))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public override IEnumerable<Document<string>> GetAll(ClaimsPrincipal user = null)
        {
            return GetIds(user).Select(id =>
            {
                var fullName = FullName.Parse(id);
                var doc = new Document<string>(id, fullName.Name, fullName.Group);
                foreach (var kvp in GetMetadata(id, user))
                    doc.Metadata.Add(kvp.Key, kvp.Value);
                return doc;
            });
        }

        public override IEnumerable<Document<string>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            return GetAll(user).Where(d => d.Group == group);
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            var dir = Path.Combine(_rootDirectory, group.Replace("/", Path.DirectorySeparatorChar.ToString()));
            return Directory.Exists(dir);
        }

        private string GetFilePath(FullName fullName)
        {
            var safePath = Path.Combine(_rootDirectory, fullName.Group ?? "", fullName.Name);
            return safePath;
        }

        private string ToIdFromPath(string fullPath)
        {
            var relative = GetRelativePath(_rootDirectory, fullPath);
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        private string GetRelativePath(string relativeTo, string path)
        {
            var fromUri = new Uri(AppendDirectorySeparatorChar(relativeTo));
            var toUri = new Uri(path);

            if (fromUri.Scheme != toUri.Scheme) { return path; }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return toUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase)
                ? relativePath.Replace('/', Path.DirectorySeparatorChar)
                : relativePath;
        }

        private string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;
            return path;
        }

    }
}

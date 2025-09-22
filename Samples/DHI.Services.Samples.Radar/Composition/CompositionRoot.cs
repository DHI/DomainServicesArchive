namespace DHI.Services.Samples.Radar.Composition
{
    using DHI.Services.Rasters;
    using DHI.Services.Rasters.Radar;
    using DHI.Services.Rasters.Radar.DELIMITEDASCII;
    using DHI.Services.Rasters.Zones;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Serialization;

    /// <summary>Composition root that constructs services with the real repositories.</summary>
    public static class CompositionRoot
    {
        // ----- RADAR -----
        public static RadarImageService<AsciiImage> CreateRadarService(string repositoryTypeName, string connectionString)
        {
            var repoType = ResolveType(repositoryTypeName);
            var normalized = NormalizeRadarConnectionString(connectionString);
            var repo = (IRasterRepository<AsciiImage>)Activator.CreateInstance(repoType, normalized)!;
            return new RadarImageService<AsciiImage>(repo);
        }

        public static Type[] GetRadarRepositoryTypes() =>
            RadarImageService<AsciiImage>.GetRepositoryTypes();

        // ----- ZONES -----
        public static ZoneService CreateZoneService(string repositoryTypeName, string filePath)
        {
            var normalizedJsonPath = ToAbsolutePathOrDefault(filePath, Path.Combine("App_Data", "zones.json"));
            EnsureAppData(normalizedJsonPath);

            var repoType = ResolveType(repositoryTypeName);
            var convs = new List<JsonConverter>
            {
                new DHI.Services.Rasters.Zones.Converters.ZoneDictionaryConverter(),
                new ZoneConverter(),
                new ZoneTypeConverter(),
            };

            var ctorWithConverters = repoType.GetConstructor(new[] { typeof(string), typeof(IEnumerable<JsonConverter>) });
            object repo = ctorWithConverters != null
                ? ctorWithConverters.Invoke(new object[] { normalizedJsonPath, convs })
                : Activator.CreateInstance(repoType, normalizedJsonPath)
                    ?? throw new InvalidOperationException("Could not create ZoneRepository.");

            return new ZoneService((IZoneRepository)repo);
        }

        public static Type[] GetZoneRepositoryTypes() => ZoneService.GetRepositoryTypes();

        // ----- Helpers -----

        private static string BaseDir => AppContext.BaseDirectory;

        /// <summary>Turns a possibly-relative path into an absolute path; if empty, uses a default under BaseDir.</summary>
        private static string ToAbsolutePathOrDefault(string? path, string defaultRelative)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "." || path == ".\\" || path == "./")
                return Path.GetFullPath(Path.Combine(BaseDir, defaultRelative));

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            return Path.GetFullPath(Path.Combine(BaseDir, path));
        }

        /// <summary>
        /// Normalizes the radar connection string:
        /// - resolves the folder (first segment) to an absolute path (default: App_Data\RadarImages)
        /// - keeps filePattern (middle segment) and dateTimeFormat (last segment) as-is
        /// - tolerates extra semicolons in the middle (they’re joined back for the pattern)
        /// </summary>
        private static string NormalizeRadarConnectionString(string? cs)
        {
            var defaultFolderRel = Path.Combine("App_Data", "RadarImages");
            const string defaultPattern = "PM_{datetimeFormat}.txt";
            const string defaultFormat = "yyyyMMddHH_$$$";

            if (string.IsNullOrWhiteSpace(cs))
            {
                var absDefaultFolder = ToAbsolutePathOrDefault(null, defaultFolderRel);
                return $"{absDefaultFolder};{defaultPattern};{defaultFormat}";
            }

            var parts = cs.Split(';');
            if (parts.Length < 3)
                throw new ArgumentException(
                    $"Radar connection string must have 3 parts: 'folder;filePattern;dateTimeFormat'. Got '{cs}'");

            var folder = parts[0];
            var format = parts[^1];
            var pattern = string.Join(";", parts.Skip(1).Take(parts.Length - 2));

            var absFolder = ToAbsolutePathOrDefault(folder, defaultFolderRel);
            return $"{absFolder};{pattern};{format}";
        }

        private static void EnsureAppData(string jsonPath)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(jsonPath))!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(jsonPath))
            {
                // zones repository expects dictionary shape
                File.WriteAllText(jsonPath, "{}");
                return;
            }

            // If someone left an old array file around, convert it once to dictionary by Id
            var text = File.ReadAllText(jsonPath).Trim();
            if (text.StartsWith("["))
            {
                var options = new System.Text.Json.JsonSerializerOptions();
                options.Converters.Add(new ZoneConverter());
                options.Converters.Add(new ZoneTypeConverter());

                var list = System.Text.Json.JsonSerializer
                    .Deserialize<List<Zone>>(text, options) ?? new();

                var dict = list
                    .Where(z => !string.IsNullOrWhiteSpace(z.Id))
                    .ToDictionary(z => z.Id, z => z, StringComparer.OrdinalIgnoreCase);

                var json = System.Text.Json.JsonSerializer.Serialize(dict, options);
                File.WriteAllText(jsonPath, json);
            }
        }

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
                $"Could not resolve type '{typeName}'. Ensure the assembly is referenced and the name is assembly-qualified if necessary.");
        }
    }
}

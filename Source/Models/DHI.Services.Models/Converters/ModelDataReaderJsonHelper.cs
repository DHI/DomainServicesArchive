namespace DHI.Services.Models.Converters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;

    public static class ModelDataReaderJsonHelper
    {
        public static void AutoRegisterTypesFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object)
                    continue;

                if (!property.Value.TryGetProperty("TypeName", out var typeNameProp) ||
                    !property.Value.TryGetProperty("$type", out var wrapperTypeProp))
                {
                    Console.WriteLine($"Skipping '{property.Name}' — missing TypeName or $type.");
                    continue;
                }

                var typeName = typeNameProp.GetString();
                var wrapperTypeString = wrapperTypeProp.GetString();

                if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(wrapperTypeString))
                {
                    Console.WriteLine($"Invalid type strings for '{property.Name}'.");
                    continue;
                }

                var innerTypeName = ExtractInnerTypeName(wrapperTypeString);
                var assemblyName = ExtractAssemblyNameFromTypeString(wrapperTypeString);

                if (string.IsNullOrWhiteSpace(innerTypeName) || string.IsNullOrWhiteSpace(assemblyName))
                {
                    Console.WriteLine($"Could not parse inner type or assembly from: {wrapperTypeString}");
                    continue;
                }

                if (!IsAssemblyLoaded(assemblyName))
                {
                    try
                    {
                        Assembly.Load(assemblyName);
                        Console.WriteLine($"Loaded assembly: {assemblyName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load assembly '{assemblyName}': {ex.Message}");
                        continue;
                    }
                }

                var modelType = TryResolveType(innerTypeName);
                if (modelType == null)
                {
                    Console.WriteLine($"Could not resolve type: '{innerTypeName}'");
                    continue;
                }

                try
                {
                    var wrapperType = typeof(ModelDataReader<>).MakeGenericType(modelType);
                    ModelDataReaderTypeRegistry.Register(typeName, wrapperType);
                    Console.WriteLine($"Registered: {typeName} => {wrapperType.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to register wrapper type for '{innerTypeName}': {ex.Message}");
                }
            }
        }

        internal static string? ExtractInnerTypeName(string fullTypeString)
        {
            var start = fullTypeString.IndexOf("[[", StringComparison.Ordinal);
            var end = fullTypeString.IndexOf(",", start, StringComparison.Ordinal);

            if (start < 0 || end < 0 || end <= start + 2)
                return null;

            return fullTypeString.Substring(start + 2, end - (start + 2)).Trim();
        }

        internal static string? ExtractAssemblyNameFromTypeString(string fullTypeString)
        {
            var start = fullTypeString.IndexOf("[[", StringComparison.Ordinal);
            var end = fullTypeString.IndexOf("]]", StringComparison.Ordinal);

            if (start == -1 || end == -1 || end <= start)
                return null;

            var inner = fullTypeString.Substring(start + 2, end - start - 2);
            var parts = inner.Split(',');
            return parts.Length >= 2 ? parts[1].Trim() : null;
        }

        internal static bool IsAssemblyLoaded(string assemblyName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(a => a.GetName().Name == assemblyName);
        }

        internal static Type? TryResolveType(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var match = asm.GetTypes().FirstOrDefault(t => t.FullName == typeName);
                    if (match != null) return match;
                }
                catch
                {
                    // Some assemblies may not allow GetTypes
                }
            }

            return null;
        }
    }
}

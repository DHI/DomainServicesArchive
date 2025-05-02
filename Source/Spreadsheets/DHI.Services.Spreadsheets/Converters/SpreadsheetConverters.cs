namespace DHI.Services.Spreadsheets.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class SpreadsheetConverter<TId> : JsonConverter<Spreadsheet<TId>>
    {
        public override Spreadsheet<TId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.Null or not JsonTokenType.None and JsonTokenType.StartObject)
            {

                if (JsonDocument.TryParseValue(ref reader, out var doc))
                {
                    var root = doc.RootElement;

                    TId? id = default;
                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Spreadsheet<TId>)}' required non-null '{nameof(Spreadsheet<TId>.Id)}' property");
                        }
                        id = idElement.Deserialize<TId>();
                    }

                    string? name = string.Empty;
                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Name), out var nameElement))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Spreadsheet<TId>)}' required non-null '{nameof(Spreadsheet<TId>.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string? group = null;
                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Group), out var groupElement))
                    {
                        group = groupElement.GetString();
                    }

                    var spreadsheet = new Spreadsheet<TId>(id!, name!, group);

                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Data), out var dataElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = {
                                new TwoDimensionalArrayConverter<object>(),
                                new DoubleConverter(),
                                new ObjectToInferredTypeConverter()
                            }
                        };
                        var datas = dataElement.Deserialize<List<object[,]>>(serializer) ?? new List<object[,]>();
                        foreach (var item in datas)
                            spreadsheet.Data.Add(item);

                    }

                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Metadata), out var metadataElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new ObjectToInferredTypeConverter() }
                        };
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>(serializer) ??
                            new Dictionary<string, object?>();

                        foreach (var item in metadatas)
                            spreadsheet.Metadata.Add(item.Key, item.Value);

                    }

                    if (root.TryGetProperty(nameof(Spreadsheet<TId>.Permissions), out var permissionElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new PermissionConverter() }
                        };
                        var permissions = permissionElement.Deserialize<IList<Authorization.Permission>>(serializer) ??
                            new List<Authorization.Permission>();

                        foreach (var item in permissions)
                            spreadsheet.Permissions.Add(item);

                    }

                    return spreadsheet;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Spreadsheet<TId> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                //writer.WriteNullValue(); 
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Id));
            JsonSerializer.Serialize(writer, value.Id, options);

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Name));
            writer.WriteStringValue(value.Name);

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Group));
            writer.WriteStringValue(value.Group);

            var serializer = new JsonSerializerOptions
            {
                Converters = {
                    new TwoDimensionalArrayConverter<object>(),
                    new DoubleConverter(),
                    new PermissionConverter(),
                    new ObjectToInferredTypeConverter()
                }
            };

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Data));
            JsonSerializer.Serialize(writer, value.Data, serializer);

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Metadata));
            JsonSerializer.Serialize(writer, value.Metadata, serializer);

            writer.WritePropertyName(nameof(Spreadsheet<TId>.Permissions));
            JsonSerializer.Serialize(writer, value.Permissions, serializer);

            writer.WriteEndObject();
        }
    }
}

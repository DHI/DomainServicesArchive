namespace DHI.Services.Documents.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class DocumentConverter<TId> : JsonConverter<Document<TId>>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Document<TId>);

        public override Document<TId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is (not JsonTokenType.Null or not JsonTokenType.None) and JsonTokenType.StartObject)
            {

                if (JsonDocument.TryParseValue(ref reader, out var doc))
                {
                    var root = doc.RootElement;

                    TId id = default;
                    if (root.TryGetProperty(nameof(Document.Id), out var idElement, true))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Document)}' required non-null '{nameof(Document.Id)}' property");
                        }
                        id = (TId)Convert.ChangeType(idElement.GetString(), typeof(TId));
                    }

                    string name = string.Empty;
                    if (root.TryGetProperty(nameof(Document.Name), out var nameElement, true))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Document)}' required non-null '{nameof(Document.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string group = string.Empty;
                    if (root.TryGetProperty(nameof(Document.Group), out var groupElement, true))
                    {
                        group = groupElement.GetString();
                    }

                    var document = new Document<TId>(id, name, group);

                    if (root.TryGetProperty(nameof(Document.Metadata), out var metadataElement, true))
                    {
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>() ??
                            new Dictionary<string, object>();

                        foreach (var metadata in metadatas)
                            document.Metadata.Add(metadata.Key, metadata.Value);
                    }

                    if (root.TryGetProperty(nameof(Document.Permissions), out var permissionElement, true))
                    {
                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new PermissionConverter(),
                                }
                        };
                        var permissions = permissionElement.Deserialize<IList<Authorization.Permission>>(_serializer) ??
                            new List<Authorization.Permission>();

                        foreach (var permission in permissions)
                            document.Permissions.Add(permission);
                    }

                    return document;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Document<TId> value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Document<TId>)null, options);
                    break;
                default:
                    {
                        var document = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(Document<TId>.Id));
                        JsonSerializer.Serialize(writer, document.Id);

                        writer.WritePropertyName(nameof(Document<TId>.Name));
                        writer.WriteStringValue(document.Name);

                        writer.WritePropertyName(nameof(Document<TId>.Group));
                        writer.WriteStringValue(document.Group);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                            {
                                new PermissionConverter()
                            }
                        };

                        writer.WritePropertyName(nameof(Document<TId>.Metadata));
                        JsonSerializer.Serialize(writer, document.Metadata, serializer);

                        writer.WritePropertyName(nameof(Document<TId>.Permissions));
                        JsonSerializer.Serialize(writer, document.Permissions, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }

    public class DocumentConverter : DocumentConverter<string>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Document);
    }
}

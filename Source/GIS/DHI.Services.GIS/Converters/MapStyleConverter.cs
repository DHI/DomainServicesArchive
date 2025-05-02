namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Services.GIS.Maps;

    public class MapStyleConverter : JsonConverter<MapStyle>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MapStyle);

        public override MapStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    string id = default;
                    if (root.TryGetProperty(nameof(MapStyle.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(MapStyle)}' required non-null '{nameof(MapStyle.Id)}' property");
                        }
                        id = idElement.GetString();
                    }

                    string name = string.Empty;
                    if (root.TryGetProperty(nameof(MapStyle.Name), out var nameElement))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(MapStyle)}' required non-null '{nameof(MapStyle.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    var mapStyle = new MapStyle(id, name);

                    if (root.TryGetProperty(nameof(MapStyle.StyleCode), out var styleCodeElement))
                    {
                        mapStyle.StyleCode = styleCodeElement.GetString();
                    }

                    if (root.TryGetProperty(nameof(MapStyle.StyleFile), out var styleFileElement))
                    {
                        mapStyle.StyleFile = styleFileElement.GetString();
                    }

                    if (root.TryGetProperty(nameof(MapStyle.Metadata), out var metadataElement))
                    {
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>() ??
                            new Dictionary<string, object>();

                        foreach (var metadata in metadatas)
                            mapStyle.Metadata.Add(metadata.Key, metadata.Value);
                    }

                    if (root.TryGetProperty(nameof(MapStyle.Permissions), out var permissionElement))
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
                            mapStyle.Permissions.Add(permission);
                    }

                    return mapStyle;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, MapStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (MapStyle)null, options);
                    break;
                default:
                    {
                        var mapStyle = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(MapStyle.Id));
                        JsonSerializer.Serialize(writer, mapStyle.Id);

                        writer.WritePropertyName(nameof(MapStyle.Name));
                        writer.WriteStringValue(mapStyle.Name);

                        writer.WritePropertyName(nameof(MapStyle.StyleCode));
                        writer.WriteStringValue(mapStyle.StyleCode);

                        writer.WritePropertyName(nameof(MapStyle.StyleFile));
                        writer.WriteStringValue(mapStyle.StyleFile);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new PermissionConverter()
                                }
                        };

                        writer.WritePropertyName(nameof(MapStyle.Metadata));
                        JsonSerializer.Serialize(writer, mapStyle.Metadata, serializer);

                        writer.WritePropertyName(nameof(MapStyle.Permissions));
                        JsonSerializer.Serialize(writer, mapStyle.Permissions, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

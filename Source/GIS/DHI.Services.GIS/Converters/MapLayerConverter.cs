namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Services.GIS.Maps;
    using DHI.Spatial;
    using DHI.Spatial.GeoJson;

    public class MapLayerConverter : JsonConverter<Layer>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Layer);

        public override Layer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    string id = default;
                    if (root.TryGetProperty(nameof(Layer.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Layer)}' required non-null '{nameof(Layer.Id)}' property");
                        }
                        id = idElement.GetString();
                    }

                    string name = string.Empty;
                    if (root.TryGetProperty(nameof(Layer.Name), out var nameElement))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Layer)}' required non-null '{nameof(Layer.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string group = string.Empty;
                    if (root.TryGetProperty(nameof(Layer.Group), out var groupElement))
                    {
                        group = groupElement.GetString();
                    }

                    var layer = new Layer(id, name, group);

                    if (root.TryGetProperty(nameof(Layer.CoordinateSystem), out var crsElement))
                    {
                        var crs = crsElement.GetString();
                        layer.CoordinateSystem = crs;
                    }

                    if (root.TryGetProperty(nameof(Layer.BoundingBox), out var boxElement))
                    {
                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new BoundingBoxConverter(),
                                }
                        };
                        var bbox = boxElement.Deserialize<BoundingBox>(_serializer);
                        layer.BoundingBox = bbox;
                    }

                    if (root.TryGetProperty(nameof(Layer.Metadata), out var metadataElement))
                    {
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>() ??
                            new Dictionary<string, object>();

                        foreach (var metadata in metadatas)
                            layer.Metadata.Add(metadata.Key, metadata.Value);
                    }

                    if (root.TryGetProperty(nameof(Layer.Permissions), out var permissionElement))
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
                            layer.Permissions.Add(permission);
                    }

                    return layer;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Layer value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Layer)null, options);
                    break;
                default:
                    {
                        var layer = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(Layer.Id));
                        JsonSerializer.Serialize(writer, layer.Id);

                        writer.WritePropertyName(nameof(Layer.Name));
                        writer.WriteStringValue(layer.Name);

                        writer.WritePropertyName(nameof(Layer.Group));
                        writer.WriteStringValue(layer.Group);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new BoundingBoxConverter(),
                                    new PermissionConverter()
                                }
                        };

                        writer.WritePropertyName(nameof(Layer.BoundingBox));
                        JsonSerializer.Serialize(writer, layer.BoundingBox, serializer);

                        writer.WritePropertyName(nameof(Layer.CoordinateSystem));
                        writer.WriteStringValue(layer.CoordinateSystem);

                        writer.WritePropertyName(nameof(Layer.Metadata));
                        JsonSerializer.Serialize(writer, layer.Metadata, serializer);

                        writer.WritePropertyName(nameof(Layer.Permissions));
                        JsonSerializer.Serialize(writer, layer.Permissions, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

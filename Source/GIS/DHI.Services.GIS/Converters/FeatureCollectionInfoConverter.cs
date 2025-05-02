namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Spatial;
    using DHI.Spatial.GeoJson;
    using Attribute = Spatial.Attribute;

    public class FeatureCollectionInfoConverter<TId> : JsonConverter<FeatureCollectionInfo<TId>>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(FeatureCollectionInfo<TId>);

        public override FeatureCollectionInfo<TId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    TId id = default;
                    if (root.TryGetProperty(nameof(FeatureCollectionInfo<TId>.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"{typeToConvert} required non-null '{nameof(FeatureCollectionInfo<TId>.Id)}' property");
                        }
                        id = (TId)Convert.ChangeType(idElement.GetString(), typeof(TId));
                    }

                    string name = string.Empty;
                    if (root.TryGetProperty(nameof(FeatureCollectionInfo<TId>.Name), out var nameElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeToConvert}' required non-null '{nameof(FeatureCollectionInfo<TId>.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string group = null;
                    if (root.TryGetProperty(nameof(FeatureCollectionInfo<TId>.Group), out var groupElement))
                    {
                        group = groupElement.GetString() ?? null;
                    }

                    IList<Attribute> attributes = null;
                    if (root.TryGetProperty(nameof(FeatureCollectionInfo<TId>.Attributes), out var attributesElement))
                    {
                        var attributeSerializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new AttributeConverter(),
                                }
                        };

                        attributes = attributesElement.Deserialize<IList<Attribute>>(attributeSerializer) ?? null;
                    }

                    var featureCollectionInfo = new FeatureCollectionInfo<TId>(id, name, group, attributes);
                    return featureCollectionInfo;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, FeatureCollectionInfo<TId> value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (FeatureCollectionInfo<TId>)null, options);
                    break;
                default:
                    {
                        var featureCollection = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(FeatureCollection<TId>.Id));
                        JsonSerializer.Serialize(writer, featureCollection.Id);

                        writer.WritePropertyName(nameof(FeatureCollection<TId>.Name));
                        writer.WriteStringValue(featureCollection.Name);

                        writer.WritePropertyName(nameof(FeatureCollection<TId>.Group));
                        writer.WriteStringValue(featureCollection.Group);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new AttributeConverter(),
                                }
                        };

                        writer.WritePropertyName(nameof(FeatureCollection<TId>.Attributes));
                        JsonSerializer.Serialize(writer, featureCollection.Attributes, serializer);

                        writer.WritePropertyName(nameof(IFeatureCollection.Attributes));
                        JsonSerializer.Serialize(writer, featureCollection.Attributes, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

namespace DHI.Services.Places.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class FeatureIdConverter<TCollectionId> : JsonConverter<FeatureId<TCollectionId>>
        where TCollectionId : notnull
    {
        public bool CanRead => true;

        public override FeatureId<TCollectionId>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                return default;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"JsonTokenType was of type '{reader.TokenType}', only objects are supported");

            if (JsonDocument.TryParseValue(ref reader, out var doc))
            {
                var root = doc.RootElement;

                TCollectionId? featureCollectionId = default;
                if (root.TryGetProperty(nameof(FeatureId<TCollectionId>.FeatureCollectionId), out var featureCollectionIdElement))
                {
                    if (featureCollectionIdElement.ValueKind == JsonValueKind.Null)
                    {
                        throw new JsonException($"'{typeof(FeatureId<TCollectionId>)}' required non-null '{nameof(FeatureId<TCollectionId>.FeatureCollectionId)}' property.");
                    }
                    featureCollectionId = featureCollectionIdElement.Deserialize<TCollectionId>() ?? default;
                }

                string? attributeKey = string.Empty;
                if (root.TryGetProperty(nameof(FeatureId<TCollectionId>.AttributeKey), out var attributKeyElement))
                {
                    if (attributKeyElement.ValueKind == JsonValueKind.Null)
                    {
                        throw new JsonException($"'{typeof(FeatureId<TCollectionId>)}' required non-null '{nameof(FeatureId<TCollectionId>.AttributeKey)}' property.");
                    }
                    attributeKey = attributKeyElement.GetString();
                }

                object? attributeValue = default;
                if (root.TryGetProperty(nameof(FeatureId<TCollectionId>.AttributeValue), out var attributValueElement))
                {
                    if (attributValueElement.ValueKind == JsonValueKind.Null)
                    {
                        throw new JsonException($"'{typeof(FeatureId<TCollectionId>)}' required non-null '{nameof(FeatureId<TCollectionId>.AttributeValue)}' property.");
                    }
                    attributeValue = attributValueElement.GetString();
                }

                return new FeatureId<TCollectionId>(featureCollectionId!, attributeKey!, attributeValue!);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, FeatureId<TCollectionId> value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (FeatureId<TCollectionId>?)null, options);
                    break;
                default:
                    {
                        var featureIdCollection = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(FeatureId<TCollectionId>.FeatureCollectionId));
                        JsonSerializer.Serialize(writer, featureIdCollection.FeatureCollectionId);

                        writer.WritePropertyName(nameof(FeatureId<TCollectionId>.AttributeKey));
                        writer.WriteStringValue(featureIdCollection.AttributeKey);

                        writer.WritePropertyName(nameof(FeatureId<TCollectionId>.AttributeValue));
                        JsonSerializer.Serialize(writer, featureIdCollection.AttributeValue);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

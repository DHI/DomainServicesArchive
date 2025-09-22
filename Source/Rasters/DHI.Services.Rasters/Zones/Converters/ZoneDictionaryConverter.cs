namespace DHI.Services.Rasters.Zones.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ZoneDictionaryConverter : JsonConverter<IDictionary<string, Zone>>
    {
        /// <summary>
        /// To override CanConvert()
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type typeToConvert)
        => typeof(IDictionary<string, Zone>)
              .IsAssignableFrom(typeToConvert);

        public override IDictionary<string, Zone> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object for Zone dictionary.");

            var result = new Dictionary<string, Zone>(StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var propName = reader.GetString()!;
                reader.Read();

                if (propName == "$type")
                {
                    using var _ = JsonDocument.ParseValue(ref reader);
                    continue;
                }

                var zone = JsonSerializer.Deserialize<Zone>(ref reader, options)
                           ?? throw new JsonException($"Unable to deserialize Zone for key '{propName}'.");
                result[propName] = zone;
            }

            throw new JsonException("Unexpected end when reading Zone dictionary.");
        }

        /// <summary>
        /// To override Write() method
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<string, Zone> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}

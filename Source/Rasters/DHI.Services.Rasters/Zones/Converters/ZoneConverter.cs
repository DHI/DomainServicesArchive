namespace DHI.Services.Rasters.Zones
{
    using Authorization;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ZoneConverter : JsonConverter<Zone>
    {
        /// <summary>
        ///     Override Read() method.
        /// </summary>
        /// <param name="reader">The reader is reference type.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The options for deserialization.</param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override Zone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var id = string.Empty;
            var name = string.Empty;
            var pixelWeights = new HashSet<PixelWeight>();
            var size = new System.Drawing.Size();
            IDictionary<string, object> metadata = new Dictionary<string, object>();
            var zoneType = new ZoneType();
            var permissions = new List<Permission>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                var propertyName = reader.GetString();
                switch (propertyName)
                {
                    case nameof(Zone.Id):
                        reader.Read();
                        id = reader.GetString();
                        break;
                    case nameof(Zone.Name):
                        reader.Read();
                        name = reader.GetString();
                        break;
                    case nameof(Zone.PixelWeights):
                        var pixelWeightOptions = new JsonSerializerOptions();
                        pixelWeights = JsonSerializer.Deserialize<HashSet<PixelWeight>>(ref reader, pixelWeightOptions);
                        break;
                    case nameof(Zone.Type):

                        zoneType = JsonSerializer.Deserialize<ZoneType>(ref reader, options)!;
                        break;

                    case nameof(Zone.Permissions):
                        permissions = JsonSerializer.Deserialize<List<Permission>>(ref reader);
                        break;
                    case nameof(Zone.Metadata):
                        var metadataOptions = new JsonSerializerOptions();
                        metadataOptions.Converters.Add(new MetadataConverter());
                        metadata = JsonSerializer.Deserialize<IDictionary<string, object>>(ref reader, metadataOptions);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                throw new JsonException();
            }

            return new Zone(id, name, metadata, permissions, zoneType)
            {
                ImageSize = size,
                PixelWeights = pixelWeights
            };
        }

        /// <summary>
        ///     Override Write() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        public override void Write(Utf8JsonWriter writer, Zone value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Zone.Id));
            JsonSerializer.Serialize(writer, value.Id, options);

            writer.WritePropertyName(nameof(Zone.Name));
            JsonSerializer.Serialize(writer, value.Name, options);

            writer.WritePropertyName(nameof(Zone.PixelWeights));
            JsonSerializer.Serialize(writer, value.PixelWeights, options);

            writer.WritePropertyName(nameof(Zone.PixelWeightsAreValid));
            JsonSerializer.Serialize(writer, value.PixelWeightsAreValid, options);

            writer.WritePropertyName(nameof(Zone.PixelWeightTotal));
            JsonSerializer.Serialize(writer, value.PixelWeightTotal, options);

            writer.WritePropertyName(nameof(Zone.Type));
            var zoneTypeOptions = new JsonSerializerOptions();

            // using ZoneTypeConverter to serialize the Type as an object and not a string as
            // EnumerationConverter.Write() implements
            zoneTypeOptions.Converters.Add(new ZoneTypeConverter());
            JsonSerializer.Serialize(writer, value.Type, zoneTypeOptions);

            writer.WritePropertyName(nameof(Zone.ImageSize));
            JsonSerializer.Serialize(writer, value.ImageSize, options);

            writer.WritePropertyName(nameof(Zone.Metadata));
            JsonSerializer.Serialize(writer, value.Metadata, options);

            writer.WriteEndObject();
        }
    }
}
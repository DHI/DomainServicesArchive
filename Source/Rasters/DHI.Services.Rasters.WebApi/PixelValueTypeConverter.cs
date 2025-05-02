namespace DHI.Services.Rasters.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Radar;

    public class PixelValueTypeConverter : JsonConverter<PixelValueType>
    {
        public override PixelValueType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return str switch
                {
                    "Reflectivity" => PixelValueType.Reflectivity,
                    "Intensity" => PixelValueType.Intensity,
                    _ => throw new NotSupportedException($"PixelValueType '{str}' is not supported.")
                };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var jo = doc.RootElement;

                if (!jo.TryGetProperty("displayName", out var disp))
                    throw new JsonException("Expected 'displayName' when reading PixelValueType object.");

                var displayName = disp.GetString();
                return displayName switch
                {
                    "Reflectivity" => PixelValueType.Reflectivity,
                    "Intensity" => PixelValueType.Intensity,
                    _ => throw new NotSupportedException($"PixelValueType '{displayName}' is not supported.")
                };
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing PixelValueType.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            PixelValueType value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
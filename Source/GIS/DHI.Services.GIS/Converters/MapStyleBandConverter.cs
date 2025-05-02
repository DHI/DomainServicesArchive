namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Maps;
    using SkiaSharp;

    public class MapStyleBandConverter : JsonConverter<MapStyleBand>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MapStyleBand);

        public override MapStyleBand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;
                    var mapStyleBand = new MapStyleBand();

                    if (root.TryGetProperty(nameof(MapStyleBand.BandValue), out var bandValueElement))
                    {
                        mapStyleBand.BandValue = getSafeDouble(bandValueElement);
                    }
                    if (root.TryGetProperty(nameof(MapStyleBand.UpperBandValue), out var upperBandValueElement))
                    {
                        mapStyleBand.UpperBandValue = getSafeDouble(upperBandValueElement);
                    }
                    if (root.TryGetProperty(nameof(MapStyleBand.LowerBandValue), out var lowerBandValueElement))
                    {
                        mapStyleBand.LowerBandValue = getSafeDouble(lowerBandValueElement);
                    }

                    if (root.TryGetProperty(nameof(MapStyleBand.BandText), out var bandTextElement))
                    {
                        mapStyleBand.BandText = bandTextElement.GetString();
                    }

                    if (root.TryGetProperty(nameof(MapStyleBand.BandColor), out var bandColorElement))
                    {
                        var color = bandColorElement.Deserialize<SKColor>();
                        mapStyleBand.BandColor = color;
                    }

                    return mapStyleBand;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, MapStyleBand value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (MapStyleBand)null, options);
                    break;
                default:
                    {
                        var mapStyleBand = value;

                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(MapStyleBand.BandValue));
                        writeSafeDouble(writer, mapStyleBand.BandValue);

                        writer.WritePropertyName(nameof(MapStyleBand.UpperBandValue));
                        writeSafeDouble(writer, mapStyleBand.UpperBandValue);

                        writer.WritePropertyName(nameof(MapStyleBand.LowerBandValue));
                        writeSafeDouble(writer, mapStyleBand.LowerBandValue);

                        writer.WritePropertyName(nameof(MapStyleBand.BandText));
                        writer.WriteStringValue(mapStyleBand.BandText);

                        writer.WritePropertyName(nameof(MapStyleBand.BandColor));
                        JsonSerializer.Serialize(writer, mapStyleBand.BandColor);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }

        private double getSafeDouble(JsonElement element)
        {
            double value;
            if (element.ValueKind == JsonValueKind.String)
            {
                _ = double.TryParse(element.GetString(), out value);
            }
            else
            {
                element.TryGetDouble(out value);
            }

            return value;
        }

        private void writeSafeDouble(Utf8JsonWriter writer, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }
    }
}

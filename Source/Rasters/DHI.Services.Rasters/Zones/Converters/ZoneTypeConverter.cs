namespace DHI.Services.Rasters.Zones
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ZoneTypeConverter : JsonConverter<ZoneType>

    {
        public override ZoneType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            
            var zoneType = new ZoneType();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString()?? throw new JsonException($"'{nameof(ZoneType)}' property cannot be null or empty.");
                    switch (propertyName)
                    {
                        case nameof(ZoneType.DisplayName):
                            reader.Read();
                            var displayName = reader.GetString()?? throw new JsonException($"'{nameof(ZoneType)}' value cannot be null or empty.");

                            if (displayName.Equals(ZoneType.LineString.DisplayName, StringComparison.OrdinalIgnoreCase))
                            {
                                zoneType = ZoneType.LineString;
                            }
                            else if (displayName.Equals(ZoneType.Point.DisplayName, StringComparison.OrdinalIgnoreCase))
                            {
                                zoneType = ZoneType.Point;
                            }
                            else if (displayName.Equals(ZoneType.Polygon.DisplayName, StringComparison.OrdinalIgnoreCase))
                            {
                                zoneType = ZoneType.Polygon;
                            }

                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

            return zoneType;
        }

        public override void Write(Utf8JsonWriter writer, ZoneType value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("DisplayName", value.DisplayName);
            writer.WriteNumber("Value", value.Value);

            writer.WriteEndObject();
        }
    }
}

namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Maps;
    using DHI.Spatial;
    using DHI.Spatial.GeoJson;

    public class TileConverter : JsonConverter<Tile>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Tile);

        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    uint row = default;
                    if (root.TryGetProperty(nameof(Tile.Row), out var rowElement))
                    {
                        if (rowElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Tile)}' required non-null '{nameof(Tile.Row)}' property");
                        }
                        row = rowElement.GetUInt32();
                    }
                    uint col = default;
                    if (root.TryGetProperty(nameof(Tile.Row), out var colElement))
                    {
                        if (colElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Tile)}' required non-null '{nameof(Tile.Col)}' property");
                        }
                        col = colElement.GetUInt32();
                    }

                    BoundingBox bbox = default;
                    if (root.TryGetProperty(nameof(Tile.BoundingBox), out var boxElement))
                    {
                        if (colElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Tile)}' required non-null '{nameof(Tile.BoundingBox)}' property");
                        }

                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new BoundingBoxConverter(),
                                }
                        };
                        bbox = boxElement.Deserialize<BoundingBox>(_serializer);
                    }

                    return new Tile(bbox, row, col);
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Tile value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Tile)null, options);
                    break;
                default:
                    {
                        var tile = value;
                        writer.WriteStartObject();


                        writer.WritePropertyName(nameof(Tile.Row));
                        writer.WriteNumberValue(tile.Row);

                        writer.WritePropertyName(nameof(Tile.Col));
                        writer.WriteNumberValue(tile.Col);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new BoundingBoxConverter()
                                }
                        };

                        writer.WritePropertyName(nameof(Tile.BoundingBox));
                        JsonSerializer.Serialize(writer, tile.BoundingBox, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

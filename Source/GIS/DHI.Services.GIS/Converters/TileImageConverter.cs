namespace DHI.Services.GIS.Converters
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Maps;
    using DHI.Spatial;
    using DHI.Spatial.GeoJson;
    using SkiaSharp;

    public class TileImageConverter : JsonConverter<TileImage>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TileImage);

        public override TileImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    uint row = default;
                    if (root.TryGetProperty(nameof(TileImage.Row), out var rowElement))
                    {
                        if (rowElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(TileImage)}' required non-null '{nameof(TileImage.Row)}' property");
                        }
                        row = rowElement.GetUInt32();
                    }
                    uint col = default;
                    if (root.TryGetProperty(nameof(TileImage.Row), out var colElement))
                    {
                        if (colElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(TileImage)}' required non-null '{nameof(TileImage.Col)}' property");
                        }
                        col = colElement.GetUInt32();
                    }

                    BoundingBox bbox = default;
                    if (root.TryGetProperty(nameof(TileImage.BoundingBox), out var bboxElement))
                    {
                        if (bboxElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(TileImage)}' required non-null '{nameof(TileImage.BoundingBox)}' property");
                        }

                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new BoundingBoxConverter(),
                                }
                        };
                        bbox = bboxElement.Deserialize<BoundingBox>(_serializer);
                    }

                    var tile = new Tile(bbox, row, col);

                    SKBitmap image = null;
                    if (root.TryGetProperty(nameof(TileImage.Image), out var imageElement))
                    {
                        if (imageElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(TileImage)}' required non-null '{nameof(TileImage.Image)}' property");
                        }

                        if (imageElement.TryGetBytesFromBase64(out byte[] imageBytes))
                        {
                            using var stream = new MemoryStream(imageBytes);
                            image = SKBitmap.Decode(stream);
                        }
                        //var imageByte = imageElement.Deserialize<byte[]>(); 
                    }

                    var tileImage = new TileImage(image, tile);
                    return tileImage;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TileImage value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (TileImage)null, options);
                    break;
                default:
                    {
                        var tileImage = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(TileImage.Row));
                        writer.WriteNumberValue(tileImage.Row);

                        writer.WritePropertyName(nameof(TileImage.Col));
                        writer.WriteNumberValue(tileImage.Col);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                            {
                                new BoundingBoxConverter()
                            }
                        };

                        writer.WritePropertyName(nameof(TileImage.BoundingBox));
                        JsonSerializer.Serialize(writer, tileImage.BoundingBox, serializer);

                        writer.WritePropertyName(nameof(TileImage.Image));
                        using (var skData = tileImage.Image.Encode(SKEncodedImageFormat.Bmp, 100))
                        {
                            writer.WriteBase64StringValue(skData.ToArray());
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

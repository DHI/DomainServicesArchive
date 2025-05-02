namespace DHI.Services.GIS.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Maps;

    public class PaletteConverter<T> : JsonConverter<IDictionary<T, MapStyleBand>>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Palette) ||
            typeToConvert == typeof(IDictionary<T, MapStyleBand>);

        public override IDictionary<T, MapStyleBand> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None)
                && reader.TokenType == JsonTokenType.StartObject)
            {
                var serializer = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new MapStyleBandConverter()
                    }
                };

                var palletes = JsonSerializer.Deserialize<IDictionary<T, MapStyleBand>>(ref reader, serializer);
                return palletes;

                //if (JsonDocument.TryParseValue(ref reader, out var document))
                //{
                //    var root = document.RootElement;

                //    string code = default;
                //    if (root.TryGetProperty(nameof(Palette.Code), out var codeElement))
                //    {
                //        if (codeElement.ValueKind == JsonValueKind.Null)
                //        {
                //            throw new JsonException($"'{typeof(Palette)}' required non-null '{nameof(Palette.Code)}' property");
                //        }
                //        code = codeElement.GetString();
                //    }

                //    PaletteType paletteType = PaletteType.LowerThresholdValues;
                //    if (root.TryGetProperty(nameof(Palette.Type), out var typeElement))
                //    {
                //        var _serializer = new JsonSerializerOptions
                //        {
                //            Converters =
                //                {
                //                    new JsonStringEnumConverter(),
                //                }
                //        };
                //        paletteType = typeElement.Deserialize<PaletteType>(_serializer);
                //    }

                //    var palette = new Palette(code, type: paletteType);
                //    return palette;
                //}
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<T, MapStyleBand> value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Palette)null, options);
                    break;
                default:
                    {
                        var palettes = value;
                        writer.WriteStartObject();

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                            {
                                new MapStyleBandConverter()
                            }
                        };
                        var enumerator = palettes.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            writer.WritePropertyName(enumerator.Current.Key.ToString());
                            JsonSerializer.Serialize(writer, enumerator.Current.Value, serializer);
                        }
                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

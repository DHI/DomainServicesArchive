namespace DHI.Services.Places.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Services.GIS.Maps;
    using DHI.Services.TimeSeries;

    public class IndicatorConverter : BaseTypeResolverConverter
    {
        public IndicatorConverter() : base(typeof(Indicator))
        {
        }

        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Indicator);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new IndicatorJsonConverter();
        }

        protected class IndicatorJsonConverter : BaseTypeResolverJsonConverter<Indicator?>
        {
            public override Indicator? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
                {
                    if (JsonDocument.TryParseValue(ref reader, out var document))
                    {
                        var root = document.RootElement;

                        DataSource dataSource = default;
                        if (root.TryGetProperty(nameof(Indicator.DataSource), out var dataSourceElement))
                        {
                            if (dataSourceElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(Indicator)}' required non-null '{nameof(Indicator.DataSource)}' property");
                            }
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new DataSourceConverter() }
                            };

                            dataSource = dataSourceElement.Deserialize<DataSource>(serializer);
                        }

                        string? styleCode = string.Empty;
                        if (root.TryGetProperty(nameof(Indicator.StyleCode), out var styleCodeElement))
                        {
                            if (styleCodeElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(Indicator)}' required non-null '{nameof(Indicator.StyleCode)}' property");
                            }
                            styleCode = styleCodeElement.GetString();
                        }

                        PaletteType paletteType = PaletteType.LowerThresholdValues;
                        if (root.TryGetProperty(nameof(Indicator.PaletteType), out var typeElement))
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() }
                            };

                            paletteType = typeElement.Deserialize<PaletteType>(serializer);
                        }

                        double? quantile = null;
                        if (root.TryGetProperty(nameof(Indicator.Quantile), out var quantileElement))
                        {
                            if (quantileElement.ValueKind != JsonValueKind.Null &&
                                quantileElement.TryGetDouble(out double _quantile))
                            {
                                quantile = _quantile;
                            }
                        }

                        AggregationType? aggregationType = null;
                        if (root.TryGetProperty(nameof(Indicator.AggregationType), out var aggregationTypeElement))
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new EnumerationConverter() }
                            };

                            aggregationType = aggregationTypeElement.Deserialize<AggregationType>(serializer);
                        }

                        TimeInterval? timeInterval = null;
                        if (root.TryGetProperty(nameof(Indicator.TimeInterval), out var timeIntervalTypeElement))
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new TimeIntervalConverter() }
                            };

                            timeInterval = timeIntervalTypeElement.Deserialize<TimeInterval>(serializer);
                        }

                        return new Indicator(dataSource!, styleCode!, timeInterval, aggregationType, quantile, paletteType);
                    }
                }

                return default;
            }

            public override void Write(Utf8JsonWriter writer, Indicator? value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, (Indicator?)null, options);
                        break;
                    default:
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = {
                                    new JsonStringEnumConverter(),
                                    new EnumerationConverter(),
                                    new DataSourceConverter(),
                                    new TimeIntervalConverter(),
                                    new ObjectToInferredTypeConverterPatch(),
                                }
                            };

                            var indicator = value;
                            writer.WriteStartObject();

                            writer.WriteString(TypeDiscriminator, $"{value.GetType().ResolveTypeFriendlyName()}, {value.GetType().ResolveAssemblyName()}");

                            writer.WritePropertyName(nameof(Indicator.DataSource));
                            JsonSerializer.Serialize(writer, indicator.DataSource, serializer);

                            writer.WritePropertyName(nameof(Indicator.TimeInterval));
                            JsonSerializer.Serialize(writer, indicator.TimeInterval, serializer);

                            writer.WritePropertyName(nameof(Indicator.Quantile));
                            if (indicator.Quantile.HasValue)
                                writer.WriteNumberValue(indicator.Quantile.Value);
                            else
                                writer.WriteNullValue();

                            writer.WritePropertyName(nameof(Indicator.StyleCode));
                            writer.WriteStringValue(indicator.StyleCode);

                            writer.WritePropertyName(nameof(Indicator.PaletteType));
                            JsonSerializer.Serialize(writer, indicator.PaletteType, serializer);

                            writer.WritePropertyName(nameof(Indicator.AggregationType));
                            JsonSerializer.Serialize(writer, indicator?.AggregationType, serializer);

                            writer.WriteEndObject();
                            break;
                        }
                }
            }
        }
    }
}

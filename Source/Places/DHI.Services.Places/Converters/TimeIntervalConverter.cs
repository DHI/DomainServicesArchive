namespace DHI.Services.Places.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class TimeIntervalConverter : BaseTypeResolverConverter
    {
        public TimeIntervalConverter() : base(typeof(TimeInterval))
        {
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TimeInterval);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new TimeIntervalJsonConverter();
        }

        protected class TimeIntervalJsonConverter : BaseTypeResolverJsonConverter<TimeInterval?>
        {
            public bool CanRead => true;

            public override TimeInterval? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                    reader.TokenType == JsonTokenType.StartObject)
                {
                    if (JsonDocument.TryParseValue(ref reader, out var document))
                    {
                        var root = document.RootElement;

                        TimeIntervalType type = TimeIntervalType.All;
                        if (root.TryGetProperty(nameof(TimeInterval.Type), out var typeElement))
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() }
                            };
                            type = typeElement.Deserialize<TimeIntervalType>(serializer);
                        }

                        double? start = null;
                        if (root.TryGetProperty(nameof(TimeInterval.Start), out var startElement))
                        {
                            if (type != TimeIntervalType.All &&
                                startElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(TimeInterval)}' required non-null '{nameof(TimeInterval.Start)}' property when {typeof(TimeIntervalType)} type is '{type}'");
                            }

                            if (startElement.ValueKind != JsonValueKind.Null &&
                                startElement.TryGetDouble(out double _start))
                            {
                                start = _start;
                            }
                        }

                        double? end = null;
                        if (root.TryGetProperty(nameof(TimeInterval.End), out var endElement))
                        {
                            if (type != TimeIntervalType.All &&
                                endElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(TimeInterval)}' required non-null '{nameof(TimeInterval.End)}' property when {typeof(TimeIntervalType)} type is '{type}'");
                            }

                            if (endElement.ValueKind != JsonValueKind.Null &&
                                endElement.TryGetDouble(out double _end))
                            {
                                end = _end;
                            }
                        }

                        return new TimeInterval(type, start, end);
                    }
                }

                return default;
            }

            //public override void Write(Utf8JsonWriter writer, TimeInterval? value, JsonSerializerOptions options)
            //{
            //    base.Write(writer, value, NewSerializerOptions<TimeIntervalConverter>(options));
            //}

            public override void Write(Utf8JsonWriter writer, TimeInterval? value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, (TimeInterval?)null, options);
                        break;
                    default:
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                                Converters =
                                {
                                    new JsonStringEnumConverter(),
                                    new ObjectToInferredTypeConverterPatch(),
                                }
                            };

                            var timeInterval = value;
                            writer.WriteStartObject();

                            writer.WriteString(TypeDiscriminator, $"{value.GetType().ResolveTypeFriendlyName()}, {value.GetType().ResolveAssemblyName()}");

                            writer.WritePropertyName(nameof(TimeInterval.Type));
                            JsonSerializer.Serialize(writer, timeInterval.Type, serializer);

                            writer.WritePropertyName(nameof(TimeInterval.Start));
                            if (timeInterval.Start.HasValue)
                                writer.WriteNumberValue(timeInterval.Start.Value);
                            else
                                writer.WriteNullValue();

                            writer.WritePropertyName(nameof(TimeInterval.End));
                            if (timeInterval.End.HasValue)
                                writer.WriteNumberValue(timeInterval.End.Value);
                            else
                                writer.WriteNullValue();

                            writer.WriteEndObject();
                            break;
                        }
                }
            }
        }
    }
}

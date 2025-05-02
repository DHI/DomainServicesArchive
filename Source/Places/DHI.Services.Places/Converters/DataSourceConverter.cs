namespace DHI.Services.Places.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class DataSourceConverter : BaseTypeResolverConverter
    {
        public DataSourceConverter() : base(typeof(DataSource))
        {
        }

        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(DataSource);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new DataSourceJsonConverter();
        }

        protected class DataSourceJsonConverter : BaseTypeResolverJsonConverter<DataSource>
        {
            public override DataSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
                {
                    if (JsonDocument.TryParseValue(ref reader, out var document))
                    {
                        var root = document.RootElement;

                        object? entityId = default;
                        if (root.TryGetProperty(nameof(DataSource.EntityId), out var entityIdElement))
                        {
                            if (entityIdElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(DataSource)}' required non-null '{nameof(DataSource.EntityId)}' property");
                            }
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new ObjectToInferredTypeConverterPatch() }
                            };

                            entityId = entityIdElement.Deserialize<object>(serializer);
                        }

                        string? connectionId = string.Empty;
                        if (root.TryGetProperty(nameof(DataSource.ConnectionId), out var connectionIdElement))
                        {
                            if (connectionIdElement.ValueKind == JsonValueKind.Null)
                            {
                                throw new JsonException($"'{typeof(DataSource)}' required non-null '{nameof(DataSource.ConnectionId)}' property");
                            }
                            connectionId = connectionIdElement.GetString();
                        }

                        DataSourceType dataSoueType = DataSourceType.TimeSeries;
                        if (root.TryGetProperty(nameof(DataSource.Type), out var typeElement))
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() }
                            };
                            dataSoueType = typeElement.Deserialize<DataSourceType>(serializer);
                        }


                        return new DataSource(dataSoueType, connectionId!, entityId!);
                    }
                }

                return default;
            }

            public override void Write(Utf8JsonWriter writer, DataSource value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    //case null:
                    //    JsonSerializer.Serialize(writer, (DataSource?)null, options);
                    //    break;
                    default:
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                Converters = {
                                    new JsonStringEnumConverter(),
                                    new EnumerationConverter(),
                                    new ObjectToInferredTypeConverterPatch(),
                                }
                            };

                            var dataSource = value;
                            writer.WriteStartObject();
                            writer.WriteString(TypeDiscriminator, $"{value.GetType().ResolveTypeFriendlyName()}, {value.GetType().ResolveAssemblyName()}");

                            writer.WritePropertyName(nameof(DataSource.ConnectionId));
                            writer.WriteStringValue(dataSource.ConnectionId);

                            writer.WritePropertyName(nameof(DataSource.EntityId));
                            JsonSerializer.Serialize(writer, dataSource.EntityId, serializer);

                            writer.WritePropertyName(nameof(DataSource.Type));
                            JsonSerializer.Serialize(writer, dataSource.Type, serializer);

                            writer.WriteEndObject();
                            break;
                        }
                }
            }
        }

    }
}

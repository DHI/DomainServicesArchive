namespace DHI.Services.Tables.WebApi.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class ColumnConverter : JsonConverter<Column>
    {
        public override Column Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.Null or not JsonTokenType.None and JsonTokenType.StartObject)
            {

                if (JsonDocument.TryParseValue(ref reader, out var doc))
                {
                    var root = doc.RootElement;

                    string? name = string.Empty;
                    if (root.TryGetProperty(nameof(Column.Name), out var nameElement))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Column)}' required non-null '{nameof(Column.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string? qty = null;
                    if (root.TryGetProperty(nameof(Column.Quantity), out var qtyElement))
                    {
                        if (qtyElement.ValueKind != JsonValueKind.Null)
                        {
                            qty = qtyElement.GetString();
                        }
                    }

                    bool isKey = false;
                    if (root.TryGetProperty(nameof(Column.IsKey), out var isKeyElement))
                    {
                        isKey = isKeyElement.GetBoolean();
                    }

                    Type? type = default;
                    if (root.TryGetProperty(nameof(Column.DataType), out var typeElement))
                    {
                        if (typeElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Column)}' required non-null '{nameof(Column.DataType)}' property");
                        }
                        var serializer = new JsonSerializerOptions(options);
                        serializer.Converters.Clear();
                        serializer.AddConverters(new TypeStringConverter(),
                            new ObjectToInferredTypeConverter());

                        type = typeElement.Deserialize<Type>(serializer);
                    }

                    var column = new Column(name, type, isKey, qty);

                    return column;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Column value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Column.Name));
            writer.WriteStringValue(value.Name);

            writer.WritePropertyName(nameof(Column.IsKey));
            writer.WriteBooleanValue(value.IsKey);

            writer.WritePropertyName(nameof(Column.Quantity));
            writer.WriteStringValue(value.Quantity);

            var serializer = new JsonSerializerOptions(options);
            serializer.Converters.Clear();
            serializer.AddConverters(new TypeStringConverter(),
                new ObjectToInferredTypeConverter());

            writer.WritePropertyName(nameof(Column.DataType));
            JsonSerializer.Serialize(writer, value.DataType, serializer);

            writer.WriteEndObject();
        }
    }
}

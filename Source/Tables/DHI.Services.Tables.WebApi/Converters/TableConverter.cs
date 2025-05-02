namespace DHI.Services.Tables.WebApi.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class TableConverter : JsonConverter<Table>
    {
        public override Table Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.Null or not JsonTokenType.None and JsonTokenType.StartObject)
            {

                if (JsonDocument.TryParseValue(ref reader, out var doc))
                {
                    var root = doc.RootElement;

                    string? id = default;
                    if (root.TryGetProperty(nameof(Table.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Table)}' required non-null '{nameof(Table.Id)}' property");
                        }
                        id = idElement.GetString();
                    }

                    var table = new Table(id);


                    if (root.TryGetProperty(nameof(Table.Columns), out var columnsElement))
                    {
                        if (columnsElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Table)}' required non-null '{nameof(Table.Columns)}' property");
                        }

                        var serializer = new JsonSerializerOptions
                        {
                            Converters = {
                                new ColumnConverter()
                            }
                        };
                        var columns = columnsElement.Deserialize<List<Column>>(serializer);

                        foreach (var column in columns)
                            table.Columns.Add(column);
                    }


                    if (root.TryGetProperty(nameof(Table.Metadata), out var metadataElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new ObjectToInferredTypeConverter() }
                        };
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>(serializer) ??
                            new Dictionary<string, object?>();

                        foreach (var item in metadatas)
                            table.Metadata.Add(item.Key, item.Value);
                    }

                    if (root.TryGetProperty(nameof(Table.Permissions), out var permissionElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new PermissionConverter() }
                        };
                        var permissions = permissionElement.Deserialize<IList<Authorization.Permission>>(serializer) ??
                            new List<Authorization.Permission>();

                        foreach (var item in permissions)
                            table.Permissions.Add(item);

                    }
                    return table;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Table value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                //if(!(options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault | 
                //    options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault))
                //    writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Table.Id));
            JsonSerializer.Serialize(writer, value.Id, options);

            var serializer = new JsonSerializerOptions(options);
            serializer.Converters.Clear();
            serializer.AddConverters(new ColumnConverter(),
                    new TwoDimensionalArrayConverter<object>(),
                    new DoubleConverter(),
                    new PermissionConverter(),
                    new ObjectToInferredTypeConverter());

            if (value.Columns.Any() && isWritingNull(options))
            {
                writer.WritePropertyName(nameof(Table.Columns), options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase);
                JsonSerializer.Serialize(writer, value.Columns, serializer);
            }

            if (value.KeyColumns.Any() && isWritingNull(options))
            {
                writer.WritePropertyName(nameof(Table.KeyColumns), options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase);
                JsonSerializer.Serialize(writer, value.KeyColumns, serializer);
            }

            if (value.Metadata.Any())
            {
                writer.WritePropertyName(nameof(Table.Metadata));
                JsonSerializer.Serialize(writer, value.Metadata, serializer);
            }

            if (value.Permissions.Any())
            {
                writer.WritePropertyName(nameof(Table.Permissions));
                JsonSerializer.Serialize(writer, value.Permissions, serializer);
            }

            writer.WriteEndObject();
        }

        private static bool isWritingNull(JsonSerializerOptions options) => options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault | options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull;
    }
}

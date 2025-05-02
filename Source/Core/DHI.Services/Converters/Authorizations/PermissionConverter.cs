namespace DHI.Services.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Authorization;

    public class PermissionConverter : JsonConverter<Permission>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Permission);

        public override Permission Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    HashSet<string> principals = default;
                    if (root.TryGetProperty(nameof(Permission.Principals), out var princialElement, true))
                    {
                        if (princialElement.ValueKind == JsonValueKind.Array)
                        {
                            principals = new HashSet<string>();
                            foreach (var principal in princialElement.EnumerateArray())
                            {
                                principals.Add(principal.GetString());
                            }
                        }
                    }

                    PermissionType type = PermissionType.Allowed;
                    if (root.TryGetProperty(nameof(Permission.Type), out var typeElement, true))
                    {
                        Enum.TryParse(typeElement.GetString(), out type);
                    }

                    string operation = null;
                    if (root.TryGetProperty(nameof(Permission.Operation), out var opElement, true))
                    {
                        operation = opElement.GetString();
                    }

                    return new Permission(principals, operation, type);
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Permission value, JsonSerializerOptions options)
        {
            var permission = value;
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Permission.Operation), true);
            writer.WriteStringValue(permission.Operation);

            var serializer = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            };

            writer.WritePropertyName(nameof(Permission.Type), true);
            JsonSerializer.Serialize(writer, permission.Type, serializer);

            writer.WritePropertyName(nameof(Permission.Principals), true);
            JsonSerializer.Serialize(writer, permission.Principals);

            writer.WriteEndObject();
        }
    }

    //public abstract class BasedEntityConverterBase<TId> : JsonConverter<BaseEntity<TId>>
    //{
    //    public bool CanRead => true;

    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        return typeof(BaseEntity<TId>).IsAssignableFrom(typeToConvert);
    //    }

    //    public override BaseEntity<TId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
    //            reader.TokenType == JsonTokenType.StartObject)
    //        {
    //            Utf8JsonReader readerClone = reader;
    //            if (JsonDocument.TryParseValue(ref readerClone, out var document))
    //            {
    //                var root = document.RootElement;

    //                TId id = default;
    //                if (root.TryGetProperty(nameof(BaseEntity<TId>.Id), out var idElement, true))
    //                {
    //                    if (idElement.ValueKind == JsonValueKind.Null)
    //                    {
    //                        throw new JsonException($"'{typeToConvert}' required non-null '{nameof(BaseEntity<TId>.Id)}' property");
    //                    }
    //                    id = (TId)Convert.ChangeType(idElement.GetString(), typeof(TId));
    //                }

    //                string name = string.Empty;
    //                if (typeof(BaseNamedEntity<TId>).IsAssignableFrom(typeToConvert))
    //                {
    //                    if (root.TryGetProperty(nameof(BaseNamedEntity<TId>.Name), out var nameElement, true))
    //                    {
    //                        if (idElement.ValueKind == JsonValueKind.Null)
    //                        {
    //                            throw new JsonException($"'{typeToConvert}' required non-null'{nameof(BaseNamedEntity<TId>.Name)}' property");
    //                        }
    //                        name = nameElement.GetString();
    //                    }
    //                }


    //                string group = string.Empty;
    //                if (typeof(BaseGroupedEntity<TId>).IsAssignableFrom(typeToConvert))
    //                {
    //                    if (root.TryGetProperty(nameof(BaseGroupedEntity<TId>.Group), out var groupElement, true))
    //                    {
    //                        group = groupElement.GetString() ?? null;
    //                    }
    //                }


    //                var metadatas = new Dictionary<string, object?>();
    //                if (root.TryGetProperty(nameof(BaseEntity<TId>.Metadata), out var metadataElement, true))
    //                {
    //                    var _serializer = new JsonSerializerOptions
    //                    {
    //                        Converters =
    //                            {
    //                                new DictionaryConverter<object?>(),
    //                            }
    //                    };
    //                    metadatas = metadataElement.Deserialize<Dictionary<string, object?>>(_serializer);
    //                }

    //                var permissions = new List<Permission>();
    //                if (root.TryGetProperty(nameof(BaseEntity<TId>.Permissions), out var permissionElement, true))
    //                {
    //                    var _serializer = new JsonSerializerOptions
    //                    {
    //                        Converters =
    //                            {
    //                                new PermissionConverter(),
    //                            }
    //                    };
    //                    permissions = permissionElement.Deserialize<List<Permission>>(_serializer);
    //                }

    //                object typeToConverted;
    //                if (typeof(BaseGroupedEntity<TId>).IsAssignableFrom(typeToConvert))
    //                {
    //                    typeToConverted = Activator.CreateInstance(typeToConvert,
    //                    args: new object[]
    //                    {
    //                        id,
    //                        name,
    //                        group
    //                    });
    //                }
    //                else if (typeof(BaseNamedEntity<TId>).IsAssignableFrom(typeToConvert))
    //                {
    //                    typeToConverted = Activator.CreateInstance(typeToConvert,
    //                    args: new object[]
    //                    {
    //                        id,
    //                        name
    //                    });
    //                }
    //                else
    //                {
    //                    typeToConverted = Activator.CreateInstance(typeToConvert,
    //                    args: new object[]
    //                    {
    //                        id
    //                    });
    //                }

    //                var baseTypeConverted = (BaseEntity<TId>)typeToConverted;
    //                if (metadatas?.Any() == true)
    //                {
    //                    foreach (var metadata in metadatas)
    //                        baseTypeConverted.Metadata.Add(metadata.Key, metadata.Value);
    //                }
    //                if (permissions?.Any() == true)
    //                {
    //                    foreach (var permission in permissions)
    //                        baseTypeConverted.Permissions.Add(permission);
    //                }

    //                return baseTypeConverted;
    //            }

    //        }

    //        return default;
    //    }

    //    public override void Write(Utf8JsonWriter writer, BaseEntity<TId> value, JsonSerializerOptions options)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

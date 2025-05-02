namespace DHI.Services.Security.WebApi.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Authorization;
    using DHI.Services.Converters;

    public class UserGroupConverter : JsonConverter<UserGroup>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(UserGroup);

        public override UserGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    string id = default;
                    if (root.TryGetProperty(options.ToNamingPolicy(nameof(UserGroup.Id)), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(UserGroup)}' required non-null '{nameof(UserGroup.Id)}' property");
                        }
                        id = idElement.GetString();
                    }
                    string name = string.Empty;
                    if (root.TryGetProperty(options.ToNamingPolicy(nameof(UserGroup.Name)), out var nameElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(UserGroup)}' required non-null '{nameof(UserGroup.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    HashSet<string> users = new HashSet<string>();
                    if (root.TryGetProperty(options.ToNamingPolicy(nameof(UserGroup.Users)), out var usersElement))
                    {
                        users = usersElement.Deserialize<HashSet<string>>() ?? new HashSet<string>();
                    }

                    var userGroup = new UserGroup(id, name, users);

                    if (root.TryGetProperty(options.ToNamingPolicy(nameof(UserGroup.Metadata)), out var metadataElement))
                    {
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>() ??
                            new Dictionary<string, object>();

                        foreach (var metadata in metadatas)
                            userGroup.Metadata.Add(metadata.Key, metadata.Value);
                    }

                    if (root.TryGetProperty(options.ToNamingPolicy(nameof(UserGroup.Permissions)), out var permissionElement))
                    {
                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new PermissionConverter(),
                                }
                        };
                        var permissions = permissionElement.Deserialize<IList<Permission>>(_serializer) ??
                            new List<Permission>();

                        foreach (var permission in permissions)
                            userGroup.Permissions.Add(permission);
                    }

                    return userGroup;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, UserGroup value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (UserGroup)null, options);
                    break;
                default:
                    {
                        var userGroup = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(options.ToNamingPolicy(nameof(UserGroup.Id)));
                        writer.WriteStringValue(userGroup.Id);

                        writer.WritePropertyName(options.ToNamingPolicy(nameof(UserGroup.Name)));
                        writer.WriteStringValue(userGroup.Name);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                            {
                                new PermissionConverter()
                            }
                        };

                        if (userGroup.Users?.Any() != null || isWriteNull(options))
                        {
                            writer.WritePropertyName(options.ToNamingPolicy(nameof(UserGroup.Users)));
                            JsonSerializer.Serialize(writer, userGroup.Users, serializer);
                        }

                        if (userGroup.Metadata.Any() || isWriteNull(options))
                        {
                            writer.WritePropertyName(options.ToNamingPolicy(nameof(UserGroup.Metadata)));
                            JsonSerializer.Serialize(writer, userGroup.Metadata, serializer);
                        }
                        if (userGroup.Metadata.Any() || isWriteNull(options))
                        {
                            writer.WritePropertyName(options.ToNamingPolicy(nameof(UserGroup.Permissions)));
                            JsonSerializer.Serialize(writer, userGroup.Permissions, serializer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }

        private static bool isWriteNull(JsonSerializerOptions options) => options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull | options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault;
    }
}

namespace DHI.Services.Security.WebApi.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Services.Mails;

    public class MailTemplateConverter : JsonConverter<MailTemplate>
    {
        public bool CanRead => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MailTemplate);

        public override MailTemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartObject)
            {
                if (JsonDocument.TryParseValue(ref reader, out var document))
                {
                    var root = document.RootElement;

                    string id = default;
                    if (root.TryGetProperty(nameof(MailTemplate.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(MailTemplate)}' required non-null '{nameof(MailTemplate.Id)}' property");
                        }
                        id = idElement.GetString();
                    }
                    string name = string.Empty;
                    if (root.TryGetProperty(nameof(MailTemplate.Name), out var nameElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(MailTemplate)}' required non-null '{nameof(MailTemplate.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    var template = new MailTemplate(id, name);

                    if (root.TryGetProperty(nameof(MailTemplate.Subject), out var subjectElement))
                    {
                        template.Subject = subjectElement.GetString() ?? string.Empty;
                    }
                    if (root.TryGetProperty(nameof(MailTemplate.Body), out var bodyElement))
                    {
                        template.Body = bodyElement.GetString() ?? string.Empty;
                    }
                    if (root.TryGetProperty(nameof(MailTemplate.From), out var fromElement))
                    {
                        template.From = fromElement.GetString() ?? string.Empty;
                    }
                    if (root.TryGetProperty(nameof(MailTemplate.FromDisplayName), out var fromNameElement))
                    {
                        template.FromDisplayName = fromNameElement.GetString() ?? string.Empty;
                    }

                    if (root.TryGetProperty(nameof(MailTemplate.Bodies), out var bodiesElement))
                    {
                        var bodies = bodiesElement.Deserialize<IDictionary<string, string?>>() ??
                            new Dictionary<string, string>();

                        template.Bodies = bodies.ToDictionary(key => key.Key, value => value.Value ?? string.Empty);
                    }

                    if (root.TryGetProperty(nameof(MailTemplate.Metadata), out var metadataElement))
                    {
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>() ??
                            new Dictionary<string, object>();

                        foreach (var metadata in metadatas)
                            template.Metadata.Add(metadata.Key, metadata.Value);
                    }

                    if (root.TryGetProperty(nameof(MailTemplate.Permissions), out var permissionElement))
                    {
                        var _serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new PermissionConverter(),
                                }
                        };
                        var permissions = permissionElement.Deserialize<IList<Authorization.Permission>>(_serializer) ??
                            new List<Authorization.Permission>();

                        foreach (var permission in permissions)
                            template.Permissions.Add(permission);
                    }

                    return template;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, MailTemplate value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (MailTemplate)null, options);
                    break;
                default:
                    {
                        var template = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(MailTemplate.Id));
                        writer.WriteStringValue(template.Id);

                        writer.WritePropertyName(nameof(MailTemplate.Name));
                        writer.WriteStringValue(template.Name);

                        writer.WritePropertyName(nameof(MailTemplate.Subject));
                        writer.WriteStringValue(template.Subject);

                        writer.WritePropertyName(nameof(MailTemplate.From));
                        writer.WriteStringValue(template.From);

                        writer.WritePropertyName(nameof(MailTemplate.FromDisplayName));
                        writer.WriteStringValue(template.FromDisplayName);

                        writer.WritePropertyName(nameof(MailTemplate.Body));
                        writer.WriteStringValue(template.Body);

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new PermissionConverter()
                            }
                        };

                        if (template.Bodies != null || isWriteNull(options))
                        {
                            writer.WritePropertyName(nameof(MailTemplate.Bodies));
                            JsonSerializer.Serialize(writer, template.Bodies, serializer);
                        }

                        if (template.Metadata.Any() || isWriteNull(options))
                        {
                            writer.WritePropertyName(nameof(MailTemplate.Metadata));
                            JsonSerializer.Serialize(writer, template.Metadata, serializer);
                        }
                        if (template.Metadata.Any() || isWriteNull(options))
                        {
                            writer.WritePropertyName(nameof(MailTemplate.Permissions));
                            JsonSerializer.Serialize(writer, template.Permissions, serializer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }

        private static bool isWriteNull(JsonSerializerOptions options) => options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull | options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault;
    }
}

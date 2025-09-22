namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class WorkflowXamlConverter : JsonConverter<Workflow>
    {
        public override Workflow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            string id = null;
            string name = null;
            string hostGroup = null;
            string timeout = null;
            DateTime? updated = null;
            Dictionary<string, object> parameters = new();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(Workflow.Id):
                        id = reader.GetString();
                        break;
                    case nameof(Workflow.Name):
                        name = reader.GetString();
                        break;
                    case nameof(Workflow.HostGroup):
                        hostGroup = reader.GetString();
                        break;
                    case nameof(Workflow.Timeout):
                        timeout = reader.GetString();
                        break;
                    case nameof(Workflow.Updated):
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            var s = reader.GetString();
                            if (DateTimeOffset.TryParse(
                                    s,
                                    null,
                                    System.Globalization.DateTimeStyles.RoundtripKind,
                                    out var dto))
                            {
                                updated = dto.UtcDateTime;
                            }
                        }
                        break;
                    case "Parameters":
                        parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var wf = new Workflow(id, name, definition: null)
            {
                HostGroup = hostGroup,
                Updated = updated
            };

            if (TimeSpan.TryParse(timeout, out TimeSpan parsedTimeout))
                wf.Timeout = parsedTimeout;

            foreach (var kv in parameters)
                wf.Parameters[kv.Key] = kv.Value;

            return wf;
        }

        public override void Write(Utf8JsonWriter writer, Workflow value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(nameof(Workflow.Id), value.Id);
            writer.WriteString(nameof(Workflow.Name), value.Name);
            if (!string.IsNullOrWhiteSpace(value.HostGroup))
                writer.WriteString(nameof(Workflow.HostGroup), value.HostGroup);

            if (value.Timeout != null)
                writer.WriteString(nameof(Workflow.Timeout), value.Timeout.ToString());

            if (value.Updated.HasValue)
            {
                writer.WriteString(
                    nameof(Workflow.Updated),
                    DateTime.SpecifyKind(value.Updated.Value, DateTimeKind.Utc)
                            .ToUniversalTime()
                            .ToString("O"));
            }

            writer.WritePropertyName("Parameters");
            writer.WriteStartObject();
            foreach (var param in value.Parameters)
            {
                writer.WritePropertyName(param.Key);
                if (param.Value != null)
                    JsonSerializer.Serialize(writer, param.Value, param.Value.GetType(), options);
                else
                    writer.WriteNullValue();
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}

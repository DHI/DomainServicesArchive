namespace DHI.Services.Jobs.Workflows.WF
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class WorkflowConverter : JsonConverter<Workflow>
    {
        public override bool HandleNull => true;

        public override Workflow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string id = null;
            string name = null;
            string definition = null;
            string hostGroup = null;
            string timeout = null;
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                string propertyName = reader.GetString();
                switch (propertyName)
                {
                    case nameof(Workflow.Id):
                        reader.Read();
                        id = reader.GetString();
                        break;
                    case nameof(Workflow.Name):
                        reader.Read();
                        name = reader.GetString();
                        break;
                    case nameof(Workflow.Definition):
                        reader.Read();
                        definition = reader.GetString();
                        break;
                    case nameof(Workflow.HostGroup):
                        reader.Read();
                        hostGroup = reader.GetString();
                        break;
                    case nameof(Workflow.Timeout):
                        reader.Read();
                        timeout = reader.GetString();
                        break;
                    case "Parameters":
                        reader.Read();
                        parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var workflow = new Workflow(id, name, definition);
            if (string.IsNullOrEmpty(hostGroup))
            {
                workflow.HostGroup = hostGroup;
            }

            TimeSpan.TryParse(timeout, out TimeSpan timeoutValue);
            workflow.Timeout = timeoutValue;

            IEnumerable<KeyValuePair<string, object>> keyValuePairs = parameters;
            foreach (KeyValuePair<string, object> keyValue in keyValuePairs)
            {
                workflow.Parameters.Add(keyValue);
            }

            return workflow;
        }

        public override void Write(Utf8JsonWriter writer, Workflow value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(nameof(Workflow.Id), value.Id);
            writer.WriteString(nameof(Workflow.Name), value.Name);
            writer.WriteString(nameof(Workflow.Definition), value.Definition);
            if (!string.IsNullOrEmpty(value.HostGroup)) writer.WriteString(nameof(Workflow.HostGroup), value.HostGroup);
            if (value.Timeout != null) writer.WriteString(nameof(Workflow.Timeout), value.Timeout.ToString());

            writer.WritePropertyName("Parameters");
            writer.WriteStartObject();
            foreach (var item in value.Parameters)
            {
                writer.WritePropertyName(item.Key);
                if (item.Value != null)
                {
                    JsonSerializer.Serialize(writer, item.Value, item.Value.GetType(), options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
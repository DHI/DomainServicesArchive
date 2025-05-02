namespace DHI.Services.Scalars
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ScalarConverter<TId, TFlag> : JsonConverter<Scalar<TId, TFlag>> where TFlag : struct
    {
        public override Scalar<TId, TFlag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            TId id = default;
            string name = null;
            string valueTypeName = null;
            string group = null;
            ScalarData<TFlag> data = null;
            string description = null;
            bool locked = false;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                   
                string propertyName = reader.GetString();
                switch (propertyName)
                {
                    case nameof(Scalar<TId, TFlag>.Id):
                        reader.Read();
                        id = JsonSerializer.Deserialize<TId>(ref reader, options);
                        break;
                    case nameof(Scalar<TId, TFlag>.Name):
                        reader.Read();
                        name = reader.GetString();
                        break;
                    case nameof(Scalar<TId, TFlag>.ValueTypeName):
                        reader.Read();
                        valueTypeName = reader.GetString();
                        break;
                    case nameof(Scalar<TId, TFlag>.Group):
                        reader.Read();
                        group = reader.GetString();
                        break;
                    case "_data":
                        reader.Read();
                        data = JsonSerializer.Deserialize<ScalarData<TFlag>>(ref reader, options);
                        break;
                    case nameof(Scalar<TId, TFlag>.Description):
                        reader.Read();
                        description = reader.GetString();
                        break;
                    case nameof(Scalar<TId, TFlag>.Locked):
                        reader.Read();
                        locked = reader.GetBoolean();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (data != null && data.Value.GetType() != Type.GetType(valueTypeName))
            {
                // if system.text.json thinks data.Value is not what it should be, assume it is a complex type (i.e GUID) convert to string to be (de)serialized later
                data = new ScalarData<TFlag>(data.Value.ToString(), data.DateTime, data.Flag);
            }

            var scalar = new Scalar<TId, TFlag>(id, name, valueTypeName, group, data)
            {
                Description = description,
                Locked = locked
            };

            return scalar;
        }

        public override void Write(Utf8JsonWriter writer, Scalar<TId, TFlag> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Scalar<TId,TFlag>.Id));
            JsonSerializer.Serialize(writer, value.Id, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.Name));
            JsonSerializer.Serialize(writer, value.Name, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.FullName));
            JsonSerializer.Serialize(writer, value.FullName, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.Group));
            JsonSerializer.Serialize(writer, value.Group, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.ValueTypeName));
            JsonSerializer.Serialize(writer, value.ValueTypeName, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.Description));
            JsonSerializer.Serialize(writer, value.Description, options);

            writer.WritePropertyName(nameof(Scalar<TId, TFlag>.Locked));
            JsonSerializer.Serialize(writer, value.Locked, options);

            writer.WritePropertyName("_data");
            JsonSerializer.Serialize(writer, value.GetData().Value, options);

            writer.WriteEndObject();
        }
    }
}

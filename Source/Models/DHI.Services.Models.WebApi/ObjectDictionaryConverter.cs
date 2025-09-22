namespace DHI.Services.Models.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class ObjectDictionaryConverter : JsonConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadObject(ref reader);
        }

        private Dictionary<string, object> ReadObject(ref Utf8JsonReader reader)
        {
            var dict = new Dictionary<string, object>();

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dict;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propName = reader.GetString();
                reader.Read();
                dict[propName] = ReadValue(ref reader);
            }

            return dict;
        }

        private object ReadValue(ref Utf8JsonReader reader)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var l) ? l : reader.GetDouble(),
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.StartObject => ReadObject(ref reader),
                JsonTokenType.StartArray => ReadArray(ref reader),
                _ => throw new JsonException()
            };
        }

        private List<object> ReadArray(ref Utf8JsonReader reader)
        {
            var list = new List<object>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                list.Add(ReadValue(ref reader));
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(Dictionary<string, object>), options);
        }
    }
}

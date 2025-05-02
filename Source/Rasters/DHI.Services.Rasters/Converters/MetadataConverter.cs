namespace DHI.Services.Rasters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class MetadataConverter : JsonConverter<IDictionary<string, object>>
    {
        /// <summary>
        ///     Override Read() method.
        /// </summary>
        /// <param name="reader">The reader is a reference type.</param>
        /// <param name="typeToConvert">The Type to convert.</param>
        /// <param name="options">The Json serializer options for deserialize.</param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, object>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();
                object value = null;

                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        value = reader.GetInt32();
                        break;
                    case JsonTokenType.String:
                        value = reader.GetString();
                        break;
                    case JsonTokenType.True:
                        value = true;
                        break;
                    case JsonTokenType.False:
                        value = false;
                        break;
                    case JsonTokenType.Null:
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                if (propertyName != null)
                {
                    dictionary.Add(propertyName, value);
                }
            }
            return dictionary;
        }

        /// <summary>
        ///     Override Write() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The Json Serializer options for serialization.</param>
        public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value, JsonSerializerOptions options)
        {
        }
    }
}
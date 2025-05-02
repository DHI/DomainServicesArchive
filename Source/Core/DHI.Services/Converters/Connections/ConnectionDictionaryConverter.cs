namespace DHI.Services.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    public class ConnectionDictionaryConverter : ConnectionDictionaryConverter<string> { }

    public class ConnectionDictionaryConverter<TKey> : JsonConverterFactory
        where TKey : notnull
    {
        private readonly string _typeDiscriminator;

        public ConnectionDictionaryConverter(string typeDiscriminator = "$type")
        {
            _typeDiscriminator = typeDiscriminator;
        }

        public Type dictionaryType => typeof(IDictionary<,>).MakeGenericType(typeof(TKey), typeof(IConnection));

        public override bool CanConvert(Type typeToConvert)
        {
            if ((typeToConvert == dictionaryType || dictionaryType.IsAssignableFrom(typeToConvert)) &&
                typeToConvert.GenericTypeArguments[0].Equals(typeof(TKey)) &&
                (typeof(IConnection).IsAssignableFrom(typeToConvert.GenericTypeArguments[1]) ||
                    typeof(BaseConnection).IsSubclassOf(typeToConvert.GenericTypeArguments[1])))
            {
                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new ConnectionDictionaryJsonConverter(_typeDiscriminator);
        }

        protected class ConnectionDictionaryJsonConverter : JsonConverter<IDictionary<TKey, IConnection>>
        {
            private readonly string _typeDiscriminator;

            public ConnectionDictionaryJsonConverter(string typeDiscriminator = "$type")
            {
                _typeDiscriminator = typeDiscriminator;
            }
            public override bool CanConvert(Type typeToConvert)
            {
                if (base.CanConvert(typeToConvert) &&
                    typeToConvert.GenericTypeArguments[0].Equals(typeof(TKey)) &&
                    typeToConvert.GenericTypeArguments[1].Equals(typeof(IConnection)))
                {
                    return true;
                }

                return false;
            }

            public override IDictionary<TKey, IConnection> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None) &&
                    reader.TokenType != JsonTokenType.StartObject)
                    return default;

                IDictionary<TKey, IConnection> dictionary;

                if (typeToConvert.IsGenericType)
                {
                    Type keyType = typeToConvert.GetGenericArguments()[0];
                    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, typeof(IConnection));
                    dictionary = (IDictionary<TKey, IConnection>)Activator.CreateInstance(dictionaryType);
                }
                else
                {
                    dictionary = (IDictionary<TKey, IConnection>)Activator.CreateInstance(typeToConvert);
                }

                var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
                jsonObject?.Remove(_typeDiscriminator);
                foreach (var entity in jsonObject)
                {
                    var connectionJson = entity.Value?.ToString();
                    var connectionType = Type.GetType(entity.Value?[_typeDiscriminator]?.ToString()) ??
                        throw new Exception($"Type discriminator for '{typeof(IConnection)}' is not found. Require for this type to be references.");

                    var connection = JsonSerializer.Deserialize(connectionJson,
                        connectionType!,
                        options);

                    var tKey = (TKey)Convert.ChangeType(entity.Key, typeof(TKey));
                    dictionary.Add(tKey, (IConnection)connection);
                }

                return dictionary;
            }

            public override void Write(Utf8JsonWriter writer, IDictionary<TKey, IConnection> value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, (IDictionary<TKey, IConnection>)null, options);
                        break;
                    default:
                        {
                            var dictionary = value;
                            writer.WriteStartObject();
                            if (dictionary.Count > 0)
                            {
                                writer.WritePropertyName(_typeDiscriminator);
                                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TKey), typeof(IConnection));
                                writer.WriteStringValue(dictionaryType.ResolveTypeFriendlyName());

                                var enumerator = dictionary.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    writer.WritePropertyName(enumerator.Current.Key.ToString());

                                    var currentValueType = enumerator.Current.Value.GetType();

                                    JsonSerializer.Serialize(writer,
                                        enumerator.Current.Value,
                                        currentValueType,
                                        options);
                                }
                            }
                            writer.WriteEndObject();
                            break;
                        }
                }
            }
        }
    }
}

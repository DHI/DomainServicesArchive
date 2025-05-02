namespace DHI.Services.Meshes.Converters
{
    using DHI.Services.Converters;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class DictionaryConverter<T> : DictionaryConverter
    {
        public DictionaryConverter() : base(typeof(T))
        {
        }

        public DictionaryConverter(Type dictionaryKeyType) : base(dictionaryKeyType, typeof(T))
        {
        }
    }

    internal class DictionaryConverter : JsonConverterFactory
    {
        private readonly Type _dictionaryKeyType;
        private readonly Type _dictionaryValueType;
        private Type _dictionaryType;

        public DictionaryConverter(Type valueType)
            : this(typeof(string), valueType)
        {
        }

        public DictionaryConverter(Type dictionaryKeyType, Type dictionaryValueType)
        {
            _dictionaryKeyType = dictionaryKeyType ?? throw new ArgumentNullException(nameof(dictionaryKeyType));
            _dictionaryValueType = dictionaryValueType ?? throw new ArgumentNullException(nameof(dictionaryValueType));
        }

        public Type DictionaryType => _dictionaryType ??= typeof(IDictionary<,>).MakeGenericType(_dictionaryKeyType, _dictionaryValueType);

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == DictionaryType ||
                DictionaryType.IsAssignableFrom(typeToConvert))
                return true;

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(DictionaryJsonConverter<,>).MakeGenericType(_dictionaryKeyType, _dictionaryValueType);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        protected class DictionaryJsonConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue?>>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(Dictionary<TKey, TValue>) ||
                    typeof(IDictionary<TKey, TValue>).IsAssignableFrom(typeToConvert);
            }

            public override IDictionary<TKey, TValue?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                IDictionary<TKey, TValue?> dictionary;

                if (typeToConvert.IsGenericType)
                {
                    var keyType = typeToConvert.GetGenericArguments()[0];
                    var valueType = typeToConvert.GetGenericArguments()[1];
                    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                    dictionary = (IDictionary<TKey, TValue?>)Activator.CreateInstance(dictionaryType);
                }
                else
                {
                    dictionary = (IDictionary<TKey, TValue?>)Activator.CreateInstance(typeToConvert);
                }

                if (reader.TokenType == JsonTokenType.Null)
                    return dictionary;

                int depth = reader.CurrentDepth;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.StartArray) { }
                    else if (reader.TokenType == JsonTokenType.EndObject ||
                        (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == depth))
                    {
                        return dictionary;
                    }
                    else
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException("JsonTokenType was not PropertyName");
                        }

                        var tKey = reader.GetString();
                        if (string.IsNullOrWhiteSpace(tKey))
                        {
                            throw new JsonException($"Failed to get Key property name for dictionary '{typeToConvert}'");
                        }
                        var key = (TKey)Convert.ChangeType(tKey, typeof(TKey));

                        reader.Read();

                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                                {
                                    new ObjectToInferredTypeConverter()
                                }
                        };
                        TValue value = JsonSerializer.Deserialize<TValue>(ref reader, serializer);
                        dictionary.Add(key, value);
                    }
                }

                return dictionary;
            }

            public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue?> value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, null as IDictionary<TKey, TValue>, options);
                        break;
                    default:
                        {
                            var dictionary = value;
                            writer.WriteStartObject();
                            var serializer = new JsonSerializerOptions
                            {
                                Converters =
                                {
                                    new ObjectToInferredTypeConverter(),
                                    new DoubleConverter()
                                }
                            };

                            foreach (var kvp in dictionary)
                            {
                                writer.WritePropertyName(kvp.Key!.ToString());

                                if (kvp.Value == null) writer.WriteNullValue();
                                else
                                {
                                    var serializedKvp = JsonSerializer.Serialize(kvp.Value, kvp.Value.GetType(), serializer);
                                    using var document = JsonDocument.Parse(serializedKvp);
                                    var root = document.RootElement.Clone();
                                    if (root.ValueKind == JsonValueKind.Object)
                                    {
                                        if (root.GetRawText().Equals("{}")) writer.WriteNullValue();
                                        else
                                            foreach (var element in root.EnumerateObject())
                                            {
                                                element.WriteTo(writer);
                                            }
                                    }
                                    else
                                        root.WriteTo(writer);
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
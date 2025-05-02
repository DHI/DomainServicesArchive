namespace DHI.Services.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     <seealso cref="JsonConverterFactory"/> to create <seealso cref="JsonConverter"/> for nested dictionary type. 
    /// </summary>
    /// <typeparam name="TKey">Type of dictionary key</typeparam>
    /// <typeparam name="TValue">Type of dictionary value</typeparam>
    public class DictionaryTypeResolverConverter<TKey, TValue> : JsonConverterFactory
    {
        private readonly string _typeDiscriminator;
        private readonly bool _isNestedDictionary;

        public DictionaryTypeResolverConverter(bool isNestedDictionary = false, string typeDiscriminator = "$type")
        {
            _typeDiscriminator = typeDiscriminator;
            _isNestedDictionary = isNestedDictionary;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(typeof(TKey), typeof(TValue));
            if (_isNestedDictionary)
            {
                var outerType = typeof(IDictionary<,>).MakeGenericType(typeof(string), dictionaryType);
                return typeToConvert == outerType;
            }
            else
                return typeToConvert == dictionaryType || dictionaryType.IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (_isNestedDictionary)
                return new NestedDictionaryJsonConverter(_typeDiscriminator);
            else
                return new DictionaryJsonConverter(_typeDiscriminator);
        }

        /// <summary>
        ///     <see cref="JsonConverter"/> for (de)serialized dictionary of <seealso cref="IDictionary{TKey, TValue}"/> with type discriminator />
        /// </summary>
        protected class DictionaryJsonConverter : JsonConverter<IDictionary<TKey, TValue?>>
        {
            private readonly string _typeDiscriminator;

            public DictionaryJsonConverter(string typeDiscriminator = "$type")
            {
                _typeDiscriminator = typeDiscriminator;
            }

            public override IDictionary<TKey, TValue?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                IDictionary<TKey, TValue?> dictionary;

                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TKey), typeof(TValue?));
                dictionary = (IDictionary<TKey, TValue?>)Activator.CreateInstance(dictionaryType);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName &&
                        reader.GetString() != _typeDiscriminator)
                    {
                        //string key = reader.GetString()!;
                        //var tKey = (TKey)Convert.ChangeType(key, typeof(TKey));  

                        TKey tKey = TryConvertKey(reader.GetString()!);

                        if (!reader.Read())
                            throw new JsonException("Dictionary required key property");


                        TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);

                        dictionary.Add(tKey, value);
                    }
                }

                return dictionary;
            }

            public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue?> dictionary, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WriteString(_typeDiscriminator, $"{dictionary.GetType().ResolveTypeFriendlyName()}, {dictionary.GetType().ResolveAssemblyName()}");

                foreach (var kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key.AsString());
                    //JsonSerializer.Serialize(writer, kvp.Value, options);

                    writer.WriteStartObject();
                    writer.WriteString(_typeDiscriminator, $"{kvp.Value.GetType().ResolveTypeFriendlyName()}, {kvp.Value.GetType().ResolveAssemblyName()}");

                    using var document = JsonDocument.Parse(JsonSerializer.Serialize(kvp.Value, kvp.Value.GetType(), options));
                    var root = document.RootElement.Clone();

                    foreach (var elements in root.EnumerateObject())
                    {
                        elements.WriteTo(writer);
                    }
                    writer.WriteEndObject();

                }
                writer.WriteEndObject();
            }
        }

        /// <summary>
        ///     <see cref="JsonConverter"/> for (de)serialized nested dictionary of <seealso cref="IDictionary{TKey, TValue}"/> with type discriminator />
        /// </summary>
        protected class NestedDictionaryJsonConverter : JsonConverter<IDictionary<string, IDictionary<TKey, TValue?>>>
        {
            private readonly string _typeDiscriminator;

            public NestedDictionaryJsonConverter(string typeDiscriminator = "$type")
            {
                _typeDiscriminator = typeDiscriminator;
            }

            public override IDictionary<string, IDictionary<TKey, TValue?>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var result = new Dictionary<string, IDictionary<TKey, TValue?>>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName &&
                        reader.GetString() != _typeDiscriminator)
                    {
                        string outerKey = reader.GetString()!;

                        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                            throw new JsonException($"Invalid JSON format for dictionary value for key: '{outerKey}'");

                        IDictionary<TKey, TValue?> innerDictionary;

                        var innerDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TKey), typeof(TValue?));
                        innerDictionary = (IDictionary<TKey, TValue?>)Activator.CreateInstance(innerDictionaryType);

                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonTokenType.EndObject)
                                break;

                            if (reader.TokenType == JsonTokenType.PropertyName &&
                                reader.GetString() != _typeDiscriminator)
                            {
                                //string innerKey = reader.GetString()!;
                                //var tKey = (TKey)Convert.ChangeType(innerKey, typeof(TKey));

                                TKey tKey = TryConvertKey(reader.GetString()!);

                                if (!reader.Read())
                                    throw new JsonException($"Invalid JSON format for dictionary value for key: '{tKey}'");

                                TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);

                                innerDictionary.Add(tKey, value);
                            }
                        }

                        result.Add(outerKey, innerDictionary);
                    }
                }

                return result;
            }

            public override void Write(Utf8JsonWriter writer, IDictionary<string, IDictionary<TKey, TValue?>> dictionary, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString(_typeDiscriminator, $"{dictionary.GetType().ResolveTypeFriendlyName()}, {dictionary.GetType().ResolveAssemblyName()}");

                foreach (var kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key);

                    writer.WriteStartObject();
                    writer.WriteString(_typeDiscriminator, $"{kvp.Value.GetType().ResolveTypeFriendlyName()}, {kvp.Value.GetType().ResolveAssemblyName()}");

                    foreach (var innerKvp in kvp.Value)
                    {
                        writer.WritePropertyName(innerKvp.Key.AsString());
                        //JsonSerializer.Serialize(writer, innerKvp.Value, options);
                        writer.WriteStartObject();
                        writer.WriteString(_typeDiscriminator, $"{innerKvp.Value.GetType().ResolveTypeFriendlyName()}, {innerKvp.Value.GetType().ResolveAssemblyName()}");

                        using var document = JsonDocument.Parse(JsonSerializer.Serialize(innerKvp.Value, innerKvp.Value.GetType(), options));
                        var root = document.RootElement.Clone();

                        foreach (var elements in root.EnumerateObject())
                        {
                            elements.WriteTo(writer);
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }
        }

        private static TKey TryConvertKey(string key)
        {
            try
            {
                return (TKey)Convert.ChangeType(key, typeof(TKey));
            }
            catch
            {
                return (TKey)System.ComponentModel.TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromString(key);
            }
        }
    }
}

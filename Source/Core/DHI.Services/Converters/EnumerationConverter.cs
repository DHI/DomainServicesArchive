namespace DHI.Services.Converters
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class EnumerationConverter : JsonConverterFactory
    {
        private readonly string _propertyName;

        public EnumerationConverter(string propertyName = "DisplayName")
        {
            _propertyName = propertyName;
        }

        protected string PropertyName => _propertyName;

        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOf(typeof(Enumeration));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new EnumerationJsonConverter();
        }

        public class EnumerationJsonConverter : JsonConverter<Enumeration>
        {
            private readonly string _propertyName = "DisplayName";

            public EnumerationJsonConverter(string propertyName = "DisplayName")
            {
                _propertyName = propertyName;
            }

            public override Enumeration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        return GetEnumerationFromValue(reader.GetInt32(), typeToConvert);
                    case JsonTokenType.String:
                        return GetEnumerationFromName(reader.GetString(), typeToConvert);
                    case JsonTokenType.StartObject:
                        if (JsonDocument.TryParseValue(ref reader, out var doc))
                        {
                            var root = doc.RootElement;
                            var prop = root.GetProperty(nameof(Enumeration.DisplayName));
                            return GetEnumerationFromName(prop.GetString(), typeToConvert);
                        }
                        return default;
                    case JsonTokenType.Null | JsonTokenType.None:
                        return default;
                    default:
                        throw new JsonException(
                            $"Unexpected token {reader.TokenType} when parsing the enumeration.");
                }
            }

            public override void Write(Utf8JsonWriter writer, Enumeration value, JsonSerializerOptions options)
            {
                if (value is null)
                {
                    writer.WriteNull(_propertyName);
                }
                else
                {
                    var name = value.GetType().GetProperty(_propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (name == null)
                    {
                        throw new JsonException($"Error while writing JSON for {value}");
                    }

                    writer.WriteStringValue(name.GetValue(value).ToString());
                }
            }

            private static Enumeration? GetEnumerationFromName(string? name, Type objectType)
            {
                try
                {
                    object result = default;
                    var methodInfo = typeof(Enumeration).GetMethod(
                        nameof(Enumeration.FromDisplayName),
                        BindingFlags.Static | BindingFlags.Public);

                    if (methodInfo == null)
                    {
                        throw new JsonException("Serialization is not supported");
                    }

                    var genericMethod = methodInfo.MakeGenericMethod(objectType);

                    var arguments = new[] { name };

                    //genericMethod.Invoke(null, arguments);
                    //return arguments[1] as Enumeration;

                    result = genericMethod.Invoke(null, arguments);
                    return result as Enumeration;
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Error converting name '{name}' to a enumeration.", ex);
                }
            }


            private static Enumeration? GetEnumerationFromValue(object? value, Type objectType)
            {
                try
                {
                    object result = default;
                    var methodInfo = typeof(Enumeration).GetMethod(
                        nameof(Enumeration.FromValue),
                        BindingFlags.Static | BindingFlags.Public);

                    if (methodInfo == null)
                    {
                        throw new JsonException("Serialization is not supported");
                    }

                    var genericMethod = methodInfo.MakeGenericMethod(objectType);

                    var arguments = new[] { value };

                    //genericMethod.Invoke(null, arguments);
                    //return arguments[1] as Enumeration;

                    result = genericMethod.Invoke(null, arguments);
                    return result as Enumeration;
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Error converting value '{value}' to a enumeration.", ex);
                }
            }
        }
    }
}

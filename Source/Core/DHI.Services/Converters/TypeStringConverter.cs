namespace DHI.Services.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class TypeStringConverter : JsonConverterFactory
    {
        private readonly bool _serializedFullName;

        public TypeStringConverter(bool serializeFullName = false)
        {
            _serializedFullName = serializeFullName;
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Type);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new TypeStringJsonConverter(_serializedFullName);
        }

        protected class TypeStringJsonConverter : JsonConverter<Type>
        {
            private readonly bool _serializedFullName;

            public TypeStringJsonConverter(bool serializedFullName = false)
            {
                _serializedFullName = serializedFullName;
            }

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Type);

            public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    throw new JsonException();

                var type = Type.GetType(reader.GetString());
                return type;
            }

            public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, (Type)null, options);
                        break;
                    default:
                        {
                            if (_serializedFullName)
                                writer.WriteStringValue(value.FullName);
                            else
                                writer.WriteStringValue($"{value.ResolveTypeFriendlyName()}, {value.ResolveAssemblyName()}");

                            break;
                        }
                }
            }
        }
    }
}

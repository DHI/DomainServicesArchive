namespace DHI.Services.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    public class ConnectionConverter : BaseTypeResolverConverter<IConnection>
    {
        public ConnectionConverter(string typeDiscriminator = "$type")
            : base(typeDiscriminator)
        {
        }

        public override bool CanConvert(Type typeToConvert) => typeof(IConnection).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(ConnectionJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, args: new object[] { TypeDiscriminator });
        }

        protected class ConnectionJsonConverter<TClass> : BaseTypeResolverJsonConverter<IConnection>
                where TClass : class, IConnection
        {
            public ConnectionJsonConverter(string typeDiscriminator = "$type")
                : base(typeDiscriminator)
            {
            }

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TClass);

            public override IConnection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                    return default;

                try
                {
                    Utf8JsonReader readerClone = reader;
                    var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
                    if (jsonObject.ContainsKey(TypeDiscriminator))
                    {
                        var discriminatorType = jsonObject[TypeDiscriminator].ToString();
                        jsonObject?.Remove(TypeDiscriminator);
                        var knownType = TryGetType(discriminatorType);

                        if (knownType != null)
                        {
                            return (TClass)JsonSerializer.Deserialize(jsonObject.ToJsonString(),
                                knownType!,
                                options);
                        }
                    }

                    //default ;
                    var serializer = new JsonSerializerOptions
                    {
                        Converters = { new FallbackJsonConverter<TClass>() }
                    };
                    return JsonSerializer.Deserialize<TClass>(jsonObject, serializer);
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Requires '{TypeDiscriminator}' property as type discriminator for deserialized '{typeof(TClass)}':'{typeof(IConnection)}' type.", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, IConnection value, JsonSerializerOptions options)
                => base.Write(writer, value, NewSerializerOptions<ConnectionConverter>(options));
        }
    }
}

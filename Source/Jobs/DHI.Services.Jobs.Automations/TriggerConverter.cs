namespace DHI.Services.Jobs.Automations
{
    using DHI.Services.Converters;
    using System;
    using System.Text.Json.Nodes;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class TriggerConverter : BaseTypeResolverConverter<ITrigger>
    {
        public TriggerConverter(string typeDiscriminator = "$type")
           : base(typeDiscriminator)
        {
        }

        public override bool CanConvert(Type typeToConvert) => typeof(ITrigger).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(TriggerJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, args: new object[] { TypeDiscriminator });
        }

        protected class TriggerJsonConverter<TClass> : BaseTypeResolverJsonConverter<ITrigger>
                where TClass : class, ITrigger
        {
            public TriggerJsonConverter(string typeDiscriminator = "$type")
                : base(typeDiscriminator)
            {
            }

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TClass);

            public override ITrigger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                        var knownType = TryGetType(discriminatorType);

                        jsonObject.Remove(TypeDiscriminator);

                        if (knownType != null)
                        {
                            var safeOptions = NewSerializerOptions<TriggerConverter>(options);

                            return (ITrigger)JsonSerializer.Deserialize(jsonObject.ToJsonString(), knownType, safeOptions);
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
                    throw new JsonException($"Requires '{TypeDiscriminator}' property as type discriminator for deserialized '{typeof(TClass)}':'{typeof(ITrigger)}' type.", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, ITrigger value, JsonSerializerOptions options)
                => base.Write(writer, value, NewSerializerOptions<TriggerConverter>(options));
        }
    }
}

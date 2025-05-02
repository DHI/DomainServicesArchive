namespace DHI.Services.Converters
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;


    /// <summary>
    ///     Create new converter for object implementing <typeparamref name="TClass"/> type with discriminator type defined
    /// </summary>
    /// <typeparam name="TClass">The object need convert</typeparam>
    public class TypeResolverConverter<TClass> : BaseTypeResolverConverter
        where TClass : notnull
    {
        private readonly string _typeDiscriminator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeDiscriminator">Specified discriminator for (de)serialize type</param>
        public TypeResolverConverter(string typeDiscriminator = "$type")
            : base(typeof(TClass), typeDiscriminator)
        {
            _typeDiscriminator = typeDiscriminator;
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TClass) ||
            typeof(TClass).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new TypeResolverJsonConverter(_typeDiscriminator);
        }

        protected class TypeResolverJsonConverter : BaseTypeResolverJsonConverter<TClass>
        {
            public TypeResolverJsonConverter(string typeDiscriminator = "$type")
                : base(typeDiscriminator)
            {
            }

            public override TClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                    return default;

                try
                {
                    Utf8JsonReader readerClone = reader;
                    var serializer = NewSerializerOptions<TypeResolverConverter<TClass>>(options);

                    var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
                    if (jsonObject?.ContainsKey(TypeDiscriminator) == true)
                    {
                        var discriminatorType = jsonObject[TypeDiscriminator]?.ToString();
                        jsonObject?.Remove(TypeDiscriminator);
                        var converterTypeName = $"{typeof(TClass).ResolveTypeFriendlyName()}, {typeof(TClass).ResolveAssemblyName()}";

                        var isKnownType = TryGetType(discriminatorType);
                        if (isKnownType != null
                            && (isKnownType.ResolveTypeFriendlyName() == typeToConvert.ResolveTypeFriendlyName()))
                        {
                            if (typeof(TClass).IsInterface || typeof(TClass).IsAbstract ||
                               typeToConvert.IsInterface || typeToConvert.IsAbstract)
                            {
                                var interfaceConverterType = typeof(ConcreteConverter<>).MakeGenericType(typeof(TClass), isKnownType);
                                var interFaceConverter = (JsonConverter)Activator.CreateInstance(interfaceConverterType);

                                serializer = new JsonSerializerOptions
                                {
                                    Converters = { interFaceConverter }
                                };

                                return (TClass)JsonSerializer.Deserialize(jsonObject, isKnownType, serializer);
                            }
                            else
                                return base.Read(ref readerClone, typeToConvert, serializer);
                        }

                        throw new JsonException($"Type defined in discriminator is '{discriminatorType}'. This type cannot be found or not matched with type '{converterTypeName}'");
                    }
                    else
                        return base.Read(ref readerClone, typeToConvert, serializer);
                }
                catch (Exception ex)
                {
                    throw new JsonException(ex.Message, ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, TClass? value, JsonSerializerOptions options)
                => base.Write(writer, value, NewSerializerOptions<TypeResolverConverter<TClass>>(options));

            private class ConcreteConverter<TConcrete> : JsonConverter<TClass>
                where TConcrete : class, TClass
            {
                public override bool CanConvert(Type typeToConvert)
                {
                    return typeof(TConcrete) == typeToConvert ||
                        typeof(TClass).IsAssignableFrom(typeToConvert);
                }

                public override TClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    var serializer = new JsonSerializerOptions(options);
                    serializer.Converters.Clear();
                    var converters = options.Converters.Where(converter => converter.GetType().Equals(typeof(ConcreteConverter<TConcrete>)) == false);
                    serializer.AddConverters(converters);
                    serializer.AddConverters(new FallbackJsonConverter<TConcrete>());

                    return JsonSerializer.Deserialize<TConcrete>(ref reader, serializer);
                }

                public override void Write(Utf8JsonWriter writer, TClass value, JsonSerializerOptions options)
                {
                    var serializer = new JsonSerializerOptions(options);
                    serializer.Converters.Clear();
                    var converters = options.Converters.Where(converter => converter.GetType().Equals(typeof(ConcreteConverter<TConcrete>)) == false);
                    serializer.AddConverters(converters);
                    serializer.AddConverters(new FallbackJsonConverter<TConcrete>());

                    JsonSerializer.Serialize(writer, value, serializer);
                }
            }
        }
    }

    ///// <summary>
    /////     Create new converter for object implementing <typeparamref name="TInterface"/> type with discriminator type defined
    ///// </summary>
    ///// <typeparam name="TInterface">The interface of object need implement</typeparam>
    //public class TypeResolverConverter<TInterface> : JsonConverterFactory
    //{
    //    private readonly string _typeDiscriminator;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="typeDiscriminator">Specified discriminator for (de)serialize type</param>
    //    public TypeResolverConverter(string typeDiscriminator = "$type")
    //    {
    //        _typeDiscriminator = typeDiscriminator;
    //    }

    //    protected string TypeDiscriminator => _typeDiscriminator;

    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        return typeof(TInterface).IsAssignableFrom(typeToConvert);
    //    }

    //    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        var converterType = typeof(TypeResolverJsonConverter<>).MakeGenericType(typeof(TInterface), typeToConvert);
    //        return (JsonConverter)Activator.CreateInstance(type: converterType, args: new object[] { _typeDiscriminator });
    //    }

    //    protected class TypeResolverJsonConverter<TClass> : JsonConverter<TInterface>
    //            where TClass : class, TInterface
    //    {
    //        private readonly string _typeDiscriminator;

    //        public TypeResolverJsonConverter(string typeDiscriminator = "$type")
    //        {
    //            _typeDiscriminator = typeDiscriminator;
    //        }
    //        protected string TypeDiscriminator => _typeDiscriminator;

    //        public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //        {
    //            if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
    //                return default;

    //            try
    //            {
    //                Utf8JsonReader readerClone = reader;
    //                var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
    //                if (jsonObject.ContainsKey(_typeDiscriminator))
    //                {
    //                    var jsonType = jsonObject[_typeDiscriminator].ToString();
    //                    jsonObject?.Remove(_typeDiscriminator);
    //                    var knownType = Type.GetType(jsonType);

    //                    if (knownType != null)
    //                    {
    //                        return (TClass)JsonSerializer.Deserialize(jsonObject.ToJsonString(),
    //                            knownType!,
    //                            options);
    //                    }
    //                }

    //                //Deserialized with original JsonSerializerOption but remove current JsonConverter for prevent loop.
    //                //Since original 'options' is immutable, wrap with new JsonSerializerOption to copy all setting
    //                //Clear all converters, re-adding but exclude current converter
    //                var serializer = new JsonSerializerOptions(options);
    //                serializer.Converters.Clear();
    //                var converters = options.Converters.Where(converter => converter.GetType().Equals(typeof(TypeResolverConverter<>).MakeGenericType(typeof(TInterface))) == false);
    //                serializer.AddConverters(converters);
    //                serializer.AddConverters(new FallbackJsonConverter<TClass>());
    //                return JsonSerializer.Deserialize<TClass>(jsonObject, serializer);
    //            }
    //            catch (Exception ex)
    //            {
    //                throw new JsonException($"Requires '{_typeDiscriminator}' property as type discriminator for deserialized '{typeof(TClass)}':'{typeof(TInterface)}' type.", ex);
    //            }
    //        }

    //        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
    //        {
    //            switch (value)
    //            {
    //                case null:
    //                    JsonSerializer.Serialize(writer, (TClass)null, options);
    //                    break;
    //                default:
    //                    {
    //                        var type = value.GetType();

    //                        writer.WriteStartObject();
    //                        writer.WriteString(_typeDiscriminator, $"{type.ResolveAssemblyFriendlyName()}, {type.Namespace}");

    //                        var serializer = new JsonSerializerOptions();
    //                        serializer.AddConverters(
    //                            new JsonStringEnumConverter(),
    //                            new TypeStringConverter(),
    //                            new ObjectToInferredTypeConverter(),
    //                            new AutoNumberToStringConverter(),
    //                            new PermissionConverter());

    //                        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, type, serializer));
    //                        var root = document.RootElement.Clone();

    //                        foreach (var elements in root.EnumerateObject())
    //                        {
    //                            elements.WriteTo(writer);

    //                            //var element = elements.Value;
    //                            //if (element.ValueKind != (JsonValueKind.Null | JsonValueKind.Undefined))
    //                            //{
    //                            //    if (element.ValueKind == JsonValueKind.Array)
    //                            //    {
    //                            //        if (element.GetArrayLength() > 0)
    //                            //        {
    //                            //            elements.WriteTo(writer);
    //                            //        }
    //                            //    }
    //                            //    else if (element.ValueKind == JsonValueKind.Object && (element.ValueKind != JsonValueKind.Null))
    //                            //    {
    //                            //        foreach (var elementObject in element.EnumerateObject())
    //                            //        {
    //                            //            elementObject.WriteTo(writer);
    //                            //        }
    //                            //    }
    //                            //    else
    //                            //        elements.WriteTo(writer);
    //                            //}
    //                        }

    //                        writer.WriteEndObject();
    //                        break;
    //                    }
    //            }
    //        }
    //    }

    //    #region ' Fallback converters '

    //    protected class FallbackJsonConverter<T> : JsonConverter<T> where T : notnull
    //    {
    //        private readonly Func<JsonSerializerOptions> _serializerOptions = () =>
    //        {
    //            var serializer = new JsonSerializerOptions()
    //            {
    //                IncludeFields = true,
    //                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    //            };
    //            serializer.AddConverters(
    //                new JsonStringEnumConverter(),
    //                new EnumerationConverter(),
    //                new TypeStringConverter(),
    //                new ObjectToInferredTypeConverter(),
    //                new AutoNumberToStringConverter());
    //            return serializer;
    //        };


    //        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //        {
    //            Utf8JsonReader readerClone = reader;
    //            if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
    //                return default;

    //            try
    //            {
    //                return JsonSerializer.Deserialize<T>(ref reader, _serializerOptions()) ?? default;
    //            }
    //            catch
    //            {
    //                try
    //                {
    //                    return JsonSerializer.Deserialize<T>(ref readerClone) ?? default;
    //                }
    //                catch
    //                {
    //                    throw;
    //                }
    //            }
    //        }

    //        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    //        {
    //            var writerClone = writer;
    //            try
    //            {
    //                JsonSerializer.Serialize(writer, value, _serializerOptions());
    //            }
    //            catch
    //            {
    //                try
    //                {
    //                    JsonSerializer.Serialize(writerClone, value);
    //                }
    //                catch
    //                {
    //                    throw;
    //                }
    //            }
    //        }
    //    }
    //    #endregion
    //}
}

namespace DHI.Services.Converters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    public abstract class BaseTypeResolverConverter<T> : BaseTypeResolverConverter
    {
        public BaseTypeResolverConverter(string typeDiscriminator = "$type")
            : base(typeof(T), typeDiscriminator)
        {
        }
    }

    public abstract class BaseTypeResolverConverter : JsonConverterFactory
    {
        private readonly Type _classType;
        private readonly string _typeDiscriminator;

        /// <summary>
        ///     Create new converter for <paramref name="classType"/> with discriminator type defined
        /// </summary>
        /// <param name="classType">The class type information specified on <paramref name="typeDiscriminator"/></param>
        /// <param name="typeDiscriminator">Type information for (de)serialize type. Default: "$type"</param>
        public BaseTypeResolverConverter(Type classType, string typeDiscriminator = "$type")
        {
            _classType = classType;
            _typeDiscriminator = typeDiscriminator;
        }

        protected string TypeDiscriminator => _typeDiscriminator;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == _classType;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(BaseTypeResolverJsonConverter<>).MakeGenericType(_classType);
            return (JsonConverter)Activator.CreateInstance(type: converterType, args: new object[] { _typeDiscriminator });
        }

        protected abstract class BaseTypeResolverJsonConverter<T> : JsonConverter<T>
        {
            private readonly string _typeDiscriminator;

            public BaseTypeResolverJsonConverter(string typeDiscriminator = "$type")
            {
                _typeDiscriminator = typeDiscriminator;
            }

            protected string TypeDiscriminator => _typeDiscriminator;

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                    return default;

                try
                {
                    var jsonObject = JsonNode.Parse(ref reader)?.AsObject();

                    //try remove property type discriminator '$type' to clean up json object from legacy convention name
                    jsonObject?.Remove(_typeDiscriminator);

                    var serializer = NewSerializerOptions<BaseTypeResolverConverter>(options);
                    return JsonSerializer.Deserialize<T>(jsonObject, serializer);
                }
                catch (Exception ex)
                {
                    throw new JsonException(ex.Message, ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, default(T), options);
                        break;
                    default:
                        {
                            var type = value.GetType();

                            writer.WriteStartObject();
                            writer.WriteString(_typeDiscriminator, $"{type.ResolveTypeFriendlyName()}, {type.ResolveAssemblyName()}");

                            //var serializer = new JsonSerializerOptions
                            //{
                            //    Converters = { new FallbackJsonConverter<T>() }
                            //};

                            using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                            var root = document.RootElement.Clone();

                            foreach (var elements in root.EnumerateObject())
                            {
                                elements.WriteTo(writer);
                            }

                            writer.WriteEndObject();
                            break;
                        }
                }
            }

            /// <summary>
            ///     Create new <see cref="JsonSerializerOptions"/> using original options and remove current converter.
            ///     This is needed to prevent loop.
            /// </summary>
            /// <typeparam name="TConverter">Converter type to be excluded in new <see cref="JsonSerializerOptions"/></typeparam>
            /// <param name="options">Source of <see cref="JsonSerializerOptions"/></param>
            /// <param name="fallbackConverters">Collection for fallback converter</param>
            /// <returns></returns>
            protected JsonSerializerOptions NewSerializerOptions<TConverter>(JsonSerializerOptions options, params JsonConverter[] fallbackConverters)
                where TConverter : BaseTypeResolverConverter
            {
                //Deserialized with original JsonSerializerOption but remove current JsonConverter to prevent loop.
                //Since original 'options' is immutable, wrap with new JsonSerializerOption to copy all settings.
                //Clear all converters and re-adding but exclude current converter
                var serializer = new JsonSerializerOptions(options);
                serializer.Converters.Clear();
                var converters = options.Converters.Where(converter => converter.GetType().Equals(typeof(TConverter)) == false);
                serializer.AddConverters(converters);

                if (fallbackConverters?.Any() == true)
                    serializer.AddConverters(fallbackConverters);
                else
                    serializer.AddConverters(new FallbackJsonConverter<T>());

                return serializer;
            }
        }


        protected static Type TryGetType(string typeDiscriminator)
        {
            var returnType = Type.GetType(typeDiscriminator);
            if (returnType == null)
            {
                var names = typeDiscriminator.Split(',').Select(x => x.Trim()).ToArray();

                if (returnType == null)
                {
                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var type = Directory.GetFiles(path, "DHI.*.dll")
                        .Select(Assembly.LoadFile)
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(type => type.Namespace?.StartsWith(names[1]) == true)
                        .Where(type => type.ResolveTypeFriendlyName() == names[0])
                        .FirstOrDefault();

                    if (type != null)
                        returnType = Type.GetType(type.AssemblyQualifiedName);
                }
            }

            return returnType;
        }

        #region ' Fallback converters ' 
        protected class FallbackJsonConverter<T> : JsonConverter<T> where T : notnull
        {
            private readonly Func<JsonSerializerOptions> _serializerOptions = () =>
            {
                var serializer = new JsonSerializerOptions()
                {
                    IncludeFields = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                serializer.AddConverters(
                    new JsonStringEnumConverter(),
                    new EnumerationConverter(),
                    new TypeStringConverter(),
                    new ObjectToInferredTypeConverter(),
                    new AutoNumberToStringConverter(),
                    new PermissionConverter());
                return serializer;
            };


            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Utf8JsonReader readerClone = reader;
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                    return default;

                try
                {
                    return JsonSerializer.Deserialize<T>(ref reader, _serializerOptions()) ?? default;
                }
                catch
                {
                    try
                    {
                        return JsonSerializer.Deserialize<T>(ref readerClone) ?? default;
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var writerClone = writer;
                try
                {
                    JsonSerializer.Serialize(writer, value, _serializerOptions());
                }
                catch
                {
                    try
                    {
                        JsonSerializer.Serialize(writerClone, value);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }
        #endregion
    }
}

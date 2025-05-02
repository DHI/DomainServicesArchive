namespace DHI.Services.Rasters.WebApi
{
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using DHI.Services.Converters;
    using DHI.Services.Rasters.Zones;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '
        private static readonly System.Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;

        private SerializerOptionsDefault()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null,
            };
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            _serializerOptions.Converters.Add(new EnumerationConverter());
            _serializerOptions.Converters.Add(new TypeStringConverter());
            _serializerOptions.Converters.Add(new ObjectToInferredTypeConverter());
            _serializerOptions.Converters.Add(new AutoNumberToStringConverter());
            _serializerOptions.Converters.Add(new PermissionConverter());
            _serializerOptions.Converters.Add(new ConnectionDictionaryConverter());
            _serializerOptions.Converters.Add(new ConnectionConverter());
            _serializerOptions.Converters.Add(new TypeResolverConverter<Parameters>());
            _serializerOptions.Converters.Add(new TypeResolverConverter<ConnectionType>());
            _serializerOptions.Converters.Add(new TypeResolverConverter<ProviderArgument>());
            _serializerOptions.Converters.Add(new TypeResolverConverter<ProviderType>());
            _serializerOptions.Converters.Add(new TypeResolverConverter<ConnectionType>());
            _serializerOptions.Converters.Add(new ZoneConverter());
            _serializerOptions.Converters.Add(new ZoneTypeConverter());
            _serializerOptions.Converters.Add(new ZoneDictionaryConverter());
            _serializerOptions.Converters.Add(new PixelValueTypeConverter());
        }
        #endregion

        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        ///     <seealso cref="JsonSerializerOptions"/> with pre-configured  for <see cref="ConnectionRepository"/>
        /// </summary>
        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}

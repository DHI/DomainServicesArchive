namespace ServicesRegistrationList
{
    using DHI.Services.Converters;
    using DHI.Services;
    using System.Text.Json.Serialization;
    using System.Text.Json;

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
            _serializerOptions.AddConverters(
                new JsonStringEnumConverter(),
                new EnumerationConverter(),
                new TypeStringConverter(),
                new ObjectToInferredTypeConverter(),
                new AutoNumberToStringConverter(),
                new PermissionConverter(),
                new ConnectionDictionaryConverter(),
                new ConnectionConverter(),
                new TypeResolverConverter<Parameters>(),
                new TypeResolverConverter<ConnectionType>(),
                new TypeResolverConverter<ProviderArgument>(),
                new TypeResolverConverter<ProviderType>(),
                new TypeResolverConverter<ConnectionType>()
             );
        }
        #endregion

        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        ///     <seealso cref="JsonSerializerOptions"/> with pre-configured  for <see cref="ConnectionRepository"/>
        /// </summary>
        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}

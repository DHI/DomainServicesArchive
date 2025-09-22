namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Services.Jobs.Automations;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '

        private static readonly System.Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;

        #endregion

        private SerializerOptionsDefault()
        {
            _serializer = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            };
            _serializer.AddConverters(_defaultJsonConverters());
        }

        private readonly JsonSerializerOptions _serializer;

        private static readonly Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new JsonStringEnumConverter(),
                new ObjectToInferredTypeConverter(),
                new DictionaryTypeResolverConverter<string, Host>(),
                new DictionaryTypeResolverConverter<Guid, Job>(),
                new DictionaryTypeResolverConverter<string, Automation<string>>(isNestedDictionary: true),
                new TriggerConverter(),
                new JsonCollectionItemConverter<ITrigger, TriggerConverter>(),
                new TypeResolverConverter<Parameters>(),
                new EnumerationConverter(),
                new TypeStringConverter(),
                new AutoNumberToStringConverter(),
                new PermissionConverter(),
                new ConnectionDictionaryConverter(),
                new ConnectionConverter(),
                new TypeResolverConverter<ProviderArgument>(),
                new TypeResolverConverter<ProviderType>(),
                new TypeResolverConverter<ConnectionType>()
            };
        };

        public static JsonSerializerOptions Options => instance._serializer;
    }
}
namespace DHI.Services.Places.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Places.Converters;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '
        private static readonly System.Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;

        private SerializerOptionsDefault()
        {
            _serializer = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            _serializer.AddConverters(_defaultJsonConverters());
        }
        #endregion 

        private readonly JsonSerializerOptions _serializer;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new PlaceConverter(),
                new DataSourceConverter(),
                new IndicatorConverter(),
                new TimeIntervalConverter(),
                new FeatureIdConverter<string>(),
                new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Place>()
            };
        };

        public static JsonSerializerOptions Options => instance._serializer;
    }
}
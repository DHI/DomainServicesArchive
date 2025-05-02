namespace DHI.Services.Documents.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Documents.Converters;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '
        private static readonly Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;
        #endregion 

        private SerializerOptionsDefault()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            foreach (var converter in _defaultJsonConverters())
            {
                _serializerOptions.Converters.Add(converter);
            }
        }

        private readonly JsonSerializerOptions _serializerOptions;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new JsonStringEnumConverter(),
                new DocumentConverter(),
                new DocumentConverter<string>(),
            };
        };

        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}

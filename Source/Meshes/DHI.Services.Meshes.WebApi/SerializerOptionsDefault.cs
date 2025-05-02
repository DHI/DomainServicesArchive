namespace DHI.Services.Meshes.WebApi
{
    using DHI.Services.Converters;
    using DHI.Services.TimeSeries.Converters;
    using DHI.Spatial.GeoJson;
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '
        private static readonly Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;

        private SerializerOptionsDefault()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            };

            _serializerOptions.AddConverters(_defaultJsonConverters());
        }
        #endregion 

        private readonly JsonSerializerOptions _serializerOptions;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new PositionConverter(),
                new GeometryConverter(),
                new FeatureCollectionConverter(),
                new TimeSeriesDataConverter<double>(),
                new ObjectToInferredTypeConverter(),
                new DateRangeConverter()
            };
        };

        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}

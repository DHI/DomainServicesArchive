namespace DHI.Services.TimeSeries.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///    Default <seealso cref="JsonSerializerOptions"/> with pre-configured <seealso cref="JsonConverter"/> specifically for TimeSeries
    /// </summary>
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
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _serializerOptions.AddConverters(_defaultJsonConverters());
        }
        #endregion 

        private readonly JsonSerializerOptions _serializerOptions;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new DHI.Services.Converters.EnumerationConverter(),
                new Converters.TimeSeriesConverter<string, double>(),
                new Converters.DataPointConverter<double, int?>(),
                new Converters.TimeSeriesDataWFlagConverter<double, Dictionary<string, object>>(),
                new Converters.TimeSeriesDataWFlagConverter<double, int?>(),
                new Converters.TimeSeriesDataWFlagConverter<Vector<double>, int?>(),
                new Converters.TimeSeriesDataConverter<double>(),
                new Converters.TimeSeriesDataConverter<Vector<double>>()
            };
        };

        /// <summary>
        ///     <seealso cref="JsonSerializerOptions"/> with pre-configured <seealso cref="JsonConverter"/> specifically for TimeSeries
        /// </summary>
        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}
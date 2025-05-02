namespace DHI.Services.Spreadsheets.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

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
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            _serializer.AddConverters(_defaultJsonConverters());
        }

        private readonly JsonSerializerOptions _serializer;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new MultidimensionalArrayConverterFactory(),
                new Converters.SpreadsheetConverter<string>(),
                new Converters.TwoDimensionalArrayConverter<object>(),
                new Converters.TwoDimensionalArrayConverter<double>(),
                new Converters.TwoDimensionalArrayConverter<CellFormat>(),
                new Converters.DoubleConverter(),
                new DHI.Services.Converters.ObjectToInferredTypeConverter(),
            };
        };

        public static JsonSerializerOptions Options => instance._serializer;
    }
}
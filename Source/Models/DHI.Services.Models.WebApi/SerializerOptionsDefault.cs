namespace DHI.Services.Models.WebApi
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using DHI.Services.Converters;
    using DHI.Services.TimeSeries.Converters;
    using DHI.Services.TimeSeries;

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
                IgnoreReadOnlyProperties = true,
                PropertyNameCaseInsensitive = true
            };
            _serializerOptions.AddConverters(
                new DateRangeConverter(),
                new JsonStringEnumConverter(),
                new EnumerationConverter(),
                new TypeStringConverter(),
                new ObjectToInferredTypeConverter(),
                new AutoNumberToStringConverter(),
                new PermissionConverter(),
                new DictionaryTypeResolverConverter<string, IModelDataReader>(),
                new DictionaryTypeResolverConverter<string, Scenario>(),
                new TypeStringConverter(),
                new TypeResolverConverter<Parameters>(),
                new TypeResolverConverter<ConnectionType>(),
                new TypeResolverConverter<ProviderArgument>(),
                new TypeResolverConverter<ProviderType>(),
                new TypeResolverConverter<ConnectionType>(),
                new TimeSeriesDataWFlagConverter<double, Dictionary<string, object>>(),
                new TimeSeriesDataWFlagConverter<double, int?>(),
                new TimeSeriesDataWFlagConverter<Vector<double>, int?>(),
                new TimeSeriesDataConverter<double>(),
                new TimeSeriesDataConverter<Vector<double>>()
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

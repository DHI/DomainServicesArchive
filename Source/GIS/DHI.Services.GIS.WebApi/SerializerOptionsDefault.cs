namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Maps;
    using DHI.Spatial.GeoJson;
    using DHI.Services.Converters;

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
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //ReferenceHandler = ReferenceHandler.IgnoreCycles,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            foreach (var converter in _defaultJsonConverters())
            {
                _serializerOptions.Converters.Add(converter);
            }

            // _serializerOptions.AddDefaultJsonConverter();
        }
        #endregion 

        private readonly JsonSerializerOptions _serializerOptions;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new PositionConverter(),
                new AttributeConverter(),
                new AssociationConverter(),
                new FeatureConverter(),
                new FeatureInfoConverter(),
                new FeatureCollectionConverter(),
                new GeometryConverter(),
                new GeometryCollectionConverter(),

                new GIS.Converters.MapLayerConverter(),
                new GIS.Converters.MapStyleConverter(),
                new GIS.Converters.MapStyleBandConverter(),
                new GIS.Converters.TileConverter(),
                new GIS.Converters.TileImageConverter(),
                new GIS.Converters.FeatureCollectionInfoConverter<string>(),

                new DHI.Services.Converters.ObjectToInferredTypeConverter(),

                new DHI.Services.Converters.DictionaryTypeResolverConverter<string, MapStyle>(),
            };
        };

        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}

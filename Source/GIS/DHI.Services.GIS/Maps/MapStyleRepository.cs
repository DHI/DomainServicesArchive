namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.GIS.Converters;

    public class MapStyleRepository : JsonRepository<MapStyle, string>, IMapStyleRepository
    {
        static readonly Func<JsonSerializerOptions> _requiredSerializerOptions = () =>
        {
            return new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new MapLayerConverter(),
                    new MapStyleConverter(),
                    new MapStyleBandConverter(),
                    new PaletteConverter<double>(),
                    new TileConverter(),
                    new TileImageConverter(),
                    //new DHI.Services.Converters.TypeResolverConverter<MapStyle>(),
                    //new DHI.Services.Converters.TypeResolverConverter<MapStyleBand>(),
                    new DHI.Services.Converters.DictionaryTypeResolverConverter<string, MapStyle>()
                }
            };
        };

        public MapStyleRepository(string filePath) : this(filePath, _requiredSerializerOptions())
        {
        }

        public MapStyleRepository(string fileName, JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions = null)
            : base(fileName, serializerOptions, deserializerOptions)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredSerializerOptions().Converters);
                deserializer.AddConverters(_requiredSerializerOptions().Converters);
            });
        }
    }
}
namespace DHI.Services.Places.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class PlaceConverter : PlaceConverter<string>
    {
        public PlaceConverter()
        {
        }
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Place);

        //public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        //{
        //    return new PlaceJsonConverter();
        //}

        //protected class PlaceJsonConverter : PlaceJsonConverter<string>
        //{
        //    public override bool CanConvert(Type typeToConvert) => typeof(Place) == typeToConvert;

        //    public override Place? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        //    {
        //        if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
        //            return default;

        //        if (reader.TokenType != JsonTokenType.StartObject)
        //            throw new JsonException($"JsonTokenType was of type '{reader.TokenType}', only objects are supported");

        //        if (JsonDocument.TryParseValue(ref reader, out var doc))
        //        {
        //            var root = doc.RootElement;

        //            string? id = default;
        //            if (root.TryGetProperty(nameof(Place.Id), out var idElement))
        //            {
        //                if (idElement.ValueKind == JsonValueKind.Null)
        //                {
        //                    throw new JsonException($"'{typeof(Place)}' required non-null '{nameof(Place.Id)}' property");
        //                }
        //                id = idElement.GetString();
        //            }

        //            string? name = string.Empty;
        //            if (root.TryGetProperty(nameof(Place.Name), out var nameElement))
        //            {
        //                if (nameElement.ValueKind == JsonValueKind.Null)
        //                {
        //                    throw new JsonException($"'{typeof(Place)}' required non-null '{nameof(Place.Name)}' property");
        //                }
        //                name = nameElement.GetString();
        //            }

        //            string? group = null;
        //            if (root.TryGetProperty(nameof(Place.Group), out var groupElement))
        //            {
        //                group = groupElement.GetString();
        //            }

        //            FeatureId? featureCollectionId = default;
        //            if (root.TryGetProperty(nameof(Place.FeatureId), out var featureIdElement))
        //            {
        //                var serializer = new JsonSerializerOptions
        //                {
        //                    Converters = { new FeatureIdConverter<string>() }
        //                };
        //                featureCollectionId = featureIdElement.Deserialize<FeatureId>(serializer);
        //            }

        //            var place = new Place(id!, name!, featureCollectionId!, group);

        //            if (root.TryGetProperty(nameof(Place.Metadata), out var metadataElement))
        //            {
        //                var serializer = new JsonSerializerOptions
        //                {
        //                    Converters = { new ObjectToInferredTypeConverter() }
        //                };
        //                var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>(serializer) ??
        //                    new Dictionary<string, object?>();

        //                foreach (var item in metadatas)
        //                    place.Metadata.Add(item.Key, item.Value);

        //            }
        //            if (root.TryGetProperty(nameof(Place.Indicators), out var indicatorElement))
        //            {
        //                var serializer = new JsonSerializerOptions
        //                {
        //                    Converters =
        //                    {
        //                        new IndicatorConverter(),
        //                        new DataSourceConverter(),
        //                        new TimeIntervalConverter(),
        //                    }
        //                };
        //                var indicators = indicatorElement.Deserialize<IDictionary<string, Indicator>>(serializer) ??
        //                    new Dictionary<string, Indicator>();

        //                foreach (var item in indicators.Where(x => x.Key != TypeDiscriminator))
        //                    place.Indicators.Add(item.Key, item.Value);
        //            }

        //            return place;
        //        }

        //        return default;
        //    }

        //    public override void Write(Utf8JsonWriter writer, Place? value, JsonSerializerOptions options) =>
        //        base.Write(writer, value, NewSerializerOptions<PlaceConverter>(options));
        //}
    }

    public class PlaceConverter<TCollectionId> : BaseTypeResolverConverter
            where TCollectionId : notnull
    {
        public PlaceConverter() : base(typeof(Place<TCollectionId>))
        {
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Place<TCollectionId>);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new PlaceJsonConverter();
        }

        protected class PlaceJsonConverter : BaseTypeResolverJsonConverter<Place<TCollectionId>?>
        {
            public override bool CanConvert(Type typeToConvert) => typeof(Place<TCollectionId>) == typeToConvert;

            public override Place<TCollectionId>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == (JsonTokenType.Null | JsonTokenType.None))
                    return default;

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException($"JsonTokenType was of type '{reader.TokenType}', only objects are supported");

                if (JsonDocument.TryParseValue(ref reader, out var doc))
                {
                    var root = doc.RootElement;

                    string? id = default;
                    if (root.TryGetProperty(nameof(Place<TCollectionId>.Id), out var idElement))
                    {
                        if (idElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Place<TCollectionId>)}' required non-null '{nameof(Place<TCollectionId>.Id)}' property");
                        }
                        id = idElement.GetString();
                    }

                    string? name = string.Empty;
                    if (root.TryGetProperty(nameof(Place<TCollectionId>.Name), out var nameElement))
                    {
                        if (nameElement.ValueKind == JsonValueKind.Null)
                        {
                            throw new JsonException($"'{typeof(Place<TCollectionId>)}' required non-null '{nameof(Place<TCollectionId>.Name)}' property");
                        }
                        name = nameElement.GetString();
                    }

                    string? group = null;
                    if (root.TryGetProperty(nameof(Place<TCollectionId>.Group), out var groupElement))
                    {
                        group = groupElement.GetString();
                    }

                    FeatureId<TCollectionId>? featureCollectionId = default;
                    if (root.TryGetProperty(nameof(Place<TCollectionId>.FeatureId), out var featureIdElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new FeatureIdConverter<TCollectionId>() }
                        };
                        featureCollectionId = featureIdElement.Deserialize<FeatureId<TCollectionId>>(serializer);
                    }

                    var place = new Place<TCollectionId>(id!, name!, featureCollectionId!, group);

                    if (root.TryGetProperty(nameof(Place<TCollectionId>.Metadata), out var metadataElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters = { new ObjectToInferredTypeConverterPatch() }
                        };
                        var metadatas = metadataElement.Deserialize<IDictionary<string, object?>>(serializer) ??
                            new Dictionary<string, object?>();

                        foreach (var item in metadatas)
                            place.Metadata.Add(item.Key, item.Value);

                    }
                    if (root.TryGetProperty(nameof(Place<TCollectionId>.Indicators), out var indicatorElement))
                    {
                        var serializer = new JsonSerializerOptions
                        {
                            Converters =
                            {
                                new IndicatorConverter(),
                                new DataSourceConverter(),
                                new TimeIntervalConverter(),
                                new DictionaryTypeResolverConverter<string, Indicator>()
                            }
                        };
                        var indicators = indicatorElement.Deserialize<IDictionary<string, Indicator>>(serializer) ??
                            new Dictionary<string, Indicator>();

                        foreach (var item in indicators.Where(x => x.Key != TypeDiscriminator))
                            place.Indicators.Add(item.Key, item.Value);
                    }

                    return place;
                }

                return default;
            }

            //public override void Write(Utf8JsonWriter writer, Place<TCollectionId>? value, JsonSerializerOptions options) =>
            //    base.Write(writer, value, NewSerializerOptions<PlaceConverter<TCollectionId>>(options)); 

            public override void Write(Utf8JsonWriter writer, Place<TCollectionId>? value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        JsonSerializer.Serialize(writer, (Place<TCollectionId>?)null, options);
                        break;
                    default:
                        {
                            var serializer = new JsonSerializerOptions
                            {
                                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                                Converters =
                                {
                                    new JsonStringEnumConverter(),
                                    new IndicatorConverter(),
                                    new DataSourceConverter(),
                                    new TimeIntervalConverter(),
                                    new FeatureIdConverter<TCollectionId>(),
                                    new PermissionConverter(),
                                    new DictionaryTypeResolverConverter<string, Indicator>(),
                                    new ObjectToInferredTypeConverterPatch(),
                                }
                            };

                            var place = value;
                            writer.WriteStartObject();

                            writer.WriteString(TypeDiscriminator, $"{value.GetType().ResolveTypeFriendlyName()}, {value.GetType().ResolveAssemblyName()}");

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Id));
                            JsonSerializer.Serialize(writer, place.Id);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Name));
                            writer.WriteStringValue(place.Name);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Group));
                            writer.WriteStringValue(place.Group);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.FeatureId));
                            JsonSerializer.Serialize(writer, place.FeatureId, serializer);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Indicators));
                            JsonSerializer.Serialize(writer, place.Indicators, serializer);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Metadata));
                            JsonSerializer.Serialize(writer, place.Metadata, serializer);

                            writer.WritePropertyName(nameof(Place<TCollectionId>.Permissions));
                            JsonSerializer.Serialize(writer, place.Permissions, serializer);

                            writer.WriteEndObject();
                            break;
                        }
                }
            }
        }
    }
}

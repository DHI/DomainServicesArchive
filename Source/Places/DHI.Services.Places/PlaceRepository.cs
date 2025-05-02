namespace DHI.Services.Places
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Places.Converters;

    public class PlaceRepository<TCollectionId> : GroupedJsonRepository<Place<TCollectionId>>, IPlaceRepository<TCollectionId> where TCollectionId : notnull
    {
        static readonly Func<JsonSerializerOptions> _requiredSerializerOptions = () =>
        {
            return new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters =
                {
                    new PlaceConverter<TCollectionId>(),
                    new DataSourceConverter(),
                    new IndicatorConverter(),
                    new TimeIntervalConverter(),
                    new FeatureIdConverter<TCollectionId>(),
                    new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Place<TCollectionId>>(isNestedDictionary: true),
                }
            };
        };

        public PlaceRepository(string filePath) : this(filePath, null)
        {
        }

        public PlaceRepository(string filePath,
            IEqualityComparer<string>? comparer = null)
            : this(filePath, serializerOptions: _requiredSerializerOptions(), comparer: comparer)
        {
        }

        public PlaceRepository(string fileName,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions? deserializerOptions = null,
            IEqualityComparer<string>? comparer = null)
            : base(fileName, serializerOptions, deserializerOptions, comparer)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredSerializerOptions().Converters);
                deserializer.AddConverters(_requiredSerializerOptions().Converters);
            });
        }

        public Maybe<Indicator> GetIndicator(string placeId, string type, ClaimsPrincipal? user = null)
        {
            var maybe = Get(placeId, user);
            if (!maybe.HasValue)
            {
                return Maybe.Empty<Indicator>();
            }

            return maybe.Value.Indicators.ContainsKey(type) ? maybe.Value.Indicators[type].ToMaybe() : Maybe.Empty<Indicator>();
        }

        public IDictionary<string, IDictionary<string, Indicator>> GetIndicators(ClaimsPrincipal? user = null)
        {
            var indicators = new Dictionary<string, IDictionary<string, Indicator>>();
            var places = GetAll(user);
            foreach (var place in places)
            {
                foreach (var indicator in place.Indicators)
                {
                    if (!indicators.ContainsKey(place.Id))
                    {
                        indicators.Add(place.Id, new Dictionary<string, Indicator> { { indicator.Key, indicator.Value } });
                    }
                    else
                    {
                        indicators[place.Id].Add(indicator.Key, indicator.Value);
                    }
                }
            }

            return indicators;
        }

        public IDictionary<string, Indicator> GetIndicatorsByPlace(string placeId, ClaimsPrincipal? user = null)
        {
            if (Contains(placeId, user))
            {
                var maybe = Get(placeId, user);
                return maybe.HasValue ? maybe.Value.Indicators : new Dictionary<string, Indicator>();
            }

            return new Dictionary<string, Indicator>();
        }

        public IDictionary<string, Indicator> GetIndicatorsByType(string type, ClaimsPrincipal? user = null)
        {
            var indicators = new Dictionary<string, Indicator>();
            var places = GetAll();
            foreach (var place in places)
            {
                foreach (var indicator in place.Indicators.Where(indicator => indicator.Key == type))
                {
                    indicators.Add(place.Id, indicator.Value);
                }
            }
            return indicators;
        }

        public IDictionary<string, Indicator> GetIndicatorsByGroupAndType(string group, string type, ClaimsPrincipal? user = null)
        {
            var indicators = new Dictionary<string, Indicator>();
            var places = GetByGroup(group);
            foreach (var place in places)
            {
                foreach (var indicator in place.Indicators.Where(indicator => indicator.Key == type))
                {
                    indicators.Add(place.Id, indicator.Value);
                }
            }

            return indicators;
        }

        public IDictionary<string, IDictionary<string, Indicator>> GetIndicatorsByGroup(string group, ClaimsPrincipal? user = null)
        {
            var indicators = new Dictionary<string, IDictionary<string, Indicator>>();
            var places = GetByGroup(group);
            foreach (var place in places)
            {
                foreach (var indicator in place.Indicators)
                {
                    if (!indicators.ContainsKey(place.Id))
                    {
                        indicators.Add(place.Id, new Dictionary<string, Indicator> { { indicator.Key, indicator.Value } });
                    }
                    else
                    {
                        indicators[place.Id].Add(indicator.Key, indicator.Value);
                    }
                }
            }

            return indicators;
        }
    }

    public class PlaceRepository : PlaceRepository<string>
    {
        static readonly Func<JsonSerializerOptions> _requiredSerializerOptions = () =>
        {
            return new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters =
                {
                    new PlaceConverter(),
                    new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Place>(isNestedDictionary: true),
                }
            };
        };
        public PlaceRepository(string filePath) : this(filePath, null)
        {
        }

        public PlaceRepository(string filePath,
            IEqualityComparer<string>? comparer = null)
            : base(filePath, comparer: comparer)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredSerializerOptions().Converters);
                deserializer.AddConverters(_requiredSerializerOptions().Converters);
            });
        }

        public PlaceRepository(string fileName,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions? deserializerOptions = null,
            IEqualityComparer<string>? comparer = null)
            : base(fileName, serializerOptions, deserializerOptions, comparer)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredSerializerOptions().Converters);
                deserializer.AddConverters(_requiredSerializerOptions().Converters);
            });
        }
    }
}
namespace DHI.Services.Places.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class FakePlaceRepository : FakeGroupedRepository<Place<string>, string>, IPlaceRepository<string>
    {
        public FakePlaceRepository(IEnumerable<Place<string>> places) : base(places)
        {
        }

        public IDictionary<string, IDictionary<string, Indicator>> GetIndicators(ClaimsPrincipal user = null)
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

        public IDictionary<string, Indicator> GetIndicatorsByPlace(string placeId, ClaimsPrincipal user = null)
        {
            var indicators = new Dictionary<string, Indicator>();
            var place = Get(placeId);
            if (place.HasValue)
            {
                indicators = place.Value.Indicators;
            }
            return indicators;
        }

        public IDictionary<string, Indicator> GetIndicatorsByType(string type, ClaimsPrincipal user = null)
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

        public IDictionary<string, Indicator> GetIndicatorsByGroupAndType(string group, string type, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<Indicator> GetIndicator(string placeId, string type, ClaimsPrincipal user = null)
        {
            var place = Get(placeId);
            if (!place.HasValue)
            {
                return Maybe.Empty<Indicator>();
            }
            return place.Value.Indicators.ContainsKey(type) ? place.Value.Indicators[type].ToMaybe() : Maybe.Empty<Indicator>();
        }

        public IDictionary<string, IDictionary<string, Indicator>> GetIndicatorsByGroup(string group, ClaimsPrincipal user = null)
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
}
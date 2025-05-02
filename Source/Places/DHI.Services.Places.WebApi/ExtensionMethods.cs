namespace DHI.Services.Places.WebApi
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static IEnumerable<PlaceDTO> ToDTOs(this IEnumerable<Place> places)
        {
            return places.Select(place => new PlaceDTO(place));
        }
        public static IEnumerable<PlaceDTO<string>> ToDTOs(this IEnumerable<Place<string>> places)
        {
            return places.Select(place => new PlaceDTO<string>(place));
        }

        public static IDictionary<string, IndicatorDTO> ToDTOs(this IDictionary<string, Indicator> indicators)
        {
            return indicators.ToDictionary(kvp => kvp.Key, kvp => new IndicatorDTO(kvp.Value));
        }

        public static PlaceDTO ToDTO(this Place place)
        {
            return new PlaceDTO(place);
        }
        public static PlaceDTO<string> ToDTO(this Place<string> place)
        {
            return new PlaceDTO<string>(place);
        }

        public static IndicatorDTO ToDTO(this Indicator indicator)
        {
            return new IndicatorDTO(indicator);
        }
    }
}
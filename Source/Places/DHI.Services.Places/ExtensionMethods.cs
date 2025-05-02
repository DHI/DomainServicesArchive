namespace DHI.Services.Places
{
    using System.Collections.Generic;
    using SkiaSharp;
    using Spatial;

    public static class ExtensionMethods
    {
        public static FeatureCollection AddIndicatorsWithStatus(this FeatureCollection featureCollection, IDictionary<string, IDictionary<string, Indicator>> indicators, IDictionary<Indicator, Maybe<SKColor>> statusList)
        {
            foreach (var feature in featureCollection.Features)
            {
                var indicatorDictionary = new Dictionary<string, Dictionary<string, object>>();
                if (indicators.ContainsKey(feature.AttributeValues["placeId"].ToString()))
                {
                    foreach (var indicator in indicators[feature.AttributeValues["placeId"].ToString()])
                    {
                        indicatorDictionary.Add(indicator.Key, new Dictionary<string, object>());
                        indicatorDictionary[indicator.Key].Add("styleCode", indicator.Value.StyleCode);
                        indicatorDictionary[indicator.Key].Add("color", statusList[indicator.Value].Value);
                    }
                }

                feature.AttributeValues["indicators"] = indicatorDictionary;
            }

            return featureCollection;
        }
    }
}
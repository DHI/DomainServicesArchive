namespace DHI.Services.Places.WebApi.Test
{
    using System.Collections.Generic;
    using System.Text.Json;
    using GIS.Maps;
    using System.Text;
    using TimeSeries;
    using Xunit;
    using System.Net.Http;

    public class PlaceDTOTest
    {
        [Fact]
        public void DtoToPlaceIsOk()
        {
            var fullName = new FullName("Stations", "MyStation");
            var placeDTO = new PlaceDTO
            {
                FullName = fullName.ToString(),
                FeatureId = new FeatureId("Stationer.shp", "StatId", "ID92_M16"),
                Indicators = new Dictionary<string, IndicatorDTO>
                {
                    { "WaterLevel",
                        new IndicatorDTO(new Indicator(
                            new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries1"),
                            "0:green|10:red",
                            TimeInterval.CreateAll(),
                            AggregationType.Maximum))
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    { "PointCategory", "Station" },
                    { "Letter", "S" },
                    { "Color", "#FFD700" }
                }
            };

            var json = JsonSerializer.Serialize(placeDTO);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var place = placeDTO.ToPlace();
            Assert.Equal(fullName.ToString(), place.FullName);
            Assert.Equal(fullName.Group, place.Group);
            Assert.Equal(fullName.Name, place.Name);
            Assert.Equal(placeDTO.FeatureId, place.FeatureId);
            Assert.Contains("WaterLevel", place.Indicators.Keys);
            Assert.Equal(AggregationType.Maximum, place.Indicators["WaterLevel"].AggregationType);
            Assert.Equal(PaletteType.LowerThresholdValues, place.Indicators["WaterLevel"].PaletteType);
            Assert.Equal("Station", place.Metadata["PointCategory"].ToString());
        }
    }
}

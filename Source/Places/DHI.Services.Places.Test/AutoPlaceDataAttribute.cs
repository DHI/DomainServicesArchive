namespace DHI.Services.Places.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using GIS;
    using GIS.Maps;
    using Provider.ShapeFile;
    using TimeSeries;
    using TimeSeries.CSV;

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class AutoPlaceDataAttribute : AutoDataAttribute
    {
        public AutoPlaceDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<Place>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                fixture.Register(() => new FeatureId("Stationer.shp", "StatId", "ID92_M16"));


                var timeSeriesService = new DiscreteTimeSeriesService<string, double>(new TimeSeriesRepository(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data")));
                var timeSeriesServiceDictionary = new Dictionary<string, IDiscreteTimeSeriesService<string, double>> { { "tsConnection", timeSeriesService } };
                var gisService = new GisService<string>(new FeatureRepository(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data\shp")));
                var dataSource1 = new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries1");
                var waterLevelIndicator = new Indicator(dataSource1, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
                var dataSource2 = new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries2");
                var dischargeIndicator = new Indicator(dataSource2, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
                var lowFlowIndicator = new Indicator(dataSource2, "20:Red|50:Green", TimeInterval.CreateAll(), AggregationType.Minimum, null, PaletteType.UpperThresholdValues);
                var myPlace = new Place("myStation", "MyStation", new FeatureId("Stationer.shp", "StatId", "ID92_M16"), "Stations");
                myPlace.Indicators.Add("WaterLevel", waterLevelIndicator);
                myPlace.Indicators.Add("Discharge", dischargeIndicator);
                myPlace.Indicators.Add("LowFlow", lowFlowIndicator);

                //IPlaceRepository<string> placeRepository = new FakePlaceRepository(new List<Place> { myPlace });
                var placeRepositoryPath = Path.Combine(Path.GetTempPath(), $"places-{DateTime.Now.Ticks}.json");
                var placeRepository = new PlaceRepository(placeRepositoryPath);

                var placeService = new PlaceService(placeRepository, timeSeriesServiceDictionary, null, gisService);
                placeService.Add(myPlace);
                fixture.Register(() => placeService);
                fixture.Register(() => placeRepository);

                return fixture;
            })
        {
        }
    }
}
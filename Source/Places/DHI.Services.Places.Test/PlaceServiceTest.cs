namespace DHI.Services.Places.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using GIS;
    using Provider.ShapeFile;
    using SkiaSharp;
    using TimeSeries;
    using Xunit;
    using Point = Spatial.Point;

    public class PlaceServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PlaceService(null, null, null, new GisService<string>(new FeatureRepository(""))));
            Assert.Equal("repository", exception.ParamName);
        }

        [Fact]
        public void CreateWithNullGisServiceThrows()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PlaceService(new FakePlaceRepository(new List<Place<string>>()), null, null, null));
            Assert.Equal("gisService", exception.ParamName);
        }

        [Theory, AutoPlaceData]
        public void GetNonExistingThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.Get("NonExistingPlace"));
        }


        [Theory, AutoPlaceData]
        public void UpdateNonExistingThrows(PlaceService placeService, Place place)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.Update(place));
        }

        [Theory, AutoPlaceData]
        public void RemoveNonExistingThrows(PlaceService placeService, Place place)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.Remove(place.Id));
        }

        [Theory, AutoPlaceData]
        public void GetByGroupForNonExistingThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoPlaceData]
        public void GetByGroupForNullGroupThrows(PlaceService placeService)
        {
            Assert.Throws<ArgumentNullException>(() => placeService.GetByGroup(null));
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesForNonExistingGroupThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesForNullOrEmptyGroupThrows(PlaceService placeService)
        {
            Assert.Throws<ArgumentNullException>(() => placeService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => placeService.GetFullNames(""));
        }

        [Theory, AutoPlaceData]
        public void AddExistingThrows(PlaceService placeService, Place place)
        {
            placeService.Add(place);
            Assert.Throws<ArgumentException>(() => placeService.Add(place));
        }

        [Theory, AutoPlaceData]
        public void AddWithExistingIdThrows(PlaceService placeService, Place place)
        {
            placeService.Add(place);
            var newPlace = new Place(place.Id, "NewName", place.FeatureId, place.Group);
            Assert.Throws<ArgumentException>(() => placeService.Add(newPlace));
        }

        [Theory, AutoPlaceData]
        public void AddWithNonExistingFeatureCollectionThrows(PlaceService placeService)
        {
            var myPlace = new Place("myStation", "MyStation", new FeatureId("NonExist.shp", "StatId", "ID92_M16"), "Stations");
            Assert.Throws<ArgumentException>(() => placeService.Add(myPlace));
        }

        [Theory, AutoPlaceData]
        public void AddWithNonExistingFeatureThrows(PlaceService placeService)
        {
            var myPlace = new Place("myStation", "MyStation", new FeatureId("Stationer.shp", "StatId", "non-exist"), "Stations");
            Assert.Throws<ArgumentException>(() => placeService.Add(myPlace));
        }

        [Theory, AutoPlaceData]
        public void AddExistingIndicatorThrows(PlaceService placeService)
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries1");
            var indicator = new Indicator(dataSource, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
            Assert.Throws<ArgumentException>(() => placeService.AddIndicator("myStation", "WaterLevel", indicator));
        }

        [Theory, AutoPlaceData]
        public void AddIndicatorToNonExistingPlaceThrows(PlaceService placeService)
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries1");
            var indicator = new Indicator(dataSource, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
            Assert.Throws<KeyNotFoundException>(() => placeService.AddIndicator("non-exist", "WaterLevel", indicator));
        }

        [Theory, AutoPlaceData]
        public void RemoveNonExistingIndicatorThrows(PlaceService placeService)
        {
            Assert.Throws<ArgumentException>(() => placeService.RemoveIndicator("myStation", "non-exist"));
        }

        [Theory, AutoPlaceData]
        public void RemoveIndicatorFromNonExistingPlaceThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.RemoveIndicator("non-exist", "Discharge"));
        }

        [Theory, AutoPlaceData]
        public void UpdateNonExistingIndicatorThrows(PlaceService placeService)
        {
            Assert.Throws<ArgumentException>(() => placeService.UpdateIndicator("myStation", "non-exist", null));
        }

        [Theory, AutoPlaceData]
        public void UpdateIndicatorAtNonExistingPlaceThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.UpdateIndicator("non-exist", "Discharge", null));
        }

        [Theory, AutoPlaceData]
        public void AddScalarIndicatorWhenNoScalarServicesThrows(PlaceService placeService)
        {
            var dataSource = new DataSource(DataSourceType.Scalar, "connectionId", "scalarId");
            var indicator = new Indicator(dataSource, "0:Red|10:Green");
            Assert.Throws<ArgumentException>(() => placeService.AddIndicator("myStation", "Scalar", indicator));
        }

        [Theory, AutoPlaceData]
        public void AddScalarIndicatorForNonExistingConnectionThrows(Place place, PlaceRepository placeRepository)
        {
            placeRepository.Add(place);
            var gisService = new GisService<string>(new FeatureRepository(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data\shp")));
            var placeService = new PlaceService(placeRepository, null, new Dictionary<string, Scalars.IScalarService<string, int>>(), gisService);
            var dataSource = new DataSource(DataSourceType.Scalar, "connectionId", "scalarId");
            var indicator = new Indicator(dataSource, "0:Red|10:Green");
            Assert.Throws<ArgumentException>(() => placeService.AddIndicator(place.Id, "Scalar", indicator));
        }

        [Theory, AutoPlaceData]
        public void AddTimeSeriesIndicatorWhenNoTimeSeriesServicesThrows(Place place, PlaceRepository placeRepository)
        {
            placeRepository.Add(place);
            var gisService = new GisService<string>(new FeatureRepository(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data\shp")));
            var placeService = new PlaceService(placeRepository, null, new Dictionary<string, Scalars.IScalarService<string, int>>(), gisService);
            var dataSource = new DataSource(DataSourceType.TimeSeries, "tsConnection", "timeseries.csv;TimeSeries2");
            var indicator = new Indicator(dataSource, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
            Assert.Throws<ArgumentException>(() => placeService.AddIndicator(place.Id, "WaterLevel", indicator));
        }

        [Theory, AutoPlaceData]
        public void AddTimeSeriesIndicatorForNonExistingConnectionThrows(PlaceService placeService)
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "non-exist", "timeseries.csv;TimeSeries2");
            var indicator = new Indicator(dataSource, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);
            Assert.Throws<ArgumentException>(() => placeService.AddIndicator("myStation", "Waterlevel", indicator));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByNonExistingPlaceReturnsEmptyDictionary(PlaceService placeService)
        {
            var indicators = placeService.GetIndicatorsByPlace("non-exist");
            Assert.Empty(indicators);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByNonExistingGroupThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetIndicatorsByGroup("non-exist"));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorForNonExistingPlaceThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetIndicator("non-exist", "Discharge"));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorOfNonExistingIndicatorTypeThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetIndicator("myStation", "non-exist"));
        }

        [Theory, AutoPlaceData]
        public void GetThresholdsForNonExistingPlaceThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetThresholdValues("non-exists"));
        }

        [Theory, AutoPlaceData]
        public void GetThresholdsOfNonExistingIndicatorTypeThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetThresholdValues("myStation", "non-exist"));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusForNonExistingPlaceThrows(PlaceService placeService)
        {
            Assert.Throws<KeyNotFoundException>(() => placeService.GetIndicatorStatus("non-exists", "Discharge"));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusOfNonExistingIndicatorTypeThrows(PlaceService placeService)
        {
            Assert.Throws<ArgumentException>(() => placeService.GetIndicatorStatusByType("non-existing"));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusListWithoutTimeIntervalThrows(PlaceService placeService)
        {
            var indicators = placeService.GetIndicatorsByPlace("myStation");

            var indicatorList = new List<Indicator>();
            foreach (var keyValuePair in indicators)
            {
                var indicator = keyValuePair.Value;
                indicatorList.Add(new Indicator(indicator.DataSource, indicator.StyleCode, null, indicator.AggregationType));
            }

            var e = Assert.Throws<ArgumentException>(() => placeService.GetIndicatorStatus(indicatorList));
            Assert.Contains("There is no declarative TimeInterval defined on one of the given indicators", e.Message);
        }

        [Theory, AutoPlaceData]
        public void GetByGroupIsOk(PlaceService placeService)
        {
            var group = placeService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(placeService.GetByGroup(group).Any());
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesByGroupIsOk(PlaceService placeService)
        {
            var group = placeService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = placeService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesIsOk(PlaceService placeService)
        {
            Assert.Single(placeService.GetFullNames());
        }

        [Theory, AutoPlaceData]
        public void GetAllIsOk(PlaceService placeService)
        {
            Assert.Single(placeService.GetAll());
        }

        [Theory, AutoPlaceData]
        public void GetIdsIsOk(PlaceService placeService)
        {
            Assert.Single(placeService.GetIds());
        }

        [Theory, AutoPlaceData]
        public void GetFeaturesIsOk(PlaceService placeService)
        {
            var collection = placeService.GetFeatures();
            Assert.Single(collection.Features);
            var feature = collection.Features.First();
            Assert.Equal(typeof(Point), feature.Geometry.GetType());
            Assert.Equal("Stations/MyStation", feature.AttributeValues["fullName"]);
            Assert.Equal("MyStation", feature.AttributeValues["name"]);
            Assert.Equal("Stationer.shp", feature.AttributeValues["groupLayer"]);
            Assert.DoesNotContain("indicators", feature.AttributeValues.Keys);
        }

        [Theory, AutoPlaceData]
        public void GetFeaturesWithIndicatorStatusIsOk(PlaceService placeService)
        {
            // Arrange
            var indicatorExpected = new Dictionary<string, object>
            {
                { "styleCode", "0:Green|10:Red" },
                { "color", SKColors.Red }
            };

            // Act
            var collection = placeService.GetFeaturesWithIndicatorStatus();
            Assert.Single(collection.Features);
            var feature = collection.Features.First();

            // Assert
            Assert.Contains("indicators", feature.AttributeValues.Keys);
            var indicators = Assert.IsAssignableFrom<Dictionary<string, Dictionary<string, object>>>(feature.AttributeValues["indicators"]);
            Assert.Equal(3, indicators.Count);
            Assert.Equal(indicatorExpected, indicators.ElementAt(0).Value);
        }

        [Theory, AutoPlaceData]
        public void GetFeaturesWithIndicatorStatusForPeriodIsOk(PlaceService placeService)
        {
            // Arrange
            var indicatorExpected = new Dictionary<string, object>
            {
                { "styleCode", "0:Green|10:Red" },
                { "color", SKColors.Green }
            };

            // Act
            var collection = placeService.GetFeaturesWithIndicatorStatus(new DateTime(2015, 11, 1), new DateTime(2015, 11, 15));
            Assert.Single(collection.Features);
            var feature = collection.Features.First();

            // Assert
            Assert.Contains("indicators", feature.AttributeValues.Keys);
            var indicators = Assert.IsAssignableFrom<Dictionary<string, Dictionary<string, object>>>(feature.AttributeValues["indicators"]);
            Assert.Equal(3, indicators.Count);
            Assert.Equal(indicatorExpected, indicators.ElementAt(0).Value);
        }

        [Theory, AutoPlaceData]
        public void AddAndGetIsOk(PlaceService placeService, Place place)
        {
            placeService.Add(place);
            Assert.Equal(place.Id, placeService.Get(place.FullName).Id);
        }

        [Theory, AutoPlaceData]
        public void CountIsOk(PlaceService placeService)
        {
            Assert.Equal(1, placeService.Count());
        }

        [Theory, AutoPlaceData]
        public void ExistsIsOk(PlaceService placeService)
        {
            var place = placeService.GetAll().ToArray()[0];
            Assert.True(placeService.Exists(place.FullName));
        }

        [Theory, AutoPlaceData]
        public void DoesNotExistIsOk(PlaceService placeService)
        {
            Assert.False(placeService.Exists("NonExisting"));
        }

        [Theory, AutoPlaceData]
        public void EventsAreRaisedOnAdd(PlaceService placeService, Place place)
        {
            var raisedEvents = new List<string>();
            placeService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            placeService.Added += (s, e) => { raisedEvents.Add("Added"); };

            placeService.Add(place);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoPlaceData]
        public void RemoveIsOk(PlaceService placeService, Place place)
        {
            placeService.Add(place);
            placeService.Remove(place.Id);

            Assert.False(placeService.Exists(place.Id));
        }

        [Theory, AutoPlaceData]
        public void EventsAreRaisedOnRemove(PlaceService placeService, Place place)
        {
            var raisedEvents = new List<string>();
            placeService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            placeService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            placeService.Add(place);

            placeService.Remove(place.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoPlaceData]
        public void UpdateIsOk(PlaceService placeService, Place place, FeatureId featureId)
        {
            placeService.Add(place);
            var updatedPlace = new Place(place.Id, $"Update{Guid.NewGuid()}", featureId, place.Group);
            placeService.Update(updatedPlace);

            Assert.Equal(updatedPlace.Name, placeService.Get(place.Id).Name);
        }

        [Theory, AutoPlaceData]
        public void AddOrUpdateIsOk(PlaceService placeService, Place place, FeatureId featureId)
        {
            var raisedEvents = new List<string>();
            placeService.Added += (s, e) => { raisedEvents.Add("Added"); };
            placeService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            placeService.AddOrUpdate(place);
            var updated = new Place(place.Id, "NewName", featureId, place.Group);
            placeService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Name, placeService.Get(place.Id).Name);
        }

        [Theory, AutoPlaceData]
        public void TryAddIsOk(PlaceService placeService, Place place)
        {
            Assert.True(placeService.TryAdd(place));
            Assert.Equal(place.Id, placeService.Get(place.FullName).Id);
        }

        [Theory, AutoPlaceData]
        public void TryUpdateIsOk(PlaceService placeService, Place place, FeatureId featureId)
        {
            placeService.Add(place);
            var updatedPlace = new Place(place.Id, "NewName", featureId, place.Group);

            Assert.True(placeService.TryUpdate(updatedPlace));
            Assert.Equal(updatedPlace.Name, placeService.Get(place.Id).Name);
        }

        [Theory, AutoPlaceData]
        public void TryUpdateNonExistingReturnsFalse(PlaceService placeService, Place place)
        {
            Assert.False(placeService.TryUpdate(place));
        }

        [Theory, AutoPlaceData]
        public void EventsAreRaisedOnUpdate(PlaceService placeService, Place place, FeatureId featureId)
        {
            var raisedEvents = new List<string>();
            placeService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            placeService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            placeService.Add(place);

            var updatedPlace = new Place(place.Id, "UpdatedName", featureId, place.Group);
            placeService.Update(updatedPlace);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusIsOk(PlaceService placeService)
        {
            Assert.Equal(SKColors.Red, placeService.GetIndicatorStatus("myStation", "WaterLevel").Value);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusUpperThresholdValuesIsOk(PlaceService placeService)
        {
            Assert.Equal(SKColors.Red, placeService.GetIndicatorStatus("myStation", "LowFlow").Value);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusByGroupIsOk(PlaceService placeService)
        {
            var colorList = placeService.GetIndicatorStatusByGroup("Stations");
            Assert.Equal(SKColors.Red, colorList.Values.ElementAt(0).Values.ElementAt(0));
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusListIsOk(PlaceService placeService)
        {
            var indicators = placeService.GetIndicatorsByPlace("myStation");
            var colorList = placeService.GetIndicatorStatus(indicators.Values.ToList());
            Assert.Equal(SKColors.Red, colorList.Values.ElementAt(0).Value);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorStatusListFromPeriodIsOk(PlaceService placeService)
        {
            var indicators = placeService.GetIndicatorsByPlace("myStation");
            var colorList = placeService.GetIndicatorStatus(indicators.Values.ToList(), new DateTime(2015, 11, 1), new DateTime(2015, 11, 15));
            Assert.Equal(SKColors.Green, colorList.Values.ElementAt(0).Value);
        }

        [Fact]
        public void ResolveTimeSeriesIdIsOk()
        {
            Assert.Equal("foo/bar/id", PlaceService.ResolveTimeSeriesId("[Path]/id", "foo/bar"));
            Assert.Equal("foo/bar/id", PlaceService.ResolveTimeSeriesId("foo/bar/id", null));
            Assert.Equal("foo/bar/id", PlaceService.ResolveTimeSeriesId("foo/bar/id", ""));
            Assert.Equal(999, PlaceService.ResolveTimeSeriesId(999, null));
            var guid = Guid.NewGuid();
            Assert.Equal(guid, PlaceService.ResolveTimeSeriesId(guid, null));
            Assert.Throws<ArgumentNullException>(() => PlaceService.ResolveTimeSeriesId("[Path]/id", null));
            Assert.Throws<ArgumentException>(() => PlaceService.ResolveTimeSeriesId("[Path]/id", ""));
        }

        [Theory, AutoPlaceData]
        public void CalculateQuantileIsOk(PlaceService placeService)
        {
            // Arrange
            var valueList = new List<double?> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var q50 = 0.5;
            var q75 = 0.75;
            var q25 = 0.25;

            // Act
            var actualQ50 = placeService.CalculateQuantile(valueList, q50);
            var actualQ75 = placeService.CalculateQuantile(valueList, q75);
            var actualQ25 = placeService.CalculateQuantile(valueList, q25);

            // Assert
            Assert.Equal(5.0, actualQ50);
            Assert.Equal(7.0, actualQ75);
            Assert.Equal(3.0, actualQ25);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorValueIsOk(PlaceService placeService, IList<double?> aggregatedValues)
        {
            // Arrange
            var waterLevelIndicator = placeService.GetIndicator("myStation", "WaterLevel");
            var indicator = new Indicator(waterLevelIndicator.DataSource, waterLevelIndicator.StyleCode, waterLevelIndicator.TimeInterval, waterLevelIndicator.AggregationType, 0.75);
            var maxExpected = aggregatedValues.Max();
            var q75Expected = placeService.CalculateQuantile(aggregatedValues.ToList(), indicator.Quantile.Value);


            // Act
            var maxActual = placeService.GetIndicatorValue(waterLevelIndicator, aggregatedValues);
            var q75Actual = placeService.GetIndicatorValue(indicator, aggregatedValues);

            // Assert
            Assert.Equal(maxExpected, maxActual);
            Assert.Equal(q75Expected, q75Actual);
        }

        [Fact]
        public void GetIndicatorStatusIsOk_TO_BE_DELETED()
        {
            var waterLevelData = new TimeSeriesData<double>(
                new List<DateTime>
                {
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 2),
                    new DateTime(2020, 1, 3)
                },
                new List<double?>
                {
                    5.2, null, 5.4
                });

            var waterLevelMyPlace = new TimeSeries<string, double>("waterLevelMyPlace", "Water Level", null, waterLevelData);
            var timeSeriesRepository = new InMemoryTimeSeriesRepository<string, double>(new List<TimeSeries<string, double>>
            {
                waterLevelMyPlace
            });

            var timeSeriesService = new DiscreteTimeSeriesService<string, double>(timeSeriesRepository);
            var timeSeriesServices = new Dictionary<string, IDiscreteTimeSeriesService<string, double>> { { "myConnection", timeSeriesService } };

            var gisService = new GisService<string>(new FeatureRepository(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data\shp")));

            var dataSource = new DataSource(DataSourceType.TimeSeries, "myConnection", "waterLevelMyPlace");
            var waterLevelIndicator = new Indicator(dataSource, "0:Green|10:Red", TimeInterval.CreateAll(), AggregationType.Maximum);

            var myPlace = new Place("myPlace", "My Place", new FeatureId("Stationer.shp", "StatId", "ID92_M16"));
            myPlace.Indicators.Add("WaterLevel", waterLevelIndicator);
            var placeRepository = new FakePlaceRepository(new List<Place> { myPlace });
            var placeService = new PlaceService(placeRepository, timeSeriesServices, null, gisService);

            Assert.Equal(SKColors.Green, placeService.GetIndicatorStatus(myPlace.Id, "WaterLevel").Value);

            var anotherPlace = new Place("anotherPlace", "Another Place", new FeatureId("Stationer.shp", "StatId", "ID92_M16"));
            placeService.Add(anotherPlace);
        }
    }
}
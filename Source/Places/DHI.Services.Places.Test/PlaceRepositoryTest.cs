namespace DHI.Services.Places.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    public sealed class PlaceRepositoryTest : IDisposable
    {
        private readonly PlaceRepository _repository;
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"places-{DateTime.Now.Ticks}.json");

        public PlaceRepositoryTest()
        {
            File.Delete(_filePath); //cleanup previous json file (in case previous test failed)
            _repository = new PlaceRepository(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new PlaceRepository(null));
        }

        [Fact]
        public void AddWithNoGroupThrows()
        {
            var place = new Place("myPlace", "My Place", new FeatureId("collectionId", "key", "value"));
            var exception = Assert.Throws<ArgumentException>(() => _repository.Add(place));
            Assert.Contains("does not belong to a group.", exception.Message);
        }

        [Fact]
        public void ContainsWithInvalidGroupedIdReturnFalse()
        {
            var isExist = _repository.Contains("InvalidPlaceId");
            Assert.False(isExist);
        }

        [Fact]
        public void GetWithInvalidGroupedIdThrows()
        {
            var exception = Assert.Throws<ArgumentException>(() => _repository.Get("InvalidPlaceId"));
            Assert.Contains("The ID of a grouped entity must be a string with following format", exception.Message);
        }

        [Fact]
        public void RemoveWithInvalidGroupedIdThrows()
        {
            var exception = Assert.Throws<ArgumentException>(() => _repository.Remove("InvalidPlaceId"));
            Assert.Contains("The ID of a grouped entity must be a string with following format", exception.Message);
        }

        [Fact]
        public void GetIndicatorsByNonExistingGroupThrows()
        {
            var exception = Assert.Throws<KeyNotFoundException>(() => _repository.GetIndicatorsByGroupAndType("NonExistingGroup", "WaterLevel"));
            Assert.Contains("not present in the dictionary.", exception.Message);
        }

        [Fact]
        public void GetNonExistingReturnsEmpty()
        {
            Assert.False(_repository.Get("NonExistingGroup/NonExistingName").HasValue);
        }

        [Fact]
        public void CanSerializeFilePlacesJson()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Data\places.json");
            var repos = new PlaceRepository<string>(path);

            //var g = _repository.GetByGroup("Stations");

            var p = repos.GetAll();
        }

        [Theory, AutoPlaceData]
        public void GetNonExistingFromExistingGroupReturnsEmpty(Place place)
        {
            _repository.Add(place);
            var id = $"{place.Group}/NonExistingName";
            Assert.False(_repository.Get(id).HasValue);
        }

        [Theory, AutoPlaceData]
        public void AddAndGetIsOk(Place place)
        {
            _repository.Add(place);
            var actual = _repository.Get(place.FullName);
            Assert.Equal(place.Id, actual.Value.Id);
        }

        [Theory, AutoPlaceData]
        public void ContainsIsOk(Place place)
        {
            _repository.Add(place);
            Assert.True(_repository.Contains(place.FullName));
        }

        [Theory, AutoPlaceData]
        public void DoesNotContainIsOk(Place place)
        {
            Assert.False(_repository.Contains(place.FullName));
        }

        [Theory, AutoPlaceData]
        public void ContainsGroupIsOk(Place place)
        {
            _repository.Add(place);
            Assert.True(_repository.ContainsGroup(place.Group));
        }

        [Theory, AutoPlaceData]
        public void DoesNotContainGroupIsOk(Place place)
        {
            Assert.False(_repository.ContainsGroup(place.Group));
        }

        [Theory, AutoPlaceData]
        public void CountIsOk(Place place)
        {
            _repository.Add(place);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoPlaceData]
        public void GetAllIsOk(Place place)
        {
            _repository.Add(place);
            Assert.Single(_repository.GetAll());
        }


        [Theory, AutoPlaceData]
        public void GetByGroupIsOk(Place place1, Place place2)
        {
            _repository.Add(place1);
            _repository.Add(place2);
            var place3 = new Place(Guid.NewGuid().ToString(), "MyPlace", new FeatureId(Guid.NewGuid().ToString(), "key", "value"), place1.Group);
            _repository.Add(place3);
            Assert.Equal(2, _repository.GetByGroup(place1.Group).Count());
            Assert.Single(_repository.GetByGroup(place2.Group));
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesByGroupIsOk(Place place1, Place place2)
        {
            _repository.Add(place1);
            _repository.Add(place2);
            var place3 = new Place(Guid.NewGuid().ToString(), "MyPlace", new FeatureId(Guid.NewGuid().ToString(), "key", "value"), place1.Group);
            _repository.Add(place3);
            Assert.Equal(2, _repository.GetFullNames(place1.Group).Count());
            Assert.Single(_repository.GetFullNames(place2.Group));
            Assert.Equal(place2.FullName, _repository.GetFullNames(place2.Group).First());
        }

        [Theory, AutoPlaceData]
        public void GetFullNamesIsOk(Place place1, Place place2)
        {
            _repository.Add(place1);
            _repository.Add(place2);
            var place3 = new Place(Guid.NewGuid().ToString(), "MyPlace", new FeatureId(Guid.NewGuid().ToString(), "key", "value"), place1.Group);
            _repository.Add(place3);
            Assert.Equal(3, _repository.GetFullNames().Count());
        }

        [Theory, AutoPlaceData]
        public void GetIdsIsOk(Place place)
        {
            _repository.Add(place);
            Assert.Equal(place.Id, _repository.GetIds().First());
        }

        [Theory, AutoPlaceData]
        public void RemoveIsOk(Place place)
        {
            _repository.Add(place);
            _repository.Remove(place.FullName);
            Assert.False(_repository.Contains(place.FullName));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoPlaceData]
        public void RemoveUsingPredicateIsOk(Place place1, Place place2)
        {
            _repository.Add(place1);
            _repository.Add(place2);
            _repository.Remove(e => e.Id == place1.Id);
            Assert.False(_repository.Contains(place1.FullName));
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoPlaceData]
        public void UpdateIsOk(Place place)
        {
            _repository.Add(place);
            place.Metadata.Add("Description", "A description");
            _repository.Update(place);
            Assert.Equal(place.Metadata["Description"], _repository.Get(place.FullName).Value.Metadata["Description"]);
        }

        [Theory, AutoPlaceData]
        public void GetAllReturnsClones(Place place)
        {
            _repository.Add(place);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(place.FullName).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {
            Assert.Empty(_repository.GetAll());
        }

        [Theory, AutoPlaceData]
        public void GetReturnsClone(Place place)
        {
            _repository.Add(place);
            var e = _repository.Get(place.FullName).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(place.FullName).Value.Metadata);
        }

        [Theory, AutoPlaceData]
        public void GetByPredicateReturnsClones(Place place)
        {
            _repository.Add(place);
            var e = _repository.Get(ent => ent.Id == place.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(place.FullName).Value.Metadata);
        }

        [Theory, AutoPlaceData]
        public void GetByGroupReturnsClones(Place place)
        {
            _repository.Add(place);
            var e = _repository.GetByGroup(place.Group).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(place.FullName).Value.Metadata);
        }

        [Theory, AutoPlaceData]
        public void GetNonExistingIndicatorReturnsEmptyMaybe(Place place)
        {
            _repository.Add(place);
            var maybe = _repository.GetIndicator(place.FullName, "NonExistingIndicator");
            Assert.False(maybe.HasValue);
        }

        [Fact]
        public void GetIndicatorForNonExistingPlaceReturnsEmptyMaybe()
        {
            var maybe = _repository.GetIndicator("MyGroup/NonExistingPlace", "WaterLevel");
            Assert.False(maybe.HasValue);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorIsOk(Place place)
        {
            var dataSource = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "MyScalar");
            place.Indicators.Add("WaterLevel", new Indicator(dataSource, "0:red|5:green"));
            _repository.Add(place);
            var maybe = _repository.GetIndicator(place.FullName, "WaterLevel");

            Assert.True(maybe.HasValue);
            Assert.Equal(dataSource, maybe.Value.DataSource);
        }

        [Fact]
        public void GetIndicatorsByNonExistingPlaceReturnsEmptyDictionary()
        {
            var indicators = _repository.GetIndicatorsByPlace("MyGroup/NonExistingPlace");
            Assert.Empty(indicators);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByPlaceIsOk(Place place)
        {
            var dataSourceWL = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel");
            var dataSourceQ = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "Discharge");
            place.Indicators.Add("WaterLevel", new Indicator(dataSourceWL, "0:red|5:green"));
            place.Indicators.Add("Discharge", new Indicator(dataSourceQ, "10:red|15:green"));
            _repository.Add(place);
            var indicators = _repository.GetIndicatorsByPlace(place.FullName);

            Assert.Equal(2, indicators.Count);
            Assert.Contains("WaterLevel", indicators);
            Assert.Equal(dataSourceWL, indicators["WaterLevel"].DataSource);
        }

        [Fact]
        public void GetIndicatorsByNonExistingTypeReturnsEmptyDictionary()
        {
            var indicators = _repository.GetIndicatorsByType("NonExistingType");
            Assert.Empty(indicators);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByTypeIsOk(Place place1, Place place2)
        {
            var dataSource1 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel1");
            var dataSource2 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel2");
            place1.Indicators.Add("WaterLevel", new Indicator(dataSource1, "0:red|5:green"));
            place2.Indicators.Add("WaterLevel", new Indicator(dataSource2, "0:red|5:green"));
            _repository.Add(place1);
            _repository.Add(place2);
            var indicators = _repository.GetIndicatorsByType("WaterLevel");

            Assert.Equal(2, indicators.Count);
            Assert.Contains(place1.Id, indicators);
            Assert.Equal(dataSource1, indicators[place1.Id].DataSource);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByGroupAndNonExistingTypeReturnsEmptyDictionary(Place place)
        {
            var dataSource = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "MyScalar");
            place.Indicators.Add("WaterLevel", new Indicator(dataSource, "0:red|5:green"));
            _repository.Add(place);
            var indicators = _repository.GetIndicatorsByGroupAndType(place.Group, "NonExistingType");

            Assert.Empty(indicators);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsByGroupAndTypeIsOk(Place place1, Place place2)
        {
            var dataSource1 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel1");
            var dataSource2 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel2");
            place1.Indicators.Add("WaterLevel", new Indicator(dataSource1, "0:red|5:green"));
            place2.Indicators.Add("WaterLevel", new Indicator(dataSource2, "0:red|5:green"));
            _repository.Add(place1);
            _repository.Add(place2);
            var indicators = _repository.GetIndicatorsByGroupAndType(place1.Group, "WaterLevel");

            Assert.Equal(1, indicators.Count);
            Assert.Contains(place1.Id, indicators);
            Assert.Equal(dataSource1, indicators[place1.Id].DataSource);
        }

        [Theory, AutoPlaceData]
        public void GetIndicatorsIsOk(Place place1, Place place2)
        {
            var dataSource1 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel1");
            var dataSource2 = new DataSource(DataSourceType.Scalar, "ScalarConnectionId", "WaterLevel2");
            place1.Indicators.Add("WaterLevel", new Indicator(dataSource1, "0:red|5:green"));
            place2.Indicators.Add("WaterLevel", new Indicator(dataSource2, "0:red|5:green"));
            _repository.Add(place1);
            _repository.Add(place2);
            var indicators = _repository.GetIndicators();

            Assert.Equal(2, indicators.Count);
            Assert.Contains(place1.Id, indicators);
            Assert.Equal(dataSource1, indicators[place1.Id].ElementAt(0).Value.DataSource);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}

// ReSharper disable UnusedMember.Global
namespace DHI.Services.Meshes.Test
{
    using DHI.Services.TimeSeries;
    using DHI.Spatial;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Xunit;

    public class GroupedMeshServiceTest
    {
        private const int _repeatCount = 3;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new GroupedMeshService<Guid>(null!));
            Assert.Equal("repository", exception.ParamName);
        }

        [Theory, AutoMeshInfoData]
        public void GetNonExistingThrows(GroupedMeshService<Guid> meshService, Guid id)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.Get(id));
        }

        [Theory, AutoMeshInfoData]
        public void GetDateTimesForNonExistingThrows(MeshService<Guid> meshService, Guid id)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetDateTimes(id));
        }

        [Theory, AutoMeshInfoData]
        public void GetValuesForNonExistingThrows(MeshService<Guid> meshService, Guid id, string item, Point point, DateRange dateRange)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetValues(id, item, point, dateRange));
            Assert.Throws<KeyNotFoundException>(() => meshService.GetValues(id, point, dateRange));
        }

        [Theory, AutoMeshInfoData]
        public void GetAggregatedValuesForNonExistingThrows(MeshService<Guid> meshService, Guid id, AggregationType aggregationType, string item, Polygon polygon, DateRange dateRange, Period period)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValues(id, aggregationType, item, dateRange));
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValues(id, aggregationType, item, polygon, dateRange));
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValues(id, aggregationType, item, period, dateRange));
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValues(id, aggregationType, item, polygon, period, dateRange));
        }

        [Theory, AutoMeshInfoData]
        public void GetAggregatedValueForNonExistingThrows(MeshService<Guid> meshService, Guid id, AggregationType aggregationType, DateTime dateTime, string item, Polygon polygon)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValue(id, aggregationType, item, dateTime));
            Assert.Throws<KeyNotFoundException>(() => meshService.GetAggregatedValue(id, aggregationType, item, polygon, dateTime));
        }

        [Theory, AutoMeshInfoData]
        public void GetByGroupForNonExistingThrows(GroupedMeshService<Guid> meshService, string group)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetByGroup(group));
        }

        [Theory, AutoMeshInfoData]
        public void GetByGroupForNullGroupThrows(GroupedMeshService<Guid> meshService)
        {
            Assert.Throws<ArgumentNullException>(() => meshService.GetByGroup(null));
        }

        [Theory, AutoMeshInfoData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedMeshService<Guid> meshService, string group)
        {
            Assert.Throws<KeyNotFoundException>(() => meshService.GetFullNames(group));
        }

        [Theory, AutoMeshInfoData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedMeshService<Guid> meshService)
        {
            Assert.Throws<ArgumentNullException>(() => meshService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => meshService.GetFullNames(""));
        }
        
        [Theory, AutoMeshInfoData]
        public void GetManyIsOk(GroupedMeshService<Guid> meshService)
        {
            var ids = meshService.GetAll().Select(m => m.Id).ToArray();
            Assert.Equal(_repeatCount, meshService.Get(ids).Count());
        }

        [Theory, AutoMeshInfoData]
        public void GetIsOk(GroupedMeshService<Guid> meshService)
        {
            var id = meshService.GetAll().Select(m => m.Id).First();
            Assert.Equal(id, meshService.Get(id).Id);
        }

        [Theory, AutoMeshInfoData]
        public void GetAllIsOk(GroupedMeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.GetAll().Count());
        }

        [Theory, AutoMeshInfoData]
        public void GetIdsIsOk(GroupedMeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.GetIds().Count());
        }


        [Theory, AutoMeshInfoData]
        public void CountIsOk(GroupedMeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.Count());
        }

        [Theory, AutoMeshInfoData]
        public void ExistsIsOk(GroupedMeshService<Guid> meshService)
        {
            var meshInfo = meshService.GetAll().ToArray()[0];
            Assert.True(meshService.Exists(meshInfo.Id));
        }

        [Theory, AutoMeshInfoData]
        public void DoesNotExistIsOk(GroupedMeshService<Guid> meshService, Guid id)
        {
            Assert.False(meshService.Exists(id));
        }

        [Theory, AutoMeshInfoData]
        public void GetByGroupIsOk(GroupedMeshService<Guid> meshService)
        {
            var group = meshService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(meshService.GetByGroup(group).Any());
        }

        [Theory, AutoMeshInfoData]
        public void GetByGroupsIsOk(GroupedMeshService<Guid> meshService)
        {
            var groups = meshService.GetAll().Select(m => m.Group).ToArray();
            Assert.NotEmpty(groups);
            Assert.Equal(_repeatCount, meshService.GetByGroups(groups).Count());
        }

        [Theory, AutoMeshInfoData]
        public void GetFullNamesByGroupIsOk(GroupedMeshService<Guid> meshService)
        {
            var group = meshService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = meshService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoMeshInfoData]
        public void GetFullNamesIsOk(GroupedMeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.GetFullNames().Count());
        }
    }
}
// ReSharper disable UnusedMember.Global
namespace DHI.Services.Meshes.Test
{
    using DHI.Services.TimeSeries;
    using DHI.Spatial;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class MeshServiceTest
    {
        private const int _repeatCount = 3;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new MeshService<Guid>(null!));
            Assert.Equal("repository", exception.ParamName);
        }

        [Theory, AutoMeshInfoData]
        public void GetNonExistingThrows(MeshService<Guid> meshService, Guid id)
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
        public void GetManyIsOk(MeshService<Guid> meshService)
        {
            var ids = meshService.GetAll().Select(m => m.Id).ToArray();
            Assert.Equal(_repeatCount, meshService.Get(ids).Count());
        }

        [Theory, AutoMeshInfoData]
        public void GetIsOk(MeshService<Guid> meshService)
        {
            var id = meshService.GetAll().Select(m => m.Id).First();
            Assert.Equal(id, meshService.Get(id).Id);
        }

        [Theory, AutoMeshInfoData]
        public void GetAllIsOk(MeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.GetAll().Count());
        }

        [Theory, AutoMeshInfoData]
        public void GetIdsIsOk(MeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.GetIds().Count());
        }


        [Theory, AutoMeshInfoData]
        public void CountIsOk(MeshService<Guid> meshService)
        {
            Assert.Equal(_repeatCount, meshService.Count());
        }

        [Theory, AutoMeshInfoData]
        public void ExistsIsOk(MeshService<Guid> meshService)
        {
            var meshInfo = meshService.GetAll().ToArray()[0];
            Assert.True(meshService.Exists(meshInfo.Id));
        }

        [Theory, AutoMeshInfoData]
        public void DoesNotExistIsOk(MeshService<Guid> meshService, Guid id)
        {
            Assert.False(meshService.Exists(id));
        }
    }
}
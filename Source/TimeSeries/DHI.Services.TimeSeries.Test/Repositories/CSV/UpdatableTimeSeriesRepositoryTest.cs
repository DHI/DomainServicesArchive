namespace DHI.Services.TimeSeries.Test.Repositories.CSV
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using DHI.Services.TimeSeries;
    using DHI.Services.TimeSeries.CSV;
    using Xunit;

    public class UpdatableTimeSeriesRepositoryTest : IClassFixture<UpdatableTimeSeriesRepositoryFixture>
    {
        private readonly UpdatableTimeSeriesRepositoryFixture _fixture;

        public UpdatableTimeSeriesRepositoryTest(UpdatableTimeSeriesRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateWithNullRootThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatableTimeSeriesRepository(null));
        }

        [Fact]
        public void CreateWithIllegalRootThrows()
        {
            Assert.Throws<ArgumentException>(() => new UpdatableTimeSeriesRepository("NonExistingRoot"));
        }

        [Fact]
        public void GetIsOk()
        {
            string id = Path.GetFileNameWithoutExtension(_fixture.FileName);
            Assert.Equal(id, _fixture.Repository.Get(id).Value.Id);
        }

        [Fact]
        public void GetAllIsOk()
        {
            Assert.True(_fixture.Repository.GetAll().Any());
        }

        [Fact]
        public void GetIdsIsOk()
        {
            Assert.True(_fixture.Repository.GetIds().Any());
            Assert.IsType<string>(_fixture.Repository.GetIds().First());
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(1, _fixture.Repository.Count());
        }

        [Fact]
        public void ContainsIsOk()
        {
            Assert.True(_fixture.Repository.Contains(Path.GetFileNameWithoutExtension(_fixture.FileName)));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_fixture.Repository.Contains("NonExisting"));
        }

        [Fact]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            Assert.True(_fixture.Repository.GetLastDateTime(timeSeriesId).Value > _fixture.Repository.GetFirstDateTime(timeSeriesId).Value);
        }

        [Fact]
        public void GetFirstValueAfterIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            var firstDateTime = _fixture.Repository.GetFirstDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetFirstValueAfter(timeSeriesId, firstDateTime).Value;
            Assert.True(value.DateTime > firstDateTime);
        }

        [Fact]
        public void GetLastValueBeforeIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            var lastDateTime = _fixture.Repository.GetLastDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetLastValueBefore(timeSeriesId, lastDateTime).Value;
            Assert.True(value.DateTime < lastDateTime);
        }

        [Fact]
        public void GetValueIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            var lastDateTime = _fixture.Repository.GetLastDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetValue(timeSeriesId, lastDateTime).Value;
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Fact]
        public void GetValueCachingIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            var value = _fixture.Repository.GetValues(timeSeriesId).Value;
            Assert.True(value.Values.Count > 0);
            value = _fixture.Repository.GetValues(timeSeriesId).Value;
            Assert.True(value.Values.Count > 0);
        }

        [Fact]
        public void GetAllValuesIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            Assert.Equal(12, _fixture.Repository.GetValues(timeSeriesId).Value.DateTimes.Count);
            Assert.Equal(12, _fixture.Repository.GetValues(timeSeriesId).Value.Values.Count);
        }

        [Fact]
        public void GetValuesIsOk()
        {
            var timeSeriesId = Path.GetFileNameWithoutExtension(_fixture.FileName);
            var from = new DateTime(2015, 11, 12, 0, 0, 0);
            var to = new DateTime(2015, 11, 16, 0, 0, 0);
            Assert.Equal(4, _fixture.Repository.GetValues(timeSeriesId, from, to).Value.DateTimes.Count);
            Assert.Equal(4, _fixture.Repository.GetValues(timeSeriesId, from, to).Value.Values.Count);
        }

        [Theory, AutoData]
        public void AddAndRemoveIsOk(List<DateTime> dateTimes, List<double?> values, string id)
        {
            var timeSeriesData = new TimeSeriesData(dateTimes, values);
            var timeSeries = new TimeSeries(id, "some name", null, timeSeriesData);
            _fixture.Repository.Add(timeSeries);

            Assert.True(File.Exists(Path.Combine(_fixture.RootPath, id) + ".csv"));
            Assert.Equal(values.Count, _fixture.Repository.GetValues(id).Value.Values.Count);

            _fixture.Repository.Remove(id);
            Assert.False(_fixture.Repository.Contains(id));
        }

        [Theory, AutoData]
        public void SetValuesIsOk(List<DateTime> dateTimes, List<double?> values, string id)
        {
            var timeSeries = new TimeSeries(id, "some name");
            _fixture.Repository.Add(timeSeries);

            var timeSeriesData = new TimeSeriesData(dateTimes, values);
            _fixture.Repository.SetValues(id, timeSeriesData);

            Assert.Equal(values.Count, _fixture.Repository.GetValues(id).Value.Values.Count);
            _fixture.Repository.Remove(id);
        }

    }
}
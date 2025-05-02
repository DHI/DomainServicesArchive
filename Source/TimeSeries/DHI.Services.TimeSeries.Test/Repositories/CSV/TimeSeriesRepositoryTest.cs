namespace DHI.Services.TimeSeries.Test.Repositories.CSV
{
    using System;
    using System.IO;
    using System.Linq;
    using DHI.Services.TimeSeries.CSV;
    using Xunit;

    public class TimeSeriesRepositoryTest : IClassFixture<TimeSeriesRepositoryFixture>
    {
        private readonly TimeSeriesRepositoryFixture _fixture;

        public TimeSeriesRepositoryTest(TimeSeriesRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<IGroupedDiscreteTimeSeriesRepository<string, double>>(_fixture.Repository);
        }

        [Fact]
        public void CreateWithNullRootThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesRepository(null));
        }

        [Fact]
        public void CreateWithIllegalRootThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository("NonExistingRoot"));
        }

        [Fact]
        public void GetIsOk()
        {
            string id = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            Assert.Equal(id, _fixture.Repository.Get(id).Value.Id);
        }

        [Fact]
        public void GetAllIsOk()
        {
            Assert.Equal(2, _fixture.Repository.GetAll().Count());
        }

        [Fact]
        public void GetIdsIsOk()
        {
            Assert.Equal(2, _fixture.Repository.GetIds().Count());
            Assert.IsType<string>(_fixture.Repository.GetIds().First());
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(2, _fixture.Repository.Count());
        }

        [Fact]
        public void ContainsIsOk()
        {
            Assert.True(_fixture.Repository.Contains(@"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1"));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_fixture.Repository.Contains(@"dir1\dir1.1\" + _fixture.FileName + ";NonExisting"));
        }

        [Theory]
        [InlineData("dir1/dir1.1/")]
        [InlineData(@"dir1\dir1.1\")]
        [InlineData("dir1/dir1.1")]
        [InlineData("dir1")]
        [InlineData("dir2")]
        [InlineData("dir2/")]
        public void ContainsGroupIsOk(string group)
        {
            Assert.True(_fixture.Repository.ContainsGroup(group));
        }

        [Theory]
        [InlineData("dir1/dir1.2/")]
        [InlineData("dir3")]
        [InlineData("/dir2")]
        public void DoesNotContainGroupIsOk(string group)
        {
            Assert.False(_fixture.Repository.ContainsGroup(group));
        }

        [Theory]
        [InlineData("dir1/dir1.1/", 2)]
        [InlineData("dir1", 2)]
        [InlineData("dir2", 0)]
        public void GetByGroupIsOk(string group, int count)
        {
            Assert.Equal(count, _fixture.Repository.GetByGroup(group).Count());
        }

        [Theory]
        [InlineData("dir1/dir1.1/", 2)]
        [InlineData("dir1", 2)]
        [InlineData("dir2", 0)]
        [InlineData("", 2)]
        public void GetFullNamesByGroupIsOk(string group, int count)
        {
            Assert.Equal(count, _fixture.Repository.GetFullNames(group).Count());
        }

        [Fact]
        public void GetFullNamesIsOk()
        {
            Assert.Equal(2, _fixture.Repository.GetFullNames().Count());
        }

        [Fact]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            Assert.True(_fixture.Repository.GetLastDateTime(timeSeriesId).Value > _fixture.Repository.GetFirstDateTime(timeSeriesId).Value);
        }

        [Fact]
        public void GetFirstValueAfterIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            var firstDateTime = _fixture.Repository.GetFirstDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetFirstValueAfter(timeSeriesId, firstDateTime).Value;
            Assert.True(value.DateTime > firstDateTime);
        }

        [Fact]
        public void GetLastValueBeforeIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            var lastDateTime = _fixture.Repository.GetLastDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetLastValueBefore(timeSeriesId, lastDateTime).Value;
            Assert.True(value.DateTime < lastDateTime);
        }

        [Fact]
        public void GetValueIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            var lastDateTime = _fixture.Repository.GetLastDateTime(timeSeriesId).Value;
            var value = _fixture.Repository.GetValue(timeSeriesId, lastDateTime).Value;
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Fact]
        public void GetValueCachingIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            var value = _fixture.Repository.GetValues(timeSeriesId).Value;
            Assert.True(value.Values.Count > 0);
            value = _fixture.Repository.GetValues(timeSeriesId).Value;
            Assert.True(value.Values.Count > 0);
        }

        [Fact]
        public void GetAllValuesIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            Assert.Equal(12, _fixture.Repository.GetValues(timeSeriesId).Value.DateTimes.Count);
            Assert.Equal(12, _fixture.Repository.GetValues(timeSeriesId).Value.Values.Count);
        }

        [Fact]
        public void GetValuesIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries1";
            var from = new DateTime(2015, 11, 12, 0, 0, 0);
            var to = new DateTime(2015, 11, 16, 0, 0, 0);
            Assert.Equal(4, _fixture.Repository.GetValues(timeSeriesId, from, to).Value.DateTimes.Count);
            Assert.Equal(4, _fixture.Repository.GetValues(timeSeriesId, from, to).Value.Values.Count);
        }

        [Fact]
        public void GetInterpolatedValueIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";TimeSeries2";
            var interpolatedDateTime = new DateTime(2015, 11, 19, 22, 52, 31);
            var timeSeriesService = new TimeSeriesService<string, double>(_fixture.Repository);
            Assert.Equal(115.0, timeSeriesService.GetInterpolatedValue(timeSeriesId, interpolatedDateTime).Value);
        }

        [Fact]
        public void GetValuesForNonExistingFileReturnsEmptyMaybeIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\NonExistingFile;NonExistingItem";
            var from = new DateTime(2015, 11, 12, 0, 0, 0);
            var to = new DateTime(2015, 11, 16, 0, 0, 0);
            Assert.False(_fixture.Repository.GetValues(timeSeriesId, from, to).HasValue);
        }

        [Fact]
        public void GetValuesForNonExistingItemReturnsEmptyMaybeIsOk()
        {
            var timeSeriesId = @"dir1\dir1.1\" + _fixture.FileName + ";NonExistingItem";
            var from = new DateTime(2015, 11, 12, 0, 0, 0);
            var to = new DateTime(2015, 11, 16, 0, 0, 0);
            Assert.False(_fixture.Repository.GetValues(timeSeriesId, from, to).HasValue);
        }

        [Fact]
        public void GetValuesForEmptyRowIsOk()
        {
            const string csv = @"yyyy-MM-ddTHH:mm:ss;empty;ones
2024-05-01T10:00:00;;1
";
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, _fixture.FileName), csv);

            var repository = new TimeSeriesRepository(_fixture.TempDirectory);
            var empty = repository.GetValues("Test.csv;empty");
            var ones = repository.GetValues("Test.csv;ones");

            var emptyValue = Assert.Single(empty.Value.Values);
            Assert.Null(emptyValue);

            var oneValue = Assert.Single(ones.Value.Values);
            Assert.Equal(1, oneValue);
        }
    }
}
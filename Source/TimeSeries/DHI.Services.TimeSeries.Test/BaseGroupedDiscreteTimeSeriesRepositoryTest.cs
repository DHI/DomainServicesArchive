namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class BaseGroupedDiscreteTimeSeriesRepositoryTest : IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public BaseGroupedDiscreteTimeSeriesRepositoryTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }
        
        [Theory, AutoTimeSeriesData]
        public void CountIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.Equal(timeSeries.Id, repository.Get(timeSeries.Id).Value.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetReturnsEmptyMaybeIfNonExistingId(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.False(repository.Get(Guid.NewGuid()).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.NotEmpty(repository.GetWithValues(timeSeries.Id).Value.Data.DateTimes);
            Assert.NotEmpty(repository.GetWithValues(timeSeries.Id).Value.Data.Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromToIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var from = repository.GetFirstDateTime(timeSeries.Id).Value.AddMilliseconds(1);
            var to = repository.GetLastDateTime(timeSeries.Id).Value.AddMilliseconds(-1);
            Assert.Equal(timeSeries.Data.Values.Count - 2, repository.GetWithValues(timeSeries.Id, from, to).Value.Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesReturnsEmptyMaybeIfNonExistingId(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.False(repository.GetWithValues(Guid.NewGuid()).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var dictionary = repository.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id, Guid.NewGuid() });

            Assert.Equal(2, dictionary.Count);
            Assert.NotEmpty(dictionary[timeSeries1.Id].Data.DateTimes);
            Assert.NotEmpty(dictionary[timeSeries1.Id].Data.Values);
            Assert.NotEmpty(dictionary[timeSeries2.Id].Data.DateTimes);
            Assert.NotEmpty(dictionary[timeSeries2.Id].Data.Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForMultipleReturnsEmptyCollectionIfNonExistingIds(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Empty(repository.GetWithValues(new[] {Guid.NewGuid(), Guid.NewGuid()} ));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromToForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var from = repository.GetFirstDateTime(timeSeries1.Id).Value.AddMilliseconds(1);
            var to = repository.GetLastDateTime(timeSeries1.Id).Value.AddMilliseconds(-1);
            var dictionary = repository.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, dictionary[timeSeries1.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.True(repository.GetFullNames().Any());
            Assert.IsType<string>(repository.GetFullNames().First());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIdsIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.True(repository.GetIds().Any());
            Assert.IsType<Guid>(repository.GetIds().First());
        }

        [Theory, AutoTimeSeriesData]
        public void ContainsIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.True(repository.Contains(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void DoesNotContainIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.False(repository.Contains(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void ContainsDateTimeIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var dateTime = timeSeries.Data.DateTimes.Max();
            Assert.True(repository.ContainsDateTime(timeSeries.Id, dateTime));
            Assert.False(repository.ContainsDateTime(timeSeries.Id, dateTime.AddHours(1)));
        }

        [Theory, AutoTimeSeriesData]
        public void GetDateTimesIsOk(IDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.Equal(timeSeries.Data.DateTimes.Count, repository.GetDateTimes(timeSeries.Id).Value.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstDateTimeIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.Equal(timeSeries.Data.DateTimes.Min(), repository.GetFirstDateTime(timeSeries.Id).Value);
        }

        [Fact]
        public void GetFirstDateTimeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstDateTime(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(firstDateTime, repository.GetFirstValue(timeSeries.Id).Value.DateTime);
        }

        [Fact]
        public void GetFirstValueReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstValue(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var firstValues = repository.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(firstValues[timeSeries1.Id].DateTime, repository.GetFirstDateTime(timeSeries1.Id).Value);
            Assert.Equal(firstValues[timeSeries2.Id].DateTime, repository.GetFirstDateTime(timeSeries2.Id).Value);
        }

        [Fact]
        public void GetFirstValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var firstValues = repository.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(firstValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values[1], repository.GetFirstValueAfter(timeSeries.Id, firstDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes[1], repository.GetFirstValueAfter(timeSeries.Id, firstDateTime).Value.DateTime);
        }

        [Fact]
        public void GetFirstValueAfterReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().Last();
            Assert.Equal(timeSeries.Data.DateTimes.Max(), repository.GetLastDateTime(timeSeries.Id).Value);
        }

        [Fact]
        public void GetLastDateTimeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastDateTime(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            Assert.Equal(lastDateTime, repository.GetLastValue(timeSeries.Id).Value.DateTime);
        }

        [Fact]
        public void GetLastValueReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastValue(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var lastValues = repository.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(lastValues[timeSeries1.Id].DateTime, repository.GetLastDateTime(timeSeries1.Id).Value);
            Assert.Equal(lastValues[timeSeries2.Id].DateTime, repository.GetLastDateTime(timeSeries2.Id).Value);
        }

        [Fact]
        public void GetLastValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var lastValues = repository.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var values = repository.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var from = repository.GetFirstDateTime(timeSeries1.Id).Value.AddMilliseconds(1);
            var to = repository.GetLastDateTime(timeSeries1.Id).Value.AddMilliseconds(-1);
            var values = repository.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            var i = timeSeries.Data.Values.Count - 2;
            Assert.Equal(timeSeries.Data.Values[i], repository.GetLastValueBefore(timeSeries.Id, lastDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes[i], repository.GetLastValueBefore(timeSeries.Id, lastDateTime).Value.DateTime);
        }

        [Fact]
        public void GetLastValueBeforeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values.First(), repository.GetValue(timeSeries.Id, firstDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes.First(), repository.GetValue(timeSeries.Id, firstDateTime).Value.DateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmptyMaybe(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.False(repository.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForMultipleForNonExistingReturnsEmpty(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Empty(repository.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MinValue, DateTime.MaxValue).Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueForNonExistingReturnsNull(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Null(repository.GetAggregatedValue(Guid.NewGuid(), AggregationType.Average, DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesForMultipleForNonExistingReturnsEmpty(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Empty(repository.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Average, DateTime.MinValue, DateTime.MaxValue).Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values.Count, repository.GetValues(timeSeries.Id, firstDateTime, lastDateTime).Value.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;

            Assert.Equal(timeSeries.Data.Values.Max(), repository.GetAggregatedValue(timeSeries.Id, AggregationType.Maximum, firstDateTime, lastDateTime).Value);
            Assert.Equal(timeSeries.Data.Values.Min(), repository.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, firstDateTime, lastDateTime).Value);
            Assert.Equal(timeSeries.Data.Values.Sum(), repository.GetAggregatedValue(timeSeries.Id, AggregationType.Sum, firstDateTime, lastDateTime).Value);
            Assert.Equal(timeSeries.Data.Values.Average(), repository.GetAggregatedValue(timeSeries.Id, AggregationType.Average, firstDateTime, lastDateTime).Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesForMultipleIsOk(IGroupedDiscreteTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];

            var maxValues = repository.GetAggregatedValues(new[] {timeSeries1.Id, timeSeries2.Id}, AggregationType.Maximum, DateTime.MinValue, DateTime.MaxValue);
            var minValues = repository.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, DateTime.MinValue, DateTime.MaxValue);

            Assert.Equal(timeSeries1.Data.Values.Max(), maxValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Max(), maxValues[timeSeries2.Id]);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }
    }
}
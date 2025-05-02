namespace DHI.Services.TimeSeries.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using CSV;
    using DHI.Services.TimeSeries;
    using Logging;
    using Xunit;

    public class TimeSeriesServiceTest: IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public TimeSeriesServiceTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesService<Guid, float>(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadForNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmpty(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(Guid.NewGuid(), to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueToWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesToWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesToWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesToWithIllegalIntervalForMultipleThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueWithNonExistingDateReturnsNull(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetValue(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetVectors(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetVectors(Guid.NewGuid(), Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsToWithIllegalIntervalThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetVectors(Guid.NewGuid(), Guid.NewGuid(), to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetInterpolatedValueForIllegalDataTypeThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<Exception>(() => timeSeriesService.GetInterpolatedValue(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterWithIllegalDateThrows(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeWithIllegalDateThrows(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries.Id, ts.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray();
            timeSeriesService.TryGet(timeSeries.Select(t => t.Id), out var ts);
            var myTimeSeries = ts.ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyWithNonExistingIdIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray();
            var ids = timeSeries.Select(e => e.Id).ToList();
            var nonExistingId = Guid.NewGuid();
            ids.Add(nonExistingId);
            timeSeriesService.TryGet(ids, out var ts);
            var myTimeSeries = ts.Where(t => t != null).ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
            Assert.DoesNotContain(nonExistingId, myTimeSeries.Select(t => t.Id));
        }
        
        [Theory, AutoTimeSeriesData]
        public void GetWithValuesIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.NotEmpty(timeSeriesService.GetWithValues(timeSeries.Id).Data.DateTimes);
            Assert.NotEmpty(timeSeriesService.GetWithValues(timeSeries.Id).Data.Values);
        }
       
        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetWithValues(timeSeries.Id, firstDateTime.AddMilliseconds(1)).Data;
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetWithValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1)).Data;
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetWithValues(timeSeries.Id, from, to).Data;
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromNullToNullIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetWithValues(timeSeries.Id, from, to).Data;
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetWithValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1)).Data;
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id, Guid.NewGuid() });

            Assert.Equal(2, dictionary.Count);
            Assert.NotEmpty(dictionary[timeSeries1.Id].Data.DateTimes);
            Assert.NotEmpty(dictionary[timeSeries1.Id].Data.Values);
            Assert.NotEmpty(dictionary[timeSeries2.Id].Data.DateTimes);
            Assert.NotEmpty(dictionary[timeSeries2.Id].Data.Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, dictionary[timeSeries1.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, dictionary[timeSeries1.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id)).AddMilliseconds(-1);
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, dictionary[timeSeries1.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesFromNullToNullForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            DateTime? from = null;
            DateTime? to = null;
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount, dictionary[timeSeries1.Id].Data.Values.Count);
            Assert.Equal(_fixture.RepeatCount, dictionary[timeSeries2.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var dictionary = timeSeriesService.GetWithValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, dictionary[timeSeries1.Id].Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForMultipleReturnsEmptyCollectionIfNonExistingIds(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }));
        }

        [Theory, AutoTimeSeriesData]
        public void GetDateTimesIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Equal(timeSeries.Data.Values.Count, timeSeriesService.GetDateTimes(timeSeries.Id).Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.GetLastDateTime(timeSeries.Id) > timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstValue = timeSeriesService.GetFirstValue(timeSeries.Id);
            Assert.Equal(firstValue.DateTime, timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstValueReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstValues = timeSeriesService.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(firstValues[timeSeries1.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries1.Id));
            Assert.Equal(firstValues[timeSeries2.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetFirstValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstValues = timeSeriesService.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(firstValues.Any());
        }

        [Fact]
        public void GetLastDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastValue = timeSeriesService.GetLastValue(timeSeries.Id);
            Assert.Equal(lastValue.DateTime, timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetLastValueReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(lastValues[timeSeries1.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries1.Id));
            Assert.Equal(lastValues[timeSeries2.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetLastValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var value = timeSeriesService.GetFirstValueAfter(timeSeries.Id, firstDateTime);
            Assert.True(value.DateTime > firstDateTime);
        }

        [Fact]
        public void GetFirstValueAfterReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetLastValueBefore(timeSeries.Id, lastDateTime);
            Assert.True(value.DateTime < lastDateTime);
        }

        [Fact]
        public void GetLastValueBeforeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new TimeSeriesService<Guid, float>(repository);
            var discreteTimeSeriesService = new DiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetValue(timeSeries.Id, lastDateTime);
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var data = timeSeriesService.GetValues(timeSeries.Id);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, DateTime.MinValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, to: DateTime.MaxValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetVectors(timeSeries.Id, timeSeries.Id, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetVectors(timeSeries.Id, timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetVectors(timeSeries.Id, timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromNullToNullIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetVectors(timeSeries.Id, timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsOverloadIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetVectors(timeSeries.Id, timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id)).AddMilliseconds(-1);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            DateTime? from = null;
            DateTime? to = null;
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new [] {timeSeries1.Id, timeSeries2.Id}, AggregationType.Minimum);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, DateTime.MinValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, to: DateTime.MaxValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromToIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllVectorsForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id );
            var vector2Ids = vector1Ids;
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids });
            Assert.Equal(_fixture.RepeatCount, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
            Assert.Equal(_fixture.RepeatCount, vectors[$"{vector2Ids.idx}; {vector2Ids.idy}"].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id);
            var vector2Ids = vector1Ids;
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id);
            var vector2Ids = vector1Ids;
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromToForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id);
            var vector2Ids = vector1Ids;
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsFromNullToNullForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id);
            var vector2Ids = vector1Ids;
            DateTime? from = null;
            DateTime? to = null;
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids }, from, to);
            Assert.Equal(_fixture.RepeatCount, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
            Assert.Equal(_fixture.RepeatCount, vectors[$"{vector2Ids.idx}; {vector2Ids.idy}"].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetVectorsOverloadForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var vector1Ids = (idx: timeSeries.Id, idy: timeSeries.Id);
            var vector2Ids = vector1Ids;
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var vectors = timeSeriesService.GetVectors(new[] { vector1Ids, vector2Ids }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, vectors[$"{vector1Ids.idx}; {vector1Ids.idy}"].Values.Count);
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = TimeSeriesService<string, double>.GetRepositoryTypes();

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = TimeSeriesService<string, double>.GetRepositoryTypes(path);

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = TimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = TimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}
namespace DHI.Services.TimeSeries.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using AutoFixture;
    using CSV;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class GroupedDiscreteTimeSeriesServiceTest: IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public GroupedDiscreteTimeSeriesServiceTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedDiscreteTimeSeriesService<Guid, float>(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetNonExistingThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForNonExistingThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadForNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupForNonExistingThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupForNullGroupThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentNullException>(() => timeSeriesService.GetByGroup(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentNullException>(() => timeSeriesService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetFullNames(""));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstDateTimeForNotExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForNotExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
       public void GetLastDateTimeForNotExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForNotExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterForNotExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValueAfter(Guid.NewGuid(), DateTime.Now));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmptyData(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid()).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForMultipleNonExistingReturnsEmptyCollection(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingDateReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetValue(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetInterpolatedValueForIllegalDataTypeThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<Exception>(() => timeSeriesService.GetInterpolatedValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForNonExistingReturnsEmptyData(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalForMultipleThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalForMultipleThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalForMultipleThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForMultipleNonExistingReturnsEmptyCollection(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MinValue, DateTime.MaxValue).Any());
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
        public void GetFirstValueAfterWithIllegalDateThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeWithIllegalDateThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeForNonExistingReturnsNull(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValueBefore(Guid.NewGuid(), DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void AddValuesForNonExistingThrows(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.SetValues(Guid.NewGuid(), new TimeSeriesData<float>()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries.Id, ts.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray();
            timeSeriesService.TryGet(timeSeries.Select(t => t.Id), out var ts);
            var myTimeSeries = ts.ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyWithNonExistingIdIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray();
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
        public void GetWithValuesIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
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
        public void GetWithValuesForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
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
        public void GetWithValuesForMultipleReturnsEmptyCollectionIfNonExistingIds(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var group = timeSeriesService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(timeSeriesService.GetByGroup(group).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupsIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var group = timeSeriesService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(timeSeriesService.GetByGroups(new List<string> { group, group }).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesByGroupIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var group = timeSeriesService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = timeSeriesService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetFullNames().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetAll().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIdsIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetIds().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void CountIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void ExistsIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.Exists(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void DoesNotExistsIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.GetLastDateTime(timeSeries.Id) > timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstValue = timeSeriesService.GetFirstValue(timeSeries.Id);
            Assert.Equal(firstValue.DateTime, timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstValueReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstValues = timeSeriesService.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(firstValues[timeSeries1.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries1.Id));
            Assert.Equal(firstValues[timeSeries2.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetFirstValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstValues = timeSeriesService.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(firstValues.Any());
        }

        [Fact]
        public void GetLastDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastValue = timeSeriesService.GetLastValue(timeSeries.Id);
            Assert.Equal(lastValue.DateTime, timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetLastValueReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(lastValues[timeSeries1.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries1.Id));
            Assert.Equal(lastValues[timeSeries2.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetLastValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var value = timeSeriesService.GetFirstValueAfter(timeSeries.Id, firstDateTime);
            Assert.True(value.DateTime > firstDateTime);
        }

        [Fact]
        public void GetFirstValueAfterReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetLastValueBefore(timeSeries.Id, lastDateTime);
            Assert.True(value.DateTime < lastDateTime);
        }

        [Fact]
        public void GetLastValueBeforeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedDiscreteTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetValue(timeSeries.Id, lastDateTime);
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void SetValuesIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];

            var dateTime1 = new DateTime(2015, 1, 1);
            var point1 = new DataPoint<float>(dateTime1, 9999f);
            var dateTime2 = new DateTime(2015, 1, 2);
            var point2 = new DataPoint<float>(dateTime2, 8888f);
            var data = new TimeSeriesData<float>();
            data.DateTimes.Add(point1.DateTime);
            data.DateTimes.Add(point2.DateTime);
            data.Values.Add(point1.Value);
            data.Values.Add(point2.Value);
            timeSeriesService.SetValues(timeSeries.Id, data);
            
            Assert.Equal(point1, timeSeriesService.GetValue(timeSeries.Id, dateTime1));
            Assert.Equal(point2, timeSeriesService.GetValue(timeSeries.Id, dateTime2));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var data = timeSeriesService.GetValues(timeSeries.Id);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id)).AddMilliseconds(-1);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            DateTime? from = null;
            DateTime? to = null;
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadForMultipleIsOk(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = GroupedDiscreteTimeSeriesService<string, double>.GetRepositoryTypes();

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = GroupedDiscreteTimeSeriesService<string, double>.GetRepositoryTypes(path);

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = GroupedDiscreteTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = GroupedDiscreteTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }

        [Theory, AutoTimeSeriesData]
        public void EventIsRaisedOnSetValues(GroupedDiscreteTimeSeriesService<Guid, float> timeSeriesService, DateTime[] dateTimes, float[] values)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.ValuesSet += (s, e) => { raisedEvents.Add("Values set"); };
            var id = timeSeriesService.GetIds().First();
            timeSeriesService.SetValues(id, new TimeSeriesData<float>(dateTimes, values));

            Assert.Equal("Values set", raisedEvents[0]);
        }
    }
}
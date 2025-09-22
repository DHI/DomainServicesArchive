namespace DHI.Services.TimeSeries.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class GroupedUpdatableTimeSeriesServiceTest: IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public GroupedUpdatableTimeSeriesServiceTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedUpdatableTimeSeriesService<Guid, float>(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadForNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.Update(timeSeries));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.Remove(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveByNonExistingGroupThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.RemoveByGroup("NonExistingGroup"));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupForNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupForNullGroupThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentNullException>(() => timeSeriesService.GetByGroup(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentNullException>(() => timeSeriesService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetFullNames(""));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstDateTimeForNotExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForNotExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeForNotExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForNotExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterForNotExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValueAfter(Guid.NewGuid(), DateTime.Now));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmptyData(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid()).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForMultipleNonExistingReturnsEmptyCollection(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingDateReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetValue(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetInterpolatedValueForIllegalDataTypeThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<Exception>(() => timeSeriesService.GetInterpolatedValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForNonExistingReturnsEmptyData(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalForMultipleThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalForMultipleThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalForMultipleThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForMultipleNonExistingReturnsEmptyCollection(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetFirstValueAfterWithIllegalDateThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeWithIllegalDateThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeForNonExistingReturnsNull(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValueBefore(Guid.NewGuid(), DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void AddValuesForNonExistingThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.SetValues(Guid.NewGuid(), new TimeSeriesData<float>()));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesToWithIllegalIntervalThrows(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries.Id, ts.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray();
            timeSeriesService.TryGet(timeSeries.Select(t => t.Id), out var ts);
            var myTimeSeries = ts.ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyWithNonExistingIdIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesForMultipleReturnsEmptyCollectionIfNonExistingIds(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }));
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var group = timeSeriesService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(timeSeriesService.GetByGroup(group).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetByGroupsIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var group = timeSeriesService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(timeSeriesService.GetByGroups(new List<string> { group, group }).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFullNamesByGroupIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetFullNamesIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetFullNames().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetAll().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIdsIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetIds().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void CountIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void ExistsIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.Exists(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void DoesNotExistsIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void AddAndGetIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            timeSeriesService.Add(timeSeries);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries.Id, ts.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void AddMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var count = timeSeriesService.Count();
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>();
            timeSeriesService.Add(timeSeriesList);
            Assert.Equal(count + _fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void EventsAreRaisedOnAdd(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            timeSeriesService.Added += (s, e) => { raisedEvents.Add("Added"); };

            timeSeriesService.Add(timeSeries);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            timeSeriesService.Add(timeSeries);
            timeSeriesService.Remove(timeSeries.Id);

            Assert.False(timeSeriesService.Exists(timeSeries.Id));
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveByGroupIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            const string myGroup = "MyGroup";
            const string anotherGroup = "anotherGroup";
            var foo = new TimeSeries<Guid,float>(Guid.NewGuid(), "foo", myGroup);
            var bar = new TimeSeries<Guid, float>(Guid.NewGuid(), "bar", myGroup);
            var baz = new TimeSeries<Guid, float>(Guid.NewGuid(), "baz", anotherGroup);
            timeSeriesService.Add(foo);
            timeSeriesService.Add(bar);
            timeSeriesService.Add(baz);
            timeSeriesService.RemoveByGroup(myGroup);

            Assert.False(timeSeriesService.Exists(foo.Id));
            Assert.False(timeSeriesService.Exists(bar.Id));
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetByGroup(myGroup));
            Assert.Single(timeSeriesService.GetByGroup(anotherGroup));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            timeSeriesService.Add(timeSeriesList);
            var ids = timeSeriesList.Select(t => t.Id).ToList();
            timeSeriesService.Remove(ids);

            Assert.False(timeSeriesService.Exists(ids[0]));
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void EventsAreRaisedOnRemove(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            timeSeriesService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            timeSeriesService.Add(timeSeries);

            timeSeriesService.Remove(timeSeries.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoTimeSeriesData]
        public void EventsAreRaisedOnRemoveByGroup(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.DeletingGroup += (s, e) => { raisedEvents.Add("DeletingGroup"); };
            timeSeriesService.DeletedGroup += (s, e) => { raisedEvents.Add("DeletedGroup"); };
            timeSeriesService.Add(timeSeries);

            timeSeriesService.RemoveByGroup(timeSeries.Group);

            Assert.Equal("DeletingGroup", raisedEvents[0]);
            Assert.Equal("DeletedGroup", raisedEvents[1]);
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            timeSeriesService.Add(timeSeries);
            var updatedTimeSeries = new TimeSeries<Guid, float>(timeSeries.Id, "Updated name");
            timeSeriesService.Update(updatedTimeSeries);

            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(updatedTimeSeries.Name, ts.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void AddOrUpdateIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.Added += (s, e) => { raisedEvents.Add("Added"); };
            timeSeriesService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            timeSeriesService.AddOrUpdate(timeSeries);
            var updated = new TimeSeries<Guid, float>(timeSeries.Id, "Updated name");
            timeSeriesService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(updated.Name, ts.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void TryAddIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            Assert.True(timeSeriesService.TryAdd(timeSeries));
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries, ts);
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var updatedTimeSeries1 = new TimeSeries<Guid, float>(timeSeries1.Id, "Updated name1");
            var updatedTimeSeries2 = new TimeSeries<Guid, float>(timeSeries2.Id, "Updated name2");

            var updatedTimeSeriesList = new List<TimeSeries<Guid, float>>() { updatedTimeSeries1, updatedTimeSeries2 };
            timeSeriesService.Update(updatedTimeSeriesList);

            timeSeriesService.TryGet(timeSeries1.Id, out var ts1);
            timeSeriesService.TryGet(timeSeries2.Id, out var ts2);
            
            Assert.Equal(updatedTimeSeries1.Name, ts1.Name);
            Assert.Equal(updatedTimeSeries2.Name, ts2.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void EventsAreRaisedOnUpdate(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            timeSeriesService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            timeSeriesService.Add(timeSeries);

            var updatedAccount = new TimeSeries<Guid, float>(timeSeries.Id, "Updated name");
            timeSeriesService.Update(updatedAccount);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoTimeSeriesData]
        public void EventIsRaisedOnSetValues(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries, DateTime[] dateTimes, float[] values)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.ValuesSet += (s, e) => { raisedEvents.Add("Values set"); };
            timeSeriesService.Add(timeSeries);
            timeSeriesService.SetValues(timeSeries.Id, new TimeSeriesData<float>(dateTimes, values));

            Assert.Equal("Values set", raisedEvents[0]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.GetLastDateTime(timeSeries.Id) > timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new GroupedUpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetValue(timeSeries.Id, lastDateTime);
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void SetValuesIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetAllValuesIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var data = timeSeriesService.GetValues(timeSeries.Id);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id)).AddMilliseconds(-1);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetValuesOverloadForMultipleIsOk(GroupedUpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveAllValuesIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.RemoveValues(timeSeries.Id);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(0, ts.Data.DateTimes.Count);
            Assert.Equal(0, ts.Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            timeSeriesService.RemoveValues(timeSeries.Id, firstDateTime.AddMilliseconds(1));
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(1, ts.Data.DateTimes.Count);
            Assert.Equal(1, ts.Data.Values.Count);
            Assert.Equal(firstDateTime, ts.Data.DateTimes.First());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesToIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            timeSeriesService.RemoveValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(1, ts.Data.DateTimes.Count);
            Assert.Equal(1, ts.Data.Values.Count);
            Assert.Equal(lastDateTime, ts.Data.DateTimes.First());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromToIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            timeSeriesService.RemoveValues(timeSeries.Id, from, to);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(2, ts.Data.DateTimes.Count);
            Assert.Equal(2, ts.Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromNullToNullIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            timeSeriesService.RemoveValues(timeSeries.Id, from, to);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(0, ts.Data.DateTimes.Count);
            Assert.Equal(0, ts.Data.Values.Count);
        }
    }
}
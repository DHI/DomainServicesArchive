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
    using Xunit;

    public class UpdatableTimeSeriesServiceTest : IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public UpdatableTimeSeriesServiceTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatableTimeSeriesService<Guid, float>(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetNonExistingThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesForNonExistingThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetWithValuesOverloadForNonExistingThrows(TimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.GetWithValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateNonExistingThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.Update(timeSeries));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveNonExistingThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.Remove(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstDateTimeForNotExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForNotExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeForNotExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastDateTime(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForNotExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValue(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterForNotExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetFirstValueAfter(Guid.NewGuid(), DateTime.Now));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmptyData(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid()).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForMultipleNonExistingReturnsEmptyCollection(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }).Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingDateReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueForNonExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetValue(Guid.NewGuid(), DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetInterpolatedValueForIllegalDataTypeThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<Exception>(() => timeSeriesService.GetInterpolatedValue(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(timeSeries.Id, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForNonExistingReturnsEmptyData(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).Values.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalForMultipleThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromWithIllegalIntervalForMultipleThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToWithIllegalIntervalForMultipleThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesInIntervalForMultipleNonExistingReturnsEmptyCollection(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetFirstValueAfterWithIllegalDateThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeWithIllegalDateThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeForNonExistingReturnsNull(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Null(timeSeriesService.GetLastValueBefore(Guid.NewGuid(), DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void AddValuesForNonExistingThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<KeyNotFoundException>(() => timeSeriesService.SetValues(Guid.NewGuid(), new TimeSeriesData<float>()));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesToWithIllegalIntervalThrows(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Throws<ArgumentException>(() => timeSeriesService.RemoveValues(timeSeries.Id, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.TryGet(timeSeries.Id, out var series);
            Assert.Equal(timeSeries.Id, series.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray();
            timeSeriesService.TryGet(timeSeries.Select(t => t.Id), out var ts);
            var myTimeSeries = ts.ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyWithNonExistingIdIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetWithValuesForMultipleReturnsEmptyCollectionIfNonExistingIds(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetWithValues(new[] { Guid.NewGuid(), Guid.NewGuid() }));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetAll().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIdsIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.GetIds().Count());
        }

        [Theory, AutoTimeSeriesData]
        public void CountIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void ExistsIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.Exists(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void DoesNotExistsIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void AddAndGetIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
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
        public void EventsAreRaisedOnAdd(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            timeSeriesService.Added += (s, e) => { raisedEvents.Add("Added"); };

            timeSeriesService.Add(timeSeries);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            timeSeriesService.Add(timeSeries);
            timeSeriesService.Remove(timeSeries.Id);

            Assert.False(timeSeriesService.Exists(timeSeries.Id));
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveMultipeIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            timeSeriesService.Add(timeSeriesList);
            var ids = timeSeriesList.Select(t => t.Id).ToList();
            timeSeriesService.Remove(ids);

            Assert.False(timeSeriesService.Exists(ids[0]));
            Assert.Equal(_fixture.RepeatCount, timeSeriesService.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void EventsAreRaisedOnRemove(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
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
        public void UpdateIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
        {
            timeSeriesService.Add(timeSeries);
            var updatedtimeSeries = new TimeSeries<Guid, float>(timeSeries.Id, "Updated name");
            timeSeriesService.Update(updatedtimeSeries);
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(updatedtimeSeries.Name, ts.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void AddOrUpdateIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
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
        public void TryAddIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
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
        public void EventsAreRaisedOnUpdate(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries)
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
        public void EventIsRaisedOnSetValues(UpdatableTimeSeriesService<Guid, float> timeSeriesService, TimeSeries<Guid, float> timeSeries, DateTime[] dateTimes, float[] values)
        {
            var raisedEvents = new List<string>();
            timeSeriesService.ValuesSet += (s, e) => { raisedEvents.Add("Values set"); };
            timeSeriesService.Add(timeSeries);
            timeSeriesService.SetValues(timeSeries.Id, new TimeSeriesData<float>(dateTimes, values));

            Assert.Equal("Values set", raisedEvents[0]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetDateTimesIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Equal(timeSeries.Data.DateTimes.Count, timeSeriesService.GetDateTimes(timeSeries.Id).Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.True(timeSeriesService.GetLastDateTime(timeSeries.Id) > timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Fact]
        public void GetFirstDateTimeReturnsNullIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, UpdatableTimeSeriesService<Guid, float> UpdatableTimeSeriesService)
        {
            var timeSeries1 = UpdatableTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = UpdatableTimeSeriesService.GetAll().ToArray()[1];
            var firstValues = timeSeriesService.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(firstValues[timeSeries1.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries1.Id));
            Assert.Equal(firstValues[timeSeries2.Id].DateTime, timeSeriesService.GetFirstDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetFirstValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
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
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastDateTime(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValue(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(TimeSeriesService<Guid, float> timeSeriesService, UpdatableTimeSeriesService<Guid, float> UpdatableTimeSeriesService)
        {
            var timeSeries1 = UpdatableTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = UpdatableTimeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(lastValues[timeSeries1.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries1.Id));
            Assert.Equal(lastValues[timeSeries2.Id].DateTime, timeSeriesService.GetLastDateTime(timeSeries2.Id));
        }

        [Fact]
        public void GetLastValueForMultipleReturnsEmptyCollectionIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            var repository = new FakeTimeSeriesRepository(timeSeriesList);
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastValues = timeSeriesService.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
            var timeSeriesService = new UpdatableTimeSeriesService<Guid, float>(repository);
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetValue(timeSeries.Id, lastDateTime);
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void SetValuesIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetAllValuesIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var data = timeSeriesService.GetValues(timeSeries.Id);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id)).AddMilliseconds(-1);
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            DateTime? from = null;
            DateTime? to = null;
            var data = timeSeriesService.GetValues(timeSeries.Id, from, to);
            Assert.Equal(_fixture.RepeatCount, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries = timeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesToForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var lastDateTime = (DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, to: lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 1, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
        {
            var timeSeries1 = timeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = timeSeriesService.GetAll().ToArray()[1];
            var from = ((DateTime)timeSeriesService.GetFirstDateTime(timeSeries1.Id)).AddMilliseconds(1);
            var to = ((DateTime)timeSeriesService.GetLastDateTime(timeSeries1.Id)).AddMilliseconds(-1);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromNullToNullForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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
        public void GetValuesOverloadForMultipleIsOk(UpdatableTimeSeriesService<Guid, float> timeSeriesService)
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

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = UpdatableTimeSeriesService<string, double>.GetRepositoryTypes();

            Assert.Contains(typeof(UpdatableTimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = UpdatableTimeSeriesService<string, double>.GetRepositoryTypes(path);

            Assert.Contains(typeof(UpdatableTimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = UpdatableTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(UpdatableTimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = UpdatableTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}
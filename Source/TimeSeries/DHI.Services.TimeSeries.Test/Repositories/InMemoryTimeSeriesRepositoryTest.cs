namespace DHI.Services.TimeSeries.Test.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class InMemoryTimeSeriesRepositoryTest : IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public InMemoryTimeSeriesRepositoryTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Theory, AutoTimeSeriesData]
        public void CountIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.Equal(timeSeries.Id, repository.Get(timeSeries.Id).Value.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetIdsIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            Assert.True(repository.GetIds().Any());
            Assert.IsType<Guid>(repository.GetIds().First());
        }

        [Theory, AutoTimeSeriesData]
        public void ContainsIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.True(repository.Contains(timeSeries.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void DoesNotContainIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            Assert.False(repository.Contains(Guid.NewGuid()));
        }

        [Theory, AutoTimeSeriesData]
        public void AddAndGetIsOk(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            Assert.Equal(timeSeries.Id, repository.Get(timeSeries.Id).Value.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void AddMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var count = repository.Count();
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>();
            repository.Add(timeSeriesList);
            Assert.Equal(count + _fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveIsOk(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            repository.Remove(timeSeries.Id);

            Assert.False(repository.Contains(timeSeries.Id));
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveMultipeIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
            repository.Add(timeSeriesList);
            var ids = timeSeriesList.Select(t => t.Id).ToList();
            repository.Remove(ids);

            Assert.False(repository.Contains(ids[0]));
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateIsOk(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            var updatedtimeSeries = new TimeSeries<Guid, float>(timeSeries.Id, "Updated name");
            repository.Update(updatedtimeSeries);

            Assert.Equal(updatedtimeSeries.Name, repository.Get(timeSeries.Id).Value.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateValuesIsOk(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            var tsData1 = timeSeries.Data;
            repository.Add(timeSeries);

            // Change timeseries data and update
            var time = timeSeries.Data.DateTimes.Select(d => d.AddHours(1));
            var values = timeSeries.Data.Values.Select(d => d + 10);
            var tsData2 = new TimeSeriesData<float>(time.ToList(), values.ToList());
            var updatedTimeSeries = new TimeSeries<Guid, float>(timeSeries.Id, timeSeries.Name, timeSeries.Group, tsData2);
            repository.Update(updatedTimeSeries);

            DateTime t1 = tsData1.DateTimes.First();
            var v2 = tsData1.Values.First();
            var v1 = repository.GetValue(updatedTimeSeries.Id, t1);
            Assert.True(v1.HasValue);
            Assert.True(v2.HasValue);
            Assert.Equal(v2.Value, v1.Value.Value);
            var updatedValues = repository.GetValues(updatedTimeSeries.Id);
            Assert.True(updatedValues.Value.Values.Count == tsData1.Values.Count + tsData2.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void ChangeValuesIsOk(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            var time = DateTime.Now;
            var expectedValue = 123;
            timeSeries.Data.Append(time, expectedValue);

            var value1 = repository.GetValue(timeSeries.Id, time);
            Assert.False(value1.HasValue);

            repository.SetValues(timeSeries.Id, timeSeries.Data);

            var value2 = repository.GetValue(timeSeries.Id, time);
            Assert.True(value2.HasValue);
            Assert.Equal(expectedValue, value2.Value.Value.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void UpdateMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var updatedTimeSeries1 = new TimeSeries<Guid, float>(timeSeries1.Id, "Updated name1");
            var updatedTimeSeries2 = new TimeSeries<Guid, float>(timeSeries2.Id, "Updated name2");

            var updatedTimeSeriesList = new List<TimeSeries<Guid, float>>() { updatedTimeSeries1, updatedTimeSeries2 };
            repository.Update(updatedTimeSeriesList);

            Assert.Equal(updatedTimeSeries1.Name, repository.Get(timeSeries1.Id).Value.Name);
            Assert.Equal(updatedTimeSeries2.Name, repository.Get(timeSeries2.Id).Value.Name);
        }

        [Theory, AutoTimeSeriesData]
        public void ContainsDateTimeIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            Assert.True(repository.ContainsDateTime(timeSeries.Id, timeSeries.Data.DateTimes.First()));
        }

        [Theory, AutoTimeSeriesData]
        public void GetDateTimesIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            Assert.Equal(timeSeries.Data.DateTimes.Count, repository.GetDateTimes(timeSeries.Id).Value.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstDateTimeIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            Assert.Equal(timeSeries.Data.DateTimes.Min(), repository.GetFirstDateTime(timeSeries.Id).Value);
        }

        [Fact]
        public void GetFirstDateTimeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstDateTime(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(firstDateTime, repository.GetFirstValue(timeSeries.Id).Value.DateTime);
        }

        [Fact]
        public void GetFirstValueReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstValue(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueForMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
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
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var firstValues = repository.GetFirstValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(firstValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetFirstValueAfterIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values[1], repository.GetFirstValueAfter(timeSeries.Id, firstDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes[1], repository.GetFirstValueAfter(timeSeries.Id, firstDateTime).Value.DateTime);
        }

        [Fact]
        public void GetFirstValueAfterReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetFirstValueAfter(timeSeries.Id, DateTime.MinValue).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastDateTimeIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().Last();
            Assert.Equal(timeSeries.Data.DateTimes.Max(), repository.GetLastDateTime(timeSeries.Id).Value);
        }

        [Fact]
        public void GetLastDateTimeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastDateTime(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().First();
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            Assert.Equal(lastDateTime, repository.GetLastValue(timeSeries.Id).Value.DateTime);
        }

        [Fact]
        public void GetLastValueReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastValue(timeSeries.Id).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueForMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
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
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var lastValues = repository.GetLastValue(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.False(lastValues.Any());
        }

        [Theory, AutoTimeSeriesData]
        public void GetAllValuesForMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var values = repository.GetValues(new[] { timeSeries1.Id, timeSeries2.Id });
            Assert.Equal(_fixture.RepeatCount, values[timeSeries1.Id].Values.Count);
            Assert.Equal(_fixture.RepeatCount, values[timeSeries2.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesFromToForMultipleIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries1 = repository.GetAll().ToArray()[0];
            var timeSeries2 = repository.GetAll().ToArray()[1];
            var from = repository.GetFirstDateTime(timeSeries1.Id).Value.AddMilliseconds(1);
            var to = repository.GetLastDateTime(timeSeries1.Id).Value.AddMilliseconds(-1);
            var values = repository.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, from, to);
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetLastValueBeforeIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            var i = timeSeries.Data.Values.Count - 2;
            Assert.Equal(timeSeries.Data.Values[i], repository.GetLastValueBefore(timeSeries.Id, lastDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes[i], repository.GetLastValueBefore(timeSeries.Id, lastDateTime).Value.DateTime);
        }

        [Fact]
        public void GetLastValueBeforeReturnsEmptyMaybeIfNoValues()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var timeSeries = repository.GetAll().First();
            Assert.False(repository.GetLastValueBefore(timeSeries.Id, DateTime.MaxValue).HasValue);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values.First(), repository.GetValue(timeSeries.Id, firstDateTime).Value.Value);
            Assert.Equal(timeSeries.Data.DateTimes.First(), repository.GetValue(timeSeries.Id, firstDateTime).Value.DateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void SetValuesIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().ToArray()[0];

            var dateTime1 = new DateTime(2015, 1, 1);
            var point1 = new DataPoint<float>(dateTime1, 9999f);
            var dateTime2 = new DateTime(2015, 1, 2);
            var point2 = new DataPoint<float>(dateTime2, 8888f);
            var data = new TimeSeriesData<float>();
            data.DateTimes.Add(point1.DateTime);
            data.DateTimes.Add(point2.DateTime);
            data.Values.Add(point1.Value);
            data.Values.Add(point2.Value);
            repository.SetValues(timeSeries.Id, data);

            Assert.Equal(point1, repository.GetValue(timeSeries.Id, dateTime1).Value);
            Assert.Equal(point2, repository.GetValue(timeSeries.Id, dateTime2).Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesOverloadIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAllWithValues().First();
            var firstDateTime = repository.GetFirstDateTime(timeSeries.Id).Value;
            var lastDateTime = repository.GetLastDateTime(timeSeries.Id).Value;
            Assert.Equal(timeSeries.Data.Values.Count, repository.GetValues(timeSeries.Id, firstDateTime, lastDateTime).Value.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveAllValuesIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().ToArray()[0];
            repository.RemoveValues(timeSeries.Id);
            Assert.Equal(0, repository.Get(timeSeries.Id).Value.Data.DateTimes.Count);
            Assert.Equal(0, repository.Get(timeSeries.Id).Value.Data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void RemoveValuesFromToIsOk(InMemoryTimeSeriesRepository<Guid, float> repository)
        {
            var timeSeries = repository.GetAll().ToArray()[0];
            var from = repository.GetFirstDateTime(timeSeries.Id).Value.AddMilliseconds(1);
            var to = repository.GetLastDateTime(timeSeries.Id).Value.AddMilliseconds(-1);
            repository.RemoveValues(timeSeries.Id, from, to);
            Assert.Equal(2, repository.GetValues(timeSeries.Id).Value.DateTimes.Count);
            Assert.Equal(2, repository.GetValues(timeSeries.Id).Value.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetReturnsClone(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            var ts = repository.Get(timeSeries.Id).Value;
            ts.Metadata.Add("Description", "A description");

            Assert.Empty(repository.Get(timeSeries.Id).Value.Metadata);
        }

        [Theory, AutoTimeSeriesData]
        public void AddCreatesClone(InMemoryTimeSeriesRepository<Guid, float> repository, TimeSeries<Guid, float> timeSeries)
        {
            repository.Add(timeSeries);
            timeSeries.Metadata.Add("Description", "A description");
            
            Assert.Empty(repository.Get(timeSeries.Id).Value.Metadata);
        }

        [Fact]
        public void ConstructorCreatesClones()
        {
            var timeSeriesList = _fixture.CreateMany<TimeSeries<string, double>>().ToList();
            var id = timeSeriesList.First().Id;
            var expectedValue = 123;
            timeSeriesList[0] = TimeSeriesExtensions.CopyWith(timeSeriesList[0], new TimeSeriesData(DateTime.Now, expectedValue));

            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);

            timeSeriesList[0].Data.Values[0] = expectedValue + expectedValue; // Change value

            var timeSeriesFromRepo = repository.GetWithValues(id);
            Assert.Equal(expectedValue, timeSeriesFromRepo.Value.Data.Values[0]); // Must not change value in repo
        }
    }
}
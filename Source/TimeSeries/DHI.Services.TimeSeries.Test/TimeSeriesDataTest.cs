namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class TimeSeriesDataTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void ConstructorWithSingleDateTimeAndValueIsOk()
        {
            var data = new TimeSeriesData<float>(DateTime.Now, 999);
            var nullData = new TimeSeriesData<float>(DateTime.Now, null);

            Assert.Single(data.DateTimes);
            Assert.Single(data.Values);
            Assert.Single(nullData.DateTimes);
            Assert.Single(nullData.Values);
            Assert.Null(nullData.GetFirst().Value.Value);
        }

        [Fact]
        public void ConstructorWithSinglePointIsOk()
        {
            var point = new DataPoint<float>(DateTime.Now, 999);
            var data = new TimeSeriesData<float>(point);
            var nullPoint = new DataPoint<float>(DateTime.Now, null);
            var nullData = new TimeSeriesData<float>(nullPoint);

            Assert.Single(data.DateTimes);
            Assert.Single(data.Values);
            Assert.Single(nullData.DateTimes);
            Assert.Single(nullData.Values);
            Assert.Null(nullData.GetFirst().Value.Value);
        }

        [Fact]
        public void ConstructorWithMultipleDateTimesAndValuesIsOk()
        {
            var values = _fixture.CreateMany<float>();
            var dateTimes = _fixture.CreateMany<DateTime>().ToArray();
            var data = new TimeSeriesData<float>(dateTimes.ToList(), values.ToList());

            Assert.Equal(dateTimes.Length, data.DateTimes.Count);
            Assert.Equal(dateTimes, data.DateTimes);
        }

        [Fact]
        public void ConstructorWithConstantValueIsOk()
        {
            var value = _fixture.Create<float>();
            var dateTimes = _fixture.CreateMany<DateTime>().ToList();
            var data = new TimeSeriesData<float>(dateTimes, value);

            Assert.Equal(dateTimes.Count, data.DateTimes.Count);
            Assert.Equal(value, data.Values.First());
            Assert.Equal(value, data.Values.Last());
        }

        [Fact]
        public void CreateEquidistantDateTimesIsOk()
        {
            var t0 = DateTime.Now;
            var timeStep = new TimeSpan(1, 0, 0);
            var dateTimes = TimeSeriesData<float>.CreateEquidistantDateTimes(t0, t0.AddHours(24), timeStep).ToList();
            Assert.Equal(25, dateTimes.Count);
            Assert.Equal(t0.Add(timeStep), dateTimes[1]);
        }

        [Theory, AutoData]
        public void CountIsOk(TimeSeriesData<float> timeSeriesData)
        {
            var dateTimes = new SortedSet<DateTime>();
            _fixture.AddManyTo(dateTimes);
            foreach (var dateTime in dateTimes)
            {
                timeSeriesData.Append(dateTime, _fixture.Create<float>());
            }

            Assert.Equal(_fixture.RepeatCount, timeSeriesData.Count);
        }

        [Theory, AutoData]
        public void HasValuesIsOk(TimeSeriesData<float> timeSeriesData)
        {
            Assert.False(timeSeriesData.HasValues);
            var dateTimes = new SortedSet<DateTime>();
            _fixture.AddManyTo(dateTimes);
            foreach (var dateTime in dateTimes)
            {
                timeSeriesData.Append(dateTime, _fixture.Create<float>());
            }

            Assert.True(timeSeriesData.HasValues);
        }

        [Theory, AutoData]
        public void AppendIsOk(TimeSeriesData<float> timeSeriesData)
        {
            timeSeriesData.Append(_fixture.Create<DateTime>(), _fixture.Create<float?>());
            timeSeriesData.Append(_fixture.Create<DateTime>(), _fixture.Create<float?>());

            Assert.Equal(2, timeSeriesData.Count);
        }

        [Theory, AutoData]
        public void InsertIsOk(TimeSeriesData<float> timeSeriesData)
        {
            timeSeriesData.Insert(_fixture.Create<DateTime>(), _fixture.Create<float?>());
            timeSeriesData.Insert(_fixture.Create<DateTime>(), 999f);

            Assert.Equal(999f, timeSeriesData.Values.First());
        }

        [Fact]
        public void ToSortedSetIsOk()
        {
            var timeSeriesData = new TimeSeriesData<float>(_fixture.CreateMany<DateTime>().ToList(), _fixture.CreateMany<float?>().ToList());

            Assert.IsType<SortedSet<DataPoint<float>>>(timeSeriesData.ToSortedSet());
        }

        [Fact]
        public void ToSortedDictionaryIsOk()
        {
            var timeSeriesData = new TimeSeriesData<float>(_fixture.CreateMany<DateTime>().ToList(), _fixture.CreateMany<float?>().ToList());

            Assert.IsType<SortedDictionary<DateTime, float?>>(timeSeriesData.ToSortedDictionary());
        }
    }
}
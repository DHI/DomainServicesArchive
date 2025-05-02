namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class TimeSeriesDataWFlagTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void ConstructorWithSingleDateTimeAndValueIsOk()
        {
            var data = new TimeSeriesDataWFlag<float, string>(DateTime.Now, 999, "ok");
            var nullData = new TimeSeriesDataWFlag<float, string>(DateTime.Now, null, "error");

            Assert.Single(data.DateTimes);
            Assert.Single(data.Values);
            Assert.Single(data.Flags);
            Assert.Single(nullData.DateTimes);
            Assert.Single(nullData.Values);
            Assert.Single(nullData.Flags);
            Assert.Null(nullData.GetFirst().Value.Value);
        }

        [Fact]
        public void ConstructorWithSinglePointIsOk()
        {
            var point = new DataPointWFlag<float, int>(DateTime.Now, 999, 0);
            var data = new TimeSeriesDataWFlag<float, int>(point);
            var nullPoint = new DataPointWFlag<float, int>(DateTime.Now, null, 1);
            var nullData = new TimeSeriesDataWFlag<float, int>(nullPoint);

            Assert.Single(data.DateTimes);
            Assert.Single(data.Values);
            Assert.Single(data.Flags);
            Assert.Single(nullData.DateTimes);
            Assert.Single(nullData.Values);
            Assert.Single(nullData.Flags);
            Assert.Null(nullData.GetFirst().Value.Value);
        }

        [Theory, AutoData]
        public void CountIsOk(TimeSeriesDataWFlag<float, int?> timeSeriesDataWFlag)
        {
            var dateTimes = new SortedSet<DateTime>();
            _fixture.AddManyTo(dateTimes);
            foreach (var dateTime in dateTimes)
            {
                timeSeriesDataWFlag.Append(dateTime, _fixture.Create<float?>(), _fixture.Create<int?>());
            }

            Assert.Equal(_fixture.RepeatCount, timeSeriesDataWFlag.Count);
        }

        [Theory, AutoData]
        public void AppendIsOk(TimeSeriesDataWFlag<float, int?> timeSeriesDataWFlag)
        {
            timeSeriesDataWFlag.Append(_fixture.Create<DateTime>(), _fixture.Create<float?>(), _fixture.Create<int?>());
            timeSeriesDataWFlag.Append(_fixture.Create<DateTime>(), _fixture.Create<float?>(), _fixture.Create<int?>());

            Assert.Equal(2, timeSeriesDataWFlag.Count);
        }

        [Fact]
        public void ToSortedSetIsOk()
        {
            var timeSeriesDataWFlag = new TimeSeriesDataWFlag<float, int?>(_fixture.CreateMany<DateTime>().ToList(), _fixture.CreateMany<float?>().ToList(), _fixture.CreateMany<int?>().ToList());

            Assert.IsType<SortedSet<DataPointWFlag<float, int?>>>(timeSeriesDataWFlag.ToSortedSet());
        }
    }
}
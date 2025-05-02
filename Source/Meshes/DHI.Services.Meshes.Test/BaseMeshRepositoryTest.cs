namespace DHI.Services.Meshes.Test
{
    using System;
    using System.Collections.Generic;
    using TimeSeries;
    using Xunit;

    public class BaseMeshRepositoryTest
    {
        private static readonly TimeSeriesData TimeSeriesDataDense = new(
            new List<DateTime>
            {
                new(2000, 1, 1, 0, 0, 0),
                new(2000, 1, 1, 0, 30, 0),
                new(2000, 1, 1, 1, 0, 0),
                new(2000, 1, 1, 1, 30, 0),
                new(2000, 1, 1, 2, 0, 0),
                new(2000, 1, 1, 2, 30, 0),
                new(2000, 1, 1, 3, 0, 0),
                new(2000, 1, 1, 3, 30, 0),
                new(2000, 1, 1, 4, 0, 0),
                new(2000, 1, 1, 4, 30, 0),
                new(2000, 1, 1, 5, 0, 0)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            });

        private static TimeSeriesDataWFlag<int?> TimeSeriesData => new(
            new List<DateTime>
            {
                new(2000, 1, 1),
                new(2000, 1, 2),
                new(2000, 1, 3),
                new(2000, 1, 4),
                new(2000, 1, 5),
                new(2000, 1, 6),
                new(2000, 1, 7),
                new(2000, 1, 8),
                new(2000, 1, 9),
                new(2000, 1, 10),
                new(2000, 1, 11)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            },
            new List<int?>
            {
                1, 1, 1, 0, 1, 1, null, 0, null, 1, 0
            });

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void GetGroupedValuesThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, period, TimeSeriesData));
        }

        [Fact]
        public void GetGroupedValuesAverageIsOk()
        {
            var yearly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(8.1, yearly.Values[0]);

            var monthly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(8.1, monthly.Values[0]);

            var daily = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Hourly, TimeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5.5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesMaximumIsOk()
        {
            var yearly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(12, yearly.Values[0]);

            var monthly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(12, monthly.Values[0]);

            var daily = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Hourly, TimeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(6, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesMinimumIsOk()
        {
            var yearly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(5, yearly.Values[0]);

            var monthly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(5, monthly.Values[0]);

            var daily = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Hourly, TimeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesSumIsOk()
        {
            var yearly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(81, yearly.Values[0]);

            var monthly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(81, monthly.Values[0]);

            var daily = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Equal(0, daily.Values[6]);

            var hourly = BaseMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Hourly, TimeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(11, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }
    }
}
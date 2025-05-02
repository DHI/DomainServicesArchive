namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class TimeSeriesExtensionsTest
    {
        private static TimeSeriesData TimeSeriesDataDouble => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 2, 0, 0),
                new DateTime(2000, 1, 1, 4, 0, 0),
                new DateTime(2000, 1, 1, 6, 0, 0),
                new DateTime(2000, 1, 1, 10, 0, 0),
                new DateTime(2000, 1, 1, 12, 0, 0),
                new DateTime(2000, 1, 1, 14, 0, 0),
                new DateTime(2000, 1, 1, 16, 0, 0),
                new DateTime(2000, 1, 1, 17, 0, 0),
                new DateTime(2000, 1, 1, 20, 0, 0),
                new DateTime(2000, 1, 1, 22, 0, 0),
                new DateTime(2000, 1, 2, 0, 0, 0),
                new DateTime(2000, 1, 2, 3, 0, 0)
            },
            new List<double?>
            {
                3, 6, 5, 8.4, null, null, 7, null, 10.3, 3, 3, 1
            });

        [Fact]
        public void ContainsSameDataIsOk()
        {
            Assert.True(TimeSeriesDataDouble.ContainsSameData(TimeSeriesDataDouble));

            var differentValues = TimeSeriesDataDouble.MultiplyWith(TimeSeriesDataDouble).data;
            Assert.False(TimeSeriesDataDouble.ContainsSameData(differentValues));

            var differentSizeAndData = new TimeSeriesData(DateTime.Now, 1.123);
            Assert.False(TimeSeriesDataDouble.ContainsSameData(differentSizeAndData));

            var sameNewData = new TimeSeriesData(differentSizeAndData.DateTimes[0], 1.1230);
            Assert.True(differentSizeAndData.ContainsSameData(sameNewData));

            var differentData = new TimeSeriesData(differentSizeAndData.DateTimes[0], 1.124);
            Assert.False(differentSizeAndData.ContainsSameData(differentData));
        }

        [Theory]
        [InlineData("01-01-2000 02:00:00")]
        [InlineData("01-02-2000 03:00:00")]
        public void ContainsDateTimeIsOk(string dateTimeStr)
        {
            Assert.True(TimeSeriesDataDouble.ContainsDateTime(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [InlineData("01-01-2000 01:00:00")]
        [InlineData("01-01-1999")]
        public void DoesNotContainDateTimeIsOk(string dateTimeStr)
        {
            Assert.False(TimeSeriesDataDouble.ContainsDateTime(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [InlineData("01-01-2000 02:00:00")]
        [InlineData("01-01-2000 03:00:00")]
        [InlineData("01-01-2000 02:34:00")]
        [InlineData("01-02-2000 01:14:15")]
        public void CoversDateTimeIsOk(string dateTimeStr)
        {
            Assert.True(TimeSeriesDataDouble.CoversDateTime(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [InlineData("01-01-2000 01:00:00")]
        [InlineData("01-01-1999")]
        public void DoesNotCoverDateTimeIsOk(string dateTimeStr)
        {
            Assert.False(TimeSeriesDataDouble.CoversDateTime(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void GetFirstDateTimeIsOk()
        {
            Assert.Equal(DateTime.Parse("01-01-2000 02:00:00", CultureInfo.InvariantCulture), TimeSeriesDataDouble.GetFirstDateTime().Value);
        }

        [Fact]
        public void GetLastDateTimeIsOk()
        {
            Assert.Equal(DateTime.Parse("01-02-2000 03:00:00", CultureInfo.InvariantCulture), TimeSeriesDataDouble.GetLastDateTime().Value);
        }

        [Fact]
        public void GetFirstIsOk()
        {
            Assert.Equal(3, TimeSeriesDataDouble.GetFirst().Value.Value);
        }

        [Fact]
        public void GetFirstAfterIsOk()
        {
            Assert.Equal(10.3, TimeSeriesDataDouble.GetFirstAfter(DateTime.Parse("01-01-2000 18:00:00", CultureInfo.InvariantCulture)).Value.Value);
        }

        [Fact]
        public void GetFirstAfterReturnsEmptyIfNoDateTimesAfterGivenDateTime()
        {
            Assert.False(TimeSeriesDataDouble.GetFirstAfter(DateTime.Parse("01-01-2020", CultureInfo.InvariantCulture)).HasValue);
        }

        [Fact]
        public void GetLastIsOk()
        {
            Assert.Equal(1, TimeSeriesDataDouble.GetLast().Value.Value);
        }

        [Fact]
        public void GetLastBeforeIsOk()
        {
            Assert.Null(TimeSeriesDataDouble.GetLastBefore(DateTime.Parse("01-01-2000 18:00:00", CultureInfo.InvariantCulture)).Value.Value);
        }

        [Fact]
        public void GetLastBeforeReturnsEmptyIfNoDateTimesBeforeGivenDateTime()
        {
            Assert.False(TimeSeriesDataDouble.GetLastBefore(DateTime.Parse("01-01-1990", CultureInfo.InvariantCulture)).HasValue);
        }

        [Theory]
        [InlineData("01-01-2000 02:00:00", 3.0)]
        [InlineData("01-02-2000 03:00:00", 1.0)]
        [InlineData("01-01-2000 12:00:00", null)]
        public void GetIsOk(string dateTimeStr, double? value)
        {
            Assert.Equal(value, TimeSeriesDataDouble.Get(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture)).Value.Value);
        }

        [Theory]
        [InlineData("01-01-2000 02:00:00", "01-01-2000 02:00:00", 1)]
        [InlineData("01-01-2000 02:30:00", "01-01-2000 02:30:00", 0)]
        [InlineData("01-01-1999", "01-01-2000", 0)]
        [InlineData("01-01-2100", "01-01-2000", 0)]
        [InlineData("01-01-2000 02:00:00", "01-01-2000 10:00:00", 4)]
        [InlineData("01-01-1999", "01-01-2000 10:00:00", 4)]
        public void GetIntervalIsOk(string from, string to, int count)
        {
            var timeSeriesData = TimeSeriesDataDouble.Get(DateTime.Parse(from, CultureInfo.InvariantCulture), DateTime.Parse(to, CultureInfo.InvariantCulture));
            Assert.Equal(count, timeSeriesData.Values.Count);
        }

        [Theory]
        [InlineData("01-01-2000 03:00:00", 4.5)]
        [InlineData("01-02-2000 03:00:00", 1.0)]
        [InlineData("01-01-2000 18:00:00", 17.3 / 2)]
        public void GetInterpolatedIsOk(string dateTimeStr, double value)
        {
            Assert.Equal(value, TimeSeriesDataDouble.GetInterpolated(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture), TimeSeriesDataType.Instantaneous).Value);
        }

        [Theory]
        [InlineData("01-01-2000 03:00:00", 4.5, true)]
        [InlineData("01-02-2000 03:00:00", 1.0, false)]
        [InlineData("01-01-2000 18:00:00", 17.3 / 2, true)]
        public void GetInterpolatedOverloadIsOk(string dateTimeStr, double value, bool isInterpolated)
        {
            var (point, interpolated) = TimeSeriesDataDouble.GetInterpolated(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture), TimeSeriesDataType.Instantaneous, null);
            Assert.Equal(value, point.Value);
            Assert.Equal(isInterpolated, interpolated);
        }

        [Theory]
        [InlineData("01-01-2000 03:00:00", 4.5)]
        [InlineData("01-02-2000 03:00:00", 1.0)]
        [InlineData("01-01-2000 18:00:00", null)]
        public void GetInterpolatedWithGapToleranceIsOk(string dateTimeStr, double? value)
        {
            Assert.Equal(value, TimeSeriesDataDouble.GetInterpolated(DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture), TimeSeriesDataType.Instantaneous, TimeSpan.FromHours(2)).point.Value);
        }

        [Fact]
        public void GetTimeStepsIsOk()
        {
            var otherTimeSeriesData = new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 2, 0, 0),
                new DateTime(2000, 1, 1, 6, 0, 0),
                new DateTime(2000, 1, 1, 8, 0, 0),
                new DateTime(2000, 1, 1, 16, 0, 0),
                new DateTime(2000, 1, 1, 17, 0, 0),
                new DateTime(2000, 1, 1, 23, 0, 0),
                new DateTime(2000, 1, 2, 0, 0, 0)
            },
            new List<double?>
            {
                8, 8, 8, 10, 6, null, 6
            });

            Assert.Equal(14, new List<ITimeSeriesData<double>> { TimeSeriesDataDouble, otherTimeSeriesData }.GetTimeSteps(TimeStepsSelection.All).Length);
            Assert.Equal(5, new List<ITimeSeriesData<double>> { TimeSeriesDataDouble, otherTimeSeriesData }.GetTimeSteps(TimeStepsSelection.CommonOnly).Length);
            Assert.Equal(TimeSeriesDataDouble.DateTimes.Count, new List<ITimeSeriesData<double>> { TimeSeriesDataDouble, otherTimeSeriesData }.GetTimeSteps(TimeStepsSelection.FirstOnly).Length);
        }

        [Theory]
        [InlineData(2.0, 16.8)]
        [InlineData(0.5, 4.2)]
        [InlineData(-0.5, -4.2)]
        public void GetScaledIsOk(double factor, double result)
        {
            var scaled = TimeSeriesDataDouble.GetScaled(factor);
            Assert.Equal(scaled.Values.ToArray()[3], result);
            Assert.Null(scaled.Values.ToArray()[4]);
        }

        [Fact]
        public void GetDoubleValuesIsOk()
        {
            var values = TimeSeriesDataDouble.GetDoubleValues();

            Assert.Equal(TimeSeriesDataDouble.Values.Count, values.Length);
            for (int i = 0; i < TimeSeriesDataDouble.Values.Count; i++)
            {
                var actualValue = TimeSeriesDataDouble.Values[i];
                Assert.Equal(values[i], actualValue ?? double.NaN);
            }
        }

        [Fact]
        public void CopyWithIsOk()
        {
            var timeSeries = new TimeSeries("id", "name", "group")
            {
                DataType = TimeSeriesDataType.Instantaneous,
                Dimension = "Dimension",
                Quantity = "Quantity",
                Unit = "Unit",
            };

            var timeSeriesCopy = timeSeries.CopyWith();

            Assert.True(_PropertyEquals(timeSeries, timeSeriesCopy));
        }

        [Fact]
        public void CopyWithDataIsOk()
        {
            var timeSeries = new TimeSeries("id", "name", "group")
            {
                DataType = TimeSeriesDataType.Instantaneous,
                Dimension = "Dimension",
                Quantity = "Quantity",
                Unit = "Unit"
            };

            var timeSeriesCopy = timeSeries.CopyWith(TimeSeriesDataDouble);

            Assert.True(_PropertyEquals(timeSeries, timeSeriesCopy));
            Assert.Equal(TimeSeriesDataDouble.DateTimes, timeSeriesCopy.Data.DateTimes);
            Assert.Equal(TimeSeriesDataDouble.Values, timeSeriesCopy.Data.Values);
        }

        [Fact]
        public void CopyWithoutDataIsOk()
        {
            var timeSeries = new TimeSeries("id", "name", "group", TimeSeriesDataDouble)
            {
                DataType = TimeSeriesDataType.Instantaneous,
                Dimension = "Dimension",
                Quantity = "Quantity",
                Unit = "Unit",
            };

            var timeSeriesCopy = timeSeries.CopyWith();

            Assert.True(_PropertyEquals(timeSeries, timeSeriesCopy));
            Assert.Empty(timeSeriesCopy.Data.DateTimes);
            Assert.Empty(timeSeriesCopy.Data.Values);
        }

        [Fact]
        public void CopyWithDataPointIsOk()
        {
            var timeSeries = new TimeSeries("id", "name", "group")
            {
                DataType = TimeSeriesDataType.Instantaneous,
                Dimension = "Dimension",
                Quantity = "Quantity",
                Unit = "Unit"
            };

            var dataPoint = new DataPoint(TimeSeriesDataDouble.DateTimes[0], TimeSeriesDataDouble.Values[0]);
            var timeSeriesCopy = timeSeries.CopyWith(dataPoint);

            Assert.True(_PropertyEquals(timeSeries, timeSeriesCopy));
            Assert.Equal(dataPoint.DateTime, timeSeriesCopy.Data.DateTimes[0]);
            Assert.Equal(dataPoint.Value, timeSeriesCopy.Data.Values[0]);
        }

        /// <summary>
        ///     Compares the properties of two time series objects but not their data.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="timeSeries">Existing time series.</param>
        /// <param name="timeSeriesToCompare">Time series to compare to.</param>
        /// <returns>True if all properties are equal except the data.</returns>
        private static bool _PropertyEquals<TValue>(TimeSeries<string, TValue> timeSeries, TimeSeries<string, TValue> timeSeriesToCompare) where TValue : struct
        {
            var areEqual = timeSeries != null;

            if (areEqual)
            {
                areEqual &= timeSeriesToCompare.Id.Equals(timeSeries.Id);
                areEqual &= timeSeriesToCompare.Name.Equals(timeSeries.Name);
                areEqual &= timeSeriesToCompare.Group.Equals(timeSeries.Group);
                areEqual &= timeSeriesToCompare.DataType.Equals(timeSeries.DataType);
                areEqual &= timeSeriesToCompare.Dimension.Equals(timeSeries.Dimension);
                areEqual &= timeSeriesToCompare.Quantity.Equals(timeSeries.Quantity);
                areEqual &= timeSeriesToCompare.Unit.Equals(timeSeries.Unit);
            }

            return areEqual;
        }
    }
}
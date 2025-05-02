namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ResamplePeriodTest
    {
        private static TimeSeriesData TimeSeriesDataDouble => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 2, 1, 1),
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
                3, 6, 5, 8, null, null, 7, null, 10, 3, 3, 1
            });

       

        [Fact]
        public void OkIfInstantaneousAndFirstValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            Assert.Equal(4, timeSeriesData.Resample(Period.Monthly).Count);
        }
        [Fact]
        public void OkIfInstantaneousAndLastValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 4));
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(null);
            Assert.Equal(4, timeSeriesData.Resample(Period.Daily).Count);
        }

        [Fact]
        public void OkIfInstantaneousAndFirstAndLastValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(null);
            Assert.Equal(6, timeSeriesData.Resample(Period.Monthly).Count);
        }

        [Fact]
        public void OkIfNotInstantaneousAndFirstValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            Assert.Equal(25, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.Accumulated).Count);
            Assert.Equal(25, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.StepAccumulated).Count);
            Assert.Equal(25, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.MeanStepForward).Count);
            Assert.Equal(25, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.MeanStepBackward).Count);
        }

        [Fact]
        public void OkIfNotInstantaneousAndLastValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(5.678);
            timeSeriesData.Values.Add(null);
            Assert.Equal(49, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.Accumulated).Count);
            Assert.Equal(49, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.StepAccumulated).Count);
            Assert.Equal(49, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.MeanStepForward).Count);
            Assert.Equal(49, timeSeriesData.Resample(Period.Hourly, TimeSeriesDataType.MeanStepBackward).Count);
        }

        [Fact]
        public void ResampleInstantaneous_Daily_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(Period.Hourly);
            Assert.Equal(26, resampled.Count);
            Assert.Null(resampled.Get(new DateTime(2000, 1, 1, 2, 0, 0)).Value.Value);
            Assert.Equal(1.0, resampled.Get(new DateTime(2000, 1, 2, 3, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value.Value, 4.4, 4.5);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 12, 0, 0)).Value.Value.Value, 7.65, 7.67);
        }

        [Fact]
        public void ResampleInstantaneous_Hourly_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(Period.Hourly);
            Assert.Equal(26, resampled.Count);
            Assert.Null(resampled.Get(new DateTime(2000, 1, 1, 2, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value.Value, 1.65, 1.67);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value.Value, 7.33, 7.34);
            Assert.Equal(8.5, resampled.Get(new DateTime(2000, 1, 1, 18, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepBackward_Monthly_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);

            var resampled = timeSeriesData.Resample(Period.Monthly, TimeSeriesDataType.MeanStepBackward);
            Assert.Equal(6, resampled.Count);
            Assert.Equal(new DateTime(2000, 6, 1, 0, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(2.1, resampled.Get(new DateTime(2000, 1, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(3.14, resampled.Get(new DateTime(2000, 2, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(2.234, resampled.Get(new DateTime(2000, 3, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(9.12, resampled.Get(new DateTime(2000, 4, 1, 0, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepForward_Daily_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);
            var resampled = timeSeriesData.Resample(Period.Daily, TimeSeriesDataType.MeanStepForward);
            Assert.Equal(166, resampled.Count);
            Assert.Equal(new DateTime(2000, 6, 14, 0, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(2.1, resampled.Get(new DateTime(2000, 1, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(1.234, resampled.Get(new DateTime(2000, 1, 2, 0, 0, 0)).Value.Value);
        }
        [Fact]
        public void ResampleMeanStepForward_Weekly_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);
            var resampled = timeSeriesData.Resample(Period.Weekly, TimeSeriesDataType.MeanStepForward);
            Assert.Equal(25, resampled.Count);
            Assert.Equal(new DateTime(2000, 6, 12, 0, 0, 0), resampled.DateTimes.Last());
            Assert.Null(resampled.Get(new DateTime(2000, 1, 1, 0, 0, 0)).Value);
        }
        [Fact]
        public void ResampleMeanStepForward_Quarterly_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 9));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 10));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 7, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 7, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 8, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 9, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 11, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 12, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 12, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);
            var resampled = timeSeriesData.Resample(Period.Quarterly, TimeSeriesDataType.MeanStepForward);
            Assert.Equal(3, resampled.Count);
            Assert.Equal(new DateTime(2000, 4, 1, 0, 0, 0), resampled.DateTimes.First());
            Assert.Null(resampled.Get(new DateTime(2000, 1, 1, 0, 0, 0)).Value);
        }

        [Fact]
        public void ResampleStepAccumulated_Monthly_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 2, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 3, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 5, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 6, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);

            var resampled = timeSeriesData.Resample(Period.Monthly, TimeSeriesDataType.StepAccumulated);
            Assert.Equal(6, resampled.Count);
            Assert.Equal(new DateTime(2000, 7, 1, 0, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(5.568, resampled.Get(new DateTime(2000, 2, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(8.494, resampled.Get(new DateTime(2000, 3, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(8.373999999999999, resampled.Get(new DateTime(2000, 4, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(12.354, resampled.Get(new DateTime(2000, 5, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(8.373999999999999, resampled.Get(new DateTime(2000, 6, 1, 0, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated_Yearly_IsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2001, 2, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2001, 2, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2002, 2, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2002, 3, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2002, 3, 5));
            timeSeriesData.DateTimes.Add(new DateTime(2003, 4, 20));
            timeSeriesData.DateTimes.Add(new DateTime(2003, 4, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2004, 5, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2004, 5, 12));
            timeSeriesData.DateTimes.Add(new DateTime(2005, 6, 4));
            timeSeriesData.DateTimes.Add(new DateTime(2005, 6, 14));
            timeSeriesData.Values.Add(2.1);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.14);
            timeSeriesData.Values.Add(1.12);
            timeSeriesData.Values.Add(4.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(6.14);
            timeSeriesData.Values.Add(9.12);
            timeSeriesData.Values.Add(3.234);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(7.14);
            timeSeriesData.Values.Add(5.12);
            timeSeriesData.Values.Add(2.3);
            var resampled = timeSeriesData.Resample(Period.Yearly, TimeSeriesDataType.StepAccumulated);
            Assert.Equal(6, resampled.Count);
            Assert.Equal(new DateTime(2006, 1, 1, 0, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(4.26, resampled.Get(new DateTime(2002, 1, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(12.354, resampled.Get(new DateTime(2004, 1, 1, 0, 0, 0)).Value.Value);
            Assert.Equal(7.42, resampled.Get(new DateTime(2006, 1, 1, 0, 0, 0)).Value.Value);
        }
    }
}

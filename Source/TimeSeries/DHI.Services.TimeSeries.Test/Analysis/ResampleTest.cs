namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class ResampleTest
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
                3, 6, 5, 8, null, null, 7, null, 10, 3, 3, 1
            });

        [Fact]
        public void TimeSpanIsTooLargeThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Resample(TimeSpan.MaxValue));
        }

        [Fact]
        public void OkIfInstantaneousAndFirstValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 4));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(3.234);
            Assert.Equal(73, timeSeriesData.Resample(TimeSpan.FromHours(1)).Count);
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
            Assert.Equal(73, timeSeriesData.Resample(TimeSpan.FromHours(1)).Count);
        }

        [Fact]
        public void OkIfInstantaneousAndFirstAndLastValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 3));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 4));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(2.234);
            timeSeriesData.Values.Add(null);
            Assert.Equal(73, timeSeriesData.Resample(TimeSpan.FromHours(1)).Count);
        }

        [Fact]
        public void OkIfNotInstantaneousAndFirstValueIsNull()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            Assert.Equal(25, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.Accumulated).Count);
            Assert.Equal(25, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.StepAccumulated).Count);
            Assert.Equal(25, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.MeanStepForward).Count);
            Assert.Equal(25, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.MeanStepBackward).Count);
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
            Assert.Equal(49, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.Accumulated).Count);
            Assert.Equal(49, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.StepAccumulated).Count);
            Assert.Equal(49, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.MeanStepForward).Count);
            Assert.Equal(49, timeSeriesData.Resample(TimeSpan.FromHours(1), TimeSeriesDataType.MeanStepBackward).Count);
        }

        [Fact]
        public void ResampleInstantaneous_1h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(1));
            Assert.Equal(26, resampled.Count);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 1, 2, 0, 0)).Value.Value);
            Assert.Equal(1.0, resampled.Get(new DateTime(2000, 1, 2, 3, 0, 0)).Value.Value);
            Assert.Equal(4.5, resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 12, 0, 0)).Value.Value.Value, 7.65, 7.67);
        }

        [Fact]
        public void ResampleInstantaneous_4h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(4));
            Assert.Equal(7, resampled.Count);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 1, 2, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value.Value, 1.65, 1.67);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value.Value, 7.33, 7.34);
            Assert.Equal(8.5, resampled.Get(new DateTime(2000, 1, 1, 18, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepBackward_3h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(3), TimeSeriesDataType.MeanStepBackward);
            Assert.Equal(9, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 2, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(5.0, resampled.Get(new DateTime(2000, 1, 1, 5, 0, 0)).Value.Value);
            Assert.Equal(8.0, resampled.Get(new DateTime(2000, 1, 1, 8, 0, 0)).Value.Value);
            Assert.Equal(7.0, resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value);
            Assert.Equal(1.0, resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepForward_3h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(3), TimeSeriesDataType.MeanStepForward);
            Assert.Equal(9, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 2, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(6.0, resampled.Get(new DateTime(2000, 1, 1, 5, 0, 0)).Value.Value);
            Assert.Equal(5.0, resampled.Get(new DateTime(2000, 1, 1, 8, 0, 0)).Value.Value);
            Assert.Equal(8.0, resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated_3h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(3), TimeSeriesDataType.StepAccumulated);
            Assert.Equal(9, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 5, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(9.0, resampled.Get(new DateTime(2000, 1, 1, 5, 0, 0)).Value.Value);
            Assert.Equal(5.0, resampled.Get(new DateTime(2000, 1, 1, 8, 0, 0)).Value.Value);
            Assert.Equal(0.0, resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value);
            Assert.Equal(0.0, resampled.Get(new DateTime(2000, 1, 1, 20, 0, 0)).Value.Value);
            Assert.Equal(13.0, resampled.Get(new DateTime(2000, 1, 1, 23, 0, 0)).Value.Value);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated_6h_IsOk()
        {
            var resampled = TimeSeriesDataDouble.Resample(TimeSpan.FromHours(6), TimeSeriesDataType.StepAccumulated);
            Assert.Equal(5, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 8, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(14.0, resampled.Get(new DateTime(2000, 1, 1, 8, 0, 0)).Value.Value);
            Assert.Equal(8.0, resampled.Get(new DateTime(2000, 1, 1, 14, 0, 0)).Value.Value);
            Assert.Equal(16.0, resampled.Get(new DateTime(2000, 1, 2, 2, 0, 0)).Value.Value);
        }
    }
}
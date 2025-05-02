namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class ResampleNiceTimestepsTest
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

        private static TimeSeriesData TimeSeriesDataDoubleDense => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 1, 4, 0),
                new DateTime(2000, 1, 1, 1, 8, 0),
                new DateTime(2000, 1, 1, 1, 12, 0),
                new DateTime(2000, 1, 1, 1, 18, 0),
                new DateTime(2000, 1, 1, 1, 21, 0),
                new DateTime(2000, 1, 1, 1, 22, 0),
                new DateTime(2000, 1, 1, 1, 30, 0),
                new DateTime(2000, 1, 1, 1, 32, 0),
                new DateTime(2000, 1, 1, 1, 33, 0),
                new DateTime(2000, 1, 1, 1, 34, 0),
                new DateTime(2000, 1, 1, 1, 43, 0),
                new DateTime(2000, 1, 1, 1, 47, 0)
            },
            new List<double?>
            {
                3, 6, 5, 8, null, null, 7, null, 10, 3, 3, 1
            });

        [Fact]
        public void TimeSpanIsTooLargeThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.MaxValue));
        }

        [Fact]
        public void FirstValueIsNullThrows()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(1.234);
            Assert.Throws<Exception>(() => timeSeriesData.ResampleNiceTimesteps(TimeSpan.FromHours(1)));
        }

        [Fact]
        public void LastValueIsNullThrows()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 1));
            timeSeriesData.DateTimes.Add(new DateTime(2000, 1, 2));
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(null);
            Assert.Throws<Exception>(() => timeSeriesData.ResampleNiceTimesteps(TimeSpan.FromHours(1)));
        }

        [Fact]
        public void ResampleInstantaneous1HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(1));
            Assert.Equal(26, resampled.Count);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 1, 2, 0, 0)).Value.Value);
            Assert.Equal(1.0, resampled.Get(new DateTime(2000, 1, 2, 3, 0, 0)).Value.Value);
            Assert.Equal(4.5, resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 12, 0, 0)).Value.Value.Value, 7.65, 7.67);
        }

        [Fact]
        public void ResampleInstantaneous15MIsOk()
        {
            var resampled = TimeSeriesDataDoubleDense.ResampleNiceTimesteps(TimeSpan.FromMinutes(15));
            Assert.Equal(3, resampled.Count);
            Assert.Equal(6.5, resampled.Get(new DateTime(2000, 1, 1, 1, 15, 0)).Value.Value);
        }

        [Fact]
        public void ResampleInstantaneousOffset1HIsOk()
        {
            var values = new TimeSeriesData(TimeSeriesDataDouble.DateTimes.Select(r => r.AddHours(0.5)).ToList(), TimeSeriesDataDouble.Values);
            var resampled = values.ResampleNiceTimesteps(TimeSpan.FromHours(1));
            Assert.Equal(25, resampled.Count);
            Assert.Equal(3.75, resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value);
            Assert.Equal(8.125, resampled.Get(new DateTime(2000, 1, 1, 18, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleInstantaneous4HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(4));
            Assert.Equal(6, resampled.Count);
            Assert.Equal(6.0, resampled.Get(new DateTime(2000, 1, 1, 4, 0, 0)).Value.Value);
            Assert.InRange(resampled.Get(new DateTime(2000, 1, 1, 12, 0, 0)).Value.Value.Value, 7.6, 7.7);
            Assert.Equal(10, resampled.Get(new DateTime(2000, 1, 1, 20, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepBackward3HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(3), TimeSeriesDataType.MeanStepBackward);
            Assert.Equal(9, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 3, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(5.0, resampled.Get(new DateTime(2000, 1, 1, 6, 0, 0)).Value.Value);
            Assert.Equal(10.0, resampled.Get(new DateTime(2000, 1, 1, 18, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleMeanStepForward3HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(3), TimeSeriesDataType.MeanStepForward);
            Assert.Equal(9, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 3, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value);
            Assert.Equal(10.0, resampled.Get(new DateTime(2000, 1, 1, 21, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated3HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(3), TimeSeriesDataType.StepAccumulated);
            Assert.Equal(10, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 6, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 1, 3, 0, 0)).Value.Value);
            Assert.Equal(6.0, resampled.Get(new DateTime(2000, 1, 1, 6, 0, 0)).Value.Value);
            Assert.Equal(5.0, resampled.Get(new DateTime(2000, 1, 1, 9, 0, 0)).Value.Value);
            Assert.Equal(0.0, resampled.Get(new DateTime(2000, 1, 1, 15, 0, 0)).Value.Value);
            Assert.Equal(10.0, resampled.Get(new DateTime(2000, 1, 1, 21, 0, 0)).Value.Value);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 2, 0, 0, 0)).Value.Value);
            Assert.Equal(3.0, resampled.Get(new DateTime(2000, 1, 2, 3, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated6HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromHours(6), TimeSeriesDataType.StepAccumulated);
            Assert.Equal(5, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 2, 6, 0, 0), resampled.DateTimes.Last());
            Assert.Equal(9.0, resampled.Get(new DateTime(2000, 1, 1, 6, 0, 0)).Value.Value);
            Assert.Equal(13.0, resampled.Get(new DateTime(2000, 1, 1, 12, 0, 0)).Value.Value);
            Assert.Equal(7.0, resampled.Get(new DateTime(2000, 1, 1, 18, 0, 0)).Value.Value);
            Assert.Equal(13.0, resampled.Get(new DateTime(2000, 1, 2, 0, 0, 0)).Value.Value);
            Assert.Equal(4.0, resampled.Get(new DateTime(2000, 1, 2, 6, 0, 0)).Value.Value);
        }

        [Fact]
        public void ResampleStepAccumulated24HIsOk()
        {
            var resampled = TimeSeriesDataDouble.ResampleNiceTimesteps(TimeSpan.FromDays(1), TimeSeriesDataType.StepAccumulated);
            Assert.Equal(2, resampled.Count);
            Assert.Equal(new DateTime(2000, 1, 3), resampled.DateTimes.Last());
            Assert.Equal(42.0, resampled.Get(new DateTime(2000, 1, 2)).Value.Value);
            Assert.Equal(4.0, resampled.Get(new DateTime(2000, 1, 3)).Value.Value);
        }
    }
}
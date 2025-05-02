namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class ReduceTest
    {
        private static TimeSeriesData TimeSeriesDataDouble => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 0, 0, 0),
                new DateTime(2000, 1, 2, 0, 0, 0),
                new DateTime(2000, 1, 3, 0, 0, 0),
                new DateTime(2000, 1, 4, 0, 0, 0),
                new DateTime(2000, 1, 5, 0, 0, 0),
                new DateTime(2000, 1, 6, 0, 0, 0),
                new DateTime(2000, 1, 10, 0, 0, 0),
                new DateTime(2000, 1, 11, 0, 0, 0),
                new DateTime(2000, 1, 13, 0, 0, 0),
                new DateTime(2000, 1, 15, 0, 0, 0),
                new DateTime(2000, 1, 18, 0, 0, 0),
                new DateTime(2000, 1, 20, 0, 0, 0),
                new DateTime(2000, 1, 22, 0, 0, 0),
                new DateTime(2000, 1, 25, 0, 0, 0),
                new DateTime(2000, 1, 26, 0, 0, 0),
                new DateTime(2000, 1, 27, 0, 0, 0),
                new DateTime(2000, 1, 28, 0, 0, 0),
                new DateTime(2000, 1, 30, 0, 0, 0),
                new DateTime(2000, 1, 31, 0, 0, 0),
            },
            new List<double?>
            {
                0, 1, 0, 2, 1, 3, 2, 4, 5, 7, 5, 3, 1, null, null, 0, 1, 0, 1
            });

        [Fact]
        public void ReturnOrignalIfLessThanMinimumCount()
        {
            var reduced = TimeSeriesDataDouble.Reduce(10, 19);
            Assert.False(reduced.Values.Except(TimeSeriesDataDouble.Values).Any());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void WrongMinimumCountThrows(int minimumCount)
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Reduce(10, minimumCount));
        }

        [Theory]
        [InlineData(120)]
        [InlineData(-1)]
        public void WrongPercentageThrows(double relativeTolerance)
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Reduce(relativeTolerance));
        }

        [Fact]
        public void ReduceIsOk()
        {
            var reduced = TimeSeriesDataDouble.Reduce(10, 3);

            Assert.Equal(14, reduced.Values.Count);
            Assert.True(reduced.ContainsDateTime(new DateTime(2000, 1, 10)));
            Assert.True(reduced.ContainsDateTime(new DateTime(2000, 1, 15)));
            Assert.True(reduced.ContainsDateTime(new DateTime(2000, 1, 22)));
        }

        [Fact]
        public void ReduceBigTimeSeriesDataIsOk()
        {
            var r = new Random();
            var dateTimes = new List<DateTime>();
            var values = new List<double?>();
            for (int i = 0; i < 100000; i++)
            {
                dateTimes.Add(new DateTime(2020, 1, 1).AddMinutes(i));
                values.Add(i / 1000 + 10 * r.NextDouble());
            }

            var data = new TimeSeriesData<double>(dateTimes, values);

            Assert.True(data.Reduce().HasValues);
        }

        [Fact]
        public void DouglasPeuckerIsOk()
        {
            var tolerance = TimeSeriesDataDouble.GetTolerance(10);
            var reducedIndexes = ReduceAnalysis.DouglasPeucker(TimeSeriesDataDouble, tolerance);

            Assert.True(reducedIndexes[0]);
            Assert.True(reducedIndexes[9]);
            Assert.False(reducedIndexes[14]);
        }

        [Fact]
        public void GetToleranceIsOk()
        {
            var tolerance = TimeSeriesDataDouble.GetTolerance(10);
            var tolerance2 = TimeSeriesDataDouble.GetTolerance(2);

            Assert.Equal(0.7, tolerance);
            Assert.Equal(0.14, tolerance2);
        }

        [Fact]
        public void GetBaseLineIsOk()
        {
            var interpolationLine = TimeSeriesDataDouble.GetBaseLine(0, 9);

            Assert.Equal(5.8e-13, interpolationLine.Slope, 1);
            Assert.Equal(0, interpolationLine.StartValue);
            Assert.Equal(TimeSeriesDataDouble.DateTimes[0], interpolationLine.StartTime);
        }

        [Fact]
        public void GetMaxDistanceIsOk()
        {
            var baseLine = TimeSeriesDataDouble.GetBaseLine(0, 9);
            var bitArray = new BitArray(9, true);
            var dmax = TimeSeriesDataDouble.GetMaxDistance(baseLine, bitArray, out _);

            Assert.Equal(2.5, dmax, 1);
        }
    }
}
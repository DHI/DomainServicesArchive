namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class PercentileAnalysisTest
    {
        private readonly Fixture _fixture = new Fixture();

        private static TimeSeriesData TimeSeriesDataDouble => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1),
                new DateTime(2000, 1, 2),
                new DateTime(2000, 1, 3),
                new DateTime(2000, 1, 4),
                new DateTime(2000, 1, 5),
                new DateTime(2000, 1, 6),
                new DateTime(2000, 1, 7),
                new DateTime(2000, 1, 8),
                new DateTime(2000, 1, 9),
                new DateTime(2000, 1, 10),
                new DateTime(2000, 1, 11)
            },

            new List<double?> 
                { 31, 1, 100, 10, 5, 62, null, 26, 37, 46, 10 });

        [Theory]
        [InlineData(120)]
        [InlineData(-1)]
        public void WrongPercentileThrowIsOk(int percentile)
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.PercentileValue(percentile));
        }

        [Fact]
        public void PercentileOfEmptyValuesReturnsNull()
        {
            Assert.Null(new TimeSeriesData().PercentileValue(10));
        }

        [Theory]
        [InlineData(10, 1.2)]
        [InlineData(75, 1.2)]
        public void ConstantDataIsOk(int percentile, double value)
        {
            var dateTimes = _fixture.CreateMany<DateTime>(4).OrderBy(d => d).ToList();
            var values = new List<double?> { 1.2, 1.2, 1.2, 1.2 };
            var timeSeriesData = new TimeSeriesData(dateTimes, values);

            Assert.Equal(value, timeSeriesData.PercentileValue(percentile));
        }

        [Theory]
        [InlineData(10, 1.2)]
        [InlineData(75, 1.2)]
        public void OneDataIsOk(int percentile, double value)
        {
            var dateTimes = _fixture.CreateMany<DateTime>(2).OrderBy(d => d).ToList();
            var values = new List<double?> { 1.2, null };
            var timeSeriesData = new TimeSeriesData(dateTimes, values);

            Assert.Equal(value, timeSeriesData.PercentileValue(percentile));
        }

        [Theory]
        [InlineData(10, 1.0)]
        [InlineData(30, 10.0)]
        [InlineData(50, 26.0)]
        [InlineData(75, 46.0)]
        [InlineData(90, 62.0)]
        public void PercentileValueIsOk(int percentile, double? value)
        {
            var percentileValue = TimeSeriesDataDouble.PercentileValue(percentile);
            Assert.Equal(value, percentileValue);
        }
    }
}
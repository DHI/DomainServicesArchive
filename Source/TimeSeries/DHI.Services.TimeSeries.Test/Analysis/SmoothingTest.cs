namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class DataSmoothTest
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
        public void WrongWindowLengthThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(10));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(2));
        }

        [Fact]
        public void WrongOrderThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(9, 6));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(9, -1));
        }

        [Fact]
        public void WindowLengthSmallerThanOrderThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(5, 4));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataDouble.Smoothing(5, 5));
        }

        [Theory]
        [InlineData(3, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 2)]
        [InlineData(7, 3)]
        [InlineData(7, 4)]
        [InlineData(9, 5)]
        public void CalculateCoefficientIsOk(int windowLength, int order)
        {
            var coefficients = SmoothingAnalysis.CalculateCoefficients(windowLength, order);
            var sumCoefficients = Convert.ToDouble(coefficients.Sum());

            Assert.Equal(coefficients.Length, windowLength);
            Assert.Equal(1, sumCoefficients, 1);
        }

        [Fact]
        public void SmoothingIsOk()
        {
            var smoothed = TimeSeriesDataDouble.Smoothing(5);

            Assert.False(smoothed.Values.Contains(null));
            Assert.Equal(smoothed.Count, TimeSeriesDataDouble.Count);
        }

        [Fact]
        public void GapFillIsOk()
        {
            var filled = TimeSeriesDataDouble.GapFill();

            Assert.False(filled.Values.Contains(null));
            Assert.Equal(filled.Count, TimeSeriesDataDouble.Count);
            Assert.Equal(0.4, filled.Values[13].Value, 1);
        }

        [Fact]
        public void SavitskyGolayIsOk()
        {
            var input = new double[]{0, 1, 0, 2, 1, 3, 2, 4, 5, 7, 5, 3, 1, 0, 1, 0, 1};
            var smoothedValues = input.SavitskyGolay(5);

            Assert.Equal(5.6, smoothedValues[8], 1);
            Assert.Equal(0.6, smoothedValues[15], 1);
        }
    }
}

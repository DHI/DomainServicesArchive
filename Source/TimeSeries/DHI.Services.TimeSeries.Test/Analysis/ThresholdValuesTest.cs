namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Linq;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class ThresholdValuesTest
    {
        [Fact]
        public void IllegalArgumentsThrows()
        {
            Assert.Throws<ArgumentException>(() => new ThresholdValues(99.9, 88.8));
        }

        [Fact]
        public void AutomaticGenerationIsOk()
        {
            var values = new ThresholdValues(10, 19);
            Assert.Equal(10, values.Count);
            Assert.Equal(10, values.Min);
            Assert.Equal(19, values.Max);
        }

        [Fact]
        public void ManualGenerationIsOk()
        {
            var values = new ThresholdValues { 3.3, -123.45, 22, 23, 22 };
            Assert.Equal(4, values.Count);
            Assert.Equal(-123.45, values.Min);
            Assert.Equal(23, values.Max);
        }

        [Fact]
        public void IntervalsGenerationFromAutomaticIsOk()
        {
            var intervals = new ThresholdValues(10, 19.5, 20).Intervals.ToList();

            Assert.Equal(21, intervals.Count);
            Assert.Equal(double.MinValue, intervals.First().Start);
            Assert.Equal(double.MaxValue, intervals.Last().End);
        }

        [Fact]
        public void IntervalsGenerationFromManualIsOk()
        {
            var intervals = new ThresholdValues { 3.3, -123.45, 22, 23, 22 }.Intervals.ToList();

            Assert.Equal(5, intervals.Count);
            Assert.Equal(double.MinValue, intervals.First().Start);
            Assert.Equal(double.MaxValue, intervals.Last().End);
        }

        [Fact]
        public void IntervalsFromOneValueIsOk()
        {
            var intervals = new ThresholdValues { 3.3 }.Intervals.ToList();

            Assert.Equal(2, intervals.Count);
            Assert.Equal(double.MinValue, intervals.First().Start);
            Assert.Equal(double.MaxValue, intervals.Last().End);
        }

        [Fact]
        public void IntervalsFromEmptyIsOk()
        {
            var intervals = new ThresholdValues().Intervals;
            Assert.Empty(intervals);
        }

        [Fact]
        public void GetIntervalForNullValueReturnsNull()
        {
            var thresholdValues = new ThresholdValues(88, 99);
            Assert.Null(thresholdValues.GetInterval(null));
        }

        [Fact]
        public void GetIntervalFromEmptyThrows()
        {
            var thresholdValues = new ThresholdValues();
            Assert.Throws<ArgumentOutOfRangeException>(() => thresholdValues.GetInterval(99));
        }

        [Fact]
        public void GetIntervalIsOk()
        {
            var thresholdValues = new ThresholdValues(10, 19.5, 20);

            var interval = thresholdValues.GetInterval(14.3);
            Assert.Equal(14, interval.Start);
            Assert.Equal(14.5, interval.End);
        }
    }
}
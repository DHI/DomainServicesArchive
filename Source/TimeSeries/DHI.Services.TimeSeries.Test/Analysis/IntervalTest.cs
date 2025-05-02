namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class IntervalTest
    {
        [Fact]
        public void IllegalArgumentsThrows()
        {
            Assert.Throws<ArgumentException>(() => new Interval<float>(99.9f, 88.8f));
        }

        [Fact]
        public void ContainsIsOk()
        {
            var interval = new Interval<double>(1, 2);
            Assert.True(interval.Contains(1));
            Assert.True(interval.Contains(1.5));
            Assert.False(interval.Contains(2));
            Assert.False(interval.Contains(5));
        }
    }
}
namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using Xunit;

    public class GapFillTest
    {
        [Fact]
        public void GapFillInIntervalThrowsIfIllegalInterval()
        {
            Assert.Throws<ArgumentException>(() => TestData.TimeSeriesData.GapFill(DateTime.MaxValue, DateTime.MinValue, TimeSpan.FromDays(1), null));
        }

        [Fact]
        public void GapFillInIntervalIsOk()
        {
            var (filledSeries, skippedCount, insertedCount) = TestData.TimeSeriesData.GapFill(new DateTime(2000, 1, 6), new DateTime(2000, 1, 8), TimeSpan.FromHours(6), 999);
            Assert.Equal(2, skippedCount);
            Assert.Equal(6, insertedCount);
            Assert.Equal(17, filledSeries.DateTimes.Count);
            Assert.Equal(999, filledSeries.Get(new DateTime(2000, 1, 7, 18, 0, 0)).Value.Value);
        }
    }
}
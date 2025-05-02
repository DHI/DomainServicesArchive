namespace DHI.Services.TimeSeries.Test.Analysis
{
    using Xunit;

    public class TimeStepTrendTest
    {
        [Fact]
        public void TimeStepTrendIsOk()
        {
            var forwardTrend = TestData.TimeSeriesDataDense.TimeStepTrend(TimeStepTrendType.Forward);
            Assert.Equal(-1, forwardTrend.Values[3].Value);
            Assert.Equal(TestData.TimeSeriesDataDense.Count, forwardTrend.Count);
            var backwardTrend = TestData.TimeSeriesDataDense.TimeStepTrend(TimeStepTrendType.Backwards);
            Assert.Equal(-3, backwardTrend.Values[3].Value);
        }
    }
}
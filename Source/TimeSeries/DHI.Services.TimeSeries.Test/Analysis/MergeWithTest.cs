namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System.Linq;
    using Xunit;

    public class MergeWithTest
    {
        [Fact]
        public void MergeWithFlagsIsOk()
        {
            var other = new TimeSeriesDataWFlag<int?>(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5)).ToList(), TestData.TimeSeriesData.Values, TestData.TimeSeriesData.Flags);
            var (mergedData, overwriteCount, appendCount) = TestData.TimeSeriesData.MergeWith(other);
            Assert.Equal(16, mergedData.DateTimes.Count);
            Assert.Equal(6, overwriteCount);
            Assert.Equal(5, appendCount);
            Assert.Null(mergedData.Values[6]);
            Assert.Null(mergedData.Flags[8]);
        }

        [Fact]
        public void MergeWithIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5)).ToList(), TestData.TimeSeriesData.Values);
            var (mergedData, overwriteCount, appendCount) = TestData.TimeSeriesData.MergeWith(other);
            Assert.Equal(16, mergedData.DateTimes.Count);
            Assert.Equal(6, overwriteCount);
            Assert.Equal(5, appendCount);
            Assert.Null(mergedData.Values[6]);
        }
    }
}
namespace DHI.Services.TimeSeries.Test
{
    using System.Collections.Generic;
    using AutoFixture.Xunit2;
    using Xunit;

    public class SortedSetExtensionsTest
    {
        [Theory]
        [AutoData]
        public void ToTimeSeriesDataIsOk(SortedSet<DataPoint<float>> sortedSet)
        {
            var timeSeriesData = sortedSet.ToTimeSeriesData();

            Assert.Equal(sortedSet.Count, timeSeriesData.DateTimes.Count);
            Assert.Equal(sortedSet.Count, timeSeriesData.Values.Count);
        }

        [Theory]
        [AutoData]
        public void ToTimeSeriesDataWFlagIsOk(SortedSet<DataPointWFlag<float, int>> sortedSet)
        {
            var timeSeriesDataWFlag = sortedSet.ToTimeSeriesDataWFlag();

            Assert.Equal(sortedSet.Count, timeSeriesDataWFlag.DateTimes.Count);
            Assert.Equal(sortedSet.Count, timeSeriesDataWFlag.Values.Count);
            Assert.Equal(sortedSet.Count, timeSeriesDataWFlag.Flags.Count);
        }
    }
}
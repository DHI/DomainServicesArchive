namespace DHI.Services.TimeSeries.Test
{
    using DHI.Services.TimeSeries;
    using Xunit;

    public class TimeSeriesTest
    {
        [Fact]
        public void FullNameIsOk()
        {
            var timeSeries = new TimeSeries("id", "name", "group");
            Assert.Equal($"{timeSeries.Group}/{timeSeries.Name}", timeSeries.FullName);
        }

        [Fact]
        public void FullNameIsNameOnlyIfNoGroup()
        {
            var timeSeries = new TimeSeries("ts1", "Ts 1", null);
            Assert.Equal(timeSeries.Name, timeSeries.FullName);
        }

        [Theory, AutoTimeSeriesData]
        public void CloneIsOk(ITimeSeriesData<float> timeSeriesData)
        {
            var timeSeries = new TimeSeries<string, float>("id", "name", "group", timeSeriesData);
            var clone = timeSeries.Clone();

            Assert.Equal(timeSeries.Id, clone.Id);
            Assert.Equal(timeSeries.DataType, clone.DataType);
            Assert.Equal(timeSeries.Dimension, clone.Dimension);
            Assert.Equal(timeSeries.Quantity, clone.Quantity);
            Assert.Equal(timeSeries.Unit, clone.Unit);
            Assert.Equal(timeSeries.Data.DateTimes, clone.Data.DateTimes);
            Assert.Equal(timeSeries.Data.Values, clone.Data.Values);
            Assert.Equal($"{timeSeries.Group}/{timeSeries.Name}", clone.FullName);
        }

        [Theory, AutoTimeSeriesData]
        public void HasValuesIsOk(ITimeSeriesData<float> timeSeriesData)
        {
            var timeSeries = new TimeSeries<string, float>("id", "name", "group");
            Assert.False(timeSeries.HasValues);

            timeSeries = new TimeSeries<string, float>("id", "name", "group", timeSeriesData);
            Assert.True(timeSeries.HasValues);
        }
    }
}
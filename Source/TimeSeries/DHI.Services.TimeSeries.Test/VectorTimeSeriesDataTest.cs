namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using AutoFixture.Xunit2;
    using Xunit;

    public class VectorTimeSeriesDataTest
    {
        [Theory, AutoData]
        public void CreateWithIncompatibleTimeSeriesDataThrows(List<DateTime> xDateTimes, List<double?> xValues, List<DateTime> yDateTimes, List<double?> yValues)
        {
            var xData = new TimeSeriesData(xDateTimes, xValues);
            var yData = new TimeSeriesData(yDateTimes, yValues);

            Assert.Throws<ArgumentException>(() => new VectorTimeSeriesData(xData, yData));
        }

        [Theory, AutoData]
        public void CreateIsOk(List<DateTime> dateTimes, List<double?> xValues, List<double?> yValues)
        {
            var xData = new TimeSeriesData(dateTimes, xValues);
            var yData = new TimeSeriesData(dateTimes, yValues);

           var vectorTimeSeriesData = new VectorTimeSeriesData(xData, yData);
           Assert.IsType<DataPoint<Vector<double>>>(vectorTimeSeriesData.Get(dateTimes[0]).Value);
        }

        [Theory, AutoData]
        public void NullValueResultsInNullVector(List<DateTime> dateTimes)
        {
            var xData = new TimeSeriesData(dateTimes, new List<double?> {null, 999.9, null} );
            var yData = new TimeSeriesData(dateTimes, new List<double?> { 999.9, 999.9, null });

            var vectorTimeSeriesData = new VectorTimeSeriesData(xData, yData);
            Assert.Null(vectorTimeSeriesData.Get(dateTimes[0]).Value.Value);
            Assert.False(vectorTimeSeriesData.Get(dateTimes[1]).Value.Value is null);
            Assert.Null(vectorTimeSeriesData.Get(dateTimes[2]).Value.Value);
        }
    }
}
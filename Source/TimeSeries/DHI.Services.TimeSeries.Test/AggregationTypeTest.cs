namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class AggregationTypeTest
    {
        [Fact]
        public void GetValueForEmptyTimeSeriesDataIsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            Assert.Equal(0, AggregationType.Average.GetValue(timeSeriesData));
            Assert.Null(AggregationType.Maximum.GetValue(timeSeriesData));
            Assert.Null(AggregationType.Minimum.GetValue(timeSeriesData));
            Assert.Equal(0, AggregationType.Sum.GetValue(timeSeriesData));
        }

        [Fact]
        public void GetDoubleValueForEmptyValuesIsOk()
        {
            var values = Enumerable.Empty<double?>().ToArray();
            Assert.Null(AggregationType.Average.GetValue(values));
            Assert.Null(AggregationType.Maximum.GetValue(values));
            Assert.Null(AggregationType.Minimum.GetValue(values));
            Assert.Equal(0, AggregationType.Sum.GetValue(values));
        }

        [Fact]
        public void GetFloatValueForEmptyValuesIsOk()
        {
            var values = Enumerable.Empty<float?>().ToArray();
            Assert.Null(AggregationType.Average.GetValue(values));
            Assert.Null(AggregationType.Maximum.GetValue(values));
            Assert.Null(AggregationType.Minimum.GetValue(values));
            Assert.Equal(0, AggregationType.Sum.GetValue(values));
        }

        [Fact]
        public void GetValueForCorruptTimeSeriesDataIsOk()
        {
            var timeSeriesData = new TimeSeriesData(DateTime.Now, null);

            Assert.Equal(0, AggregationType.Average.GetValue(timeSeriesData));
            Assert.Null(AggregationType.Maximum.GetValue(timeSeriesData));
            Assert.Null(AggregationType.Minimum.GetValue(timeSeriesData));
            Assert.Equal(0, AggregationType.Sum.GetValue(timeSeriesData));
        }

        [Fact]
        public void GetDoubleValueForCorruptValuesIsOk()
        {
            var values = new double?[] {null, null};

            Assert.Null(AggregationType.Average.GetValue(values));
            Assert.Null(AggregationType.Maximum.GetValue(values));
            Assert.Null(AggregationType.Minimum.GetValue(values));
            Assert.Equal(0, AggregationType.Sum.GetValue(values));
        }

        [Fact]
        public void GetFloatValueForCorruptValuesIsOk()
        {
            var values = new float?[] { null, null };

            Assert.Null(AggregationType.Average.GetValue(values));
            Assert.Null(AggregationType.Maximum.GetValue(values));
            Assert.Null(AggregationType.Minimum.GetValue(values));
            Assert.Equal(0, AggregationType.Sum.GetValue(values));
        }

        [Fact]
        public void GetValueForTimeSeriesDataIsOk()
        {
            var timeSeriesData = new TimeSeriesData(
                new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 4),
                    new DateTime(2015, 1, 6),
                    new DateTime(2015, 1, 9)
                },
                new List<double?>
                {
                    11.2, null, 22.4, -3
                });

            Assert.Equal(10.2, (double)AggregationType.Average.GetValue(timeSeriesData), 6);
            Assert.Equal(22.4, AggregationType.Maximum.GetValue(timeSeriesData));
            Assert.Equal(-3, AggregationType.Minimum.GetValue(timeSeriesData));
            Assert.Equal(30.6, (double)AggregationType.Sum.GetValue(timeSeriesData), 6);
        }

        [Fact]
        public void GetDoubleValueIsOk()
        {
            var data = new List<double?> {11.2, null, 22.4, -3 };

            Assert.Equal(10.2, (double)AggregationType.Average.GetValue(data), 6);
            Assert.Equal(22.4, AggregationType.Maximum.GetValue(data));
            Assert.Equal(-3, AggregationType.Minimum.GetValue(data));
            Assert.Equal(30.6, (double)AggregationType.Sum.GetValue(data), 6);
        }

        [Fact]
        public void GetFloatValueIsOk()
        {
            var data = new List<float?> { 11.2f, null, 22.4f, -3 };

            Assert.Equal(10.2f, (float)AggregationType.Average.GetValue(data), 5);
            Assert.Equal(22.4f, AggregationType.Maximum.GetValue(data));
            Assert.Equal(-3, AggregationType.Minimum.GetValue(data));
            Assert.Equal(30.6f, (float)AggregationType.Sum.GetValue(data), 5);
        }
    }
}

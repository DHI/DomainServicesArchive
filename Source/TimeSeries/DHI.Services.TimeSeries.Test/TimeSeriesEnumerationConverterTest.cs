namespace DHI.Services.TimeSeries.Test
{
    using System.Text.Json;
    using DHI.Services.Converters;
    using Xunit;

    public class TimeSeriesEnumerationConverterTest
    {
        private class ModelAgg
        {
            public ModelAgg()
            {

            }
            public ModelAgg(TimeSeriesData data)
            {
                if (data != null)
                {
                    AggregateMin = AggregationType.Minimum.GetValue(data);
                    AggregateMax = AggregationType.Maximum.GetValue(data);
                    AggregateSum = AggregationType.Sum.GetValue(data);
                }
            }

            public double? AggregateMin { get; set; }
            public double? AggregateMax { get; set; }
            public double? AggregateSum { get; set; }
        }

        [Fact]
        public void AggreationTypeEnumerationDeserialize()
        {
            var json = "{\"AggregateMin\":10,\"AggregateMax\":50,\"AggregateSum\":80}";
            var opt = new JsonSerializerOptions();
            opt.Converters.Add(new EnumerationConverter());

            var model = JsonSerializer.Deserialize<ModelAgg>(json, opt);

            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.Values.Add(10);
            timeSeriesData.Values.Add(20);
            timeSeriesData.Values.Add(50);
            var expected = new ModelAgg(timeSeriesData);

            Assert.Equal(expected.AggregateMin, model.AggregateMin);
            Assert.Equal(expected.AggregateMax, model.AggregateMax);
            Assert.Equal(expected.AggregateSum, model.AggregateSum);
        }

        [Fact]
        public void AggreationTypeEnumerationSerialize()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.Values.Add(10);
            timeSeriesData.Values.Add(20);
            timeSeriesData.Values.Add(50);
            var model = new
            {
                AggregateMin = AggregationType.Minimum.GetValue(timeSeriesData),
                AggregateMax = AggregationType.Maximum
            };
            var opt = new JsonSerializerOptions();
            opt.Converters.Add(new EnumerationConverter());
            var actual = JsonSerializer.Serialize(model, opt);

            var expected = "{\"AggregateMin\":10,\"AggregateMax\":\"Maximum\"}";
            Assert.Equal(expected, actual);
        }
    }
}
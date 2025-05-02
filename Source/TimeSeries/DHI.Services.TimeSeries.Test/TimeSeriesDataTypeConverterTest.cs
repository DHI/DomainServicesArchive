namespace DHI.Services.TimeSeries.Test
{
    using System.Text.Json;
    using Xunit;

    public class TimeSeriesDataTypeConverterTest : IClassFixture<TimeSeriesDataTypeConverterFixture>
    {
        public TimeSeriesDataTypeConverterTest(TimeSeriesDataTypeConverterFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly TimeSeriesDataTypeConverterFixture _fixture;

        [Fact]
        public void CanConvertIsOk()
        {
            Assert.True(_fixture.Converter.CanConvert(TimeSeriesDataType.Instantaneous.GetType()));
        }

        [Fact]
        public void ReadJsonIsOk()
        {
            const string jsonStr = "{\"DisplayName\":\"Step Accumulated\",\"Value\":2}";
            var timeSeriesDataType = JsonSerializer.Deserialize<TimeSeriesDataType>(jsonStr, _fixture.Settings);
            Assert.Equal("Step Accumulated", timeSeriesDataType.DisplayName);
            Assert.IsAssignableFrom<TimeSeriesDataType>(timeSeriesDataType);
        }


        [Fact]
        public void WriteJsonIsOk()
        {
            var fake = new
            {
                InstantType = TimeSeriesDataType.Instantaneous,
                AccumulatedType = TimeSeriesDataType.Accumulated,
            };
            var accumulatedType = TimeSeriesDataType.Accumulated;
            var json = JsonSerializer.Serialize(fake, _fixture.Settings);
            Assert.Equal("{\"instantType\":\"Instantaneous\",\"accumulatedType\":\"Accumulated\"}", json);
        }
    }
}
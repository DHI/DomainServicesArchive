namespace DHI.Services.TimeSeries.Test
{
    using System.Linq;
    using System.Text.Json;
    using Converters;
    using Xunit;

        
    public class TimeSeriesDataConverterTest
    {

        //[Fact]
        //public void ReadDoubleIsOk()
        //{
        //    var converter = new TimeSeriesDataConverter<double>();
        //    const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";

        //    var timeseries = JsonConvert.DeserializeObject<TimeSeriesData<double>>(jsonString, converter);
        //    Assert.NotEmpty(timeseries.Values);
        //    Assert.Equal(217.034988, timeseries.Values[0]);
        //    Assert.Equal("2012-10-30T11:50:00", timeseries.DateTimes.Last().ToString("s"));
        //}

        [Fact]
        public void ReadDoubleIsOk()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var timeseries = JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Values);
            Assert.Equal(217.034988, timeseries.Values[0]);
            Assert.Equal("2012-10-30T11:50:00", timeseries.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadVectorIsOk()
        {
            var converter = new TimeSeriesDataConverter<Vector<double>>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",{ \"X\":5.4,\"Y\":3.6,\"Size\": 4,\"Direction\":90}]]";

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var timeseries = JsonSerializer.Deserialize<TimeSeriesData<Vector<double>>>(jsonString, options);
            Assert.Equal(typeof(Vector<double>), timeseries.Values[0].GetType());
            Assert.Equal(5.4, ((Vector<double>)timeseries.Values[0]).X);
            Assert.Equal("2012-10-30T11:10:00", timeseries.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadDataIsOk()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""DateTimes"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""Values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var timeseries = JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Values);
            Assert.Equal(217.034988, timeseries.Values[0]);
            Assert.Equal("2012-10-30T11:50:00", timeseries.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadDataCamelCaseIsOk()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""dateTimes"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(converter);

            var timeseries = JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Values);
            Assert.Equal(217.034988, timeseries.Values[0]);
            Assert.Equal("2012-10-30T11:50:00", timeseries.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadDataCaseInsensitiveIsOk()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""datetimes"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(converter);

            var timeseries = JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Values);
            Assert.Equal(217.034988, timeseries.Values[0]);
            Assert.Equal("2012-10-30T11:50:00", timeseries.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadDataInvalidPropertyCamelCaseThrows()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""DateTimes"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""Values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(converter);

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options));
        }

        [Fact]
        public void ReadDataInvalidPropertyPascalCaseThrows()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""dateTimes"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options));
        }

        [Fact]
        public void ReadDataInvalidPropertyNameCaseInsensitiveThrows()
        {
            var converter = new TimeSeriesDataConverter<double>();
            const string jsonString = @"
                {
                    ""DateTimesMistake"": [""2012-10-30T11:10:00"", ""2012-10-30T11:20:00"", ""2012-10-30T11:30:00"", ""2012-10-30T11:40:00"", ""2012-10-30T11:50:00""],
                    ""Values"": [217.034988, 217.025, 217.025, 217.015, 217.015]
                }
            ";
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(converter);

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options));
        }

        //[Fact]
        //public void ReadVectorIsOk()
        //{
        //    var converter = new TimeSeriesDataConverter<Vector<double>>();
        //    const string jsonString = "[[\"2012-10-30T11:10:00\",{ \"X\":5.4,\"Y\":3.6,\"Size\": 4,\"Direction\":90}]]";

        //    var timeseries = JsonConvert.DeserializeObject<TimeSeriesData<Vector<double>>>(jsonString, converter);
        //    Assert.Equal(typeof(Vector<double>), timeseries.Values[0].GetType());
        //    Assert.Equal(5.4, ((Vector<double>)timeseries.Values[0]).X);
        //    Assert.Equal("2012-10-30T11:10:00", timeseries.DateTimes.Last().ToString("s"));
        //}
    }
}
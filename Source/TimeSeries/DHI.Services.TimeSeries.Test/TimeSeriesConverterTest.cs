namespace DHI.Services.TimeSeries.Test
{
    using Converters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    public class TimeSeriesConverterTest
    {
        [Fact]
        public void ReadTimeSeriesPascalCaseIsOk()
        {
            const string jsonString = @"
                {
                    ""Id"": ""/Temp/Demo Time Series 1"",
                    ""FullName"": ""/Temp/Demo Time Series 1"",
                    ""Name"": ""Demo Time Series 1"",
                    ""DataType"": ""Instantaneous"",
                    ""Dimension"": """",
                    ""Quantity"": ""Water Level"",
                    ""Unit"": ""m"",
                    ""Data"": {
                        ""DateTimes"": [""2024-01-01T00:00:00"", ""2024-01-02T00:00:00""],
                        ""Values"": [10.02, 11.03]
                    }
               }
            ";

            var options = new JsonSerializerOptions();
            options.Converters.Add(new TimeSeriesConverter<string, double>());
            options.Converters.Add(new TimeSeriesDataConverter<double>());
            options.Converters.Add(new TimeSeriesDataTypeConverter());

            var timeseries = JsonSerializer.Deserialize<TimeSeries<string, double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Data.DateTimes);
            Assert.Equal(10.02, timeseries.Data.Values[0]);
            Assert.Equal("2024-01-02T00:00:00", timeseries.Data.DateTimes.Last().ToString("s"));
        }

        [Fact]
        public void ReadTimeSeriesCamelCaseIsOk()
        {
            const string jsonString = @"
                {
                    ""id"": ""/Temp/Demo Time Series 1"",
                    ""fullName"": ""/Temp/Demo Time Series 1"",
                    ""name"": ""Demo Time Series 1"",
                    ""dataType"": ""Instantaneous"",
                    ""dimension"": """",
                    ""quantity"": ""Water Level"",
                    ""unit"": ""m"",
                    ""data"": {
                        ""dateTimes"": [""2024-01-01T00:00:00"", ""2024-01-02T00:00:00""],
                        ""values"": [10.02, 11.03]
                    }
               }
            ";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(new TimeSeriesConverter<string, double>());
            options.Converters.Add(new TimeSeriesDataConverter<double>());
            options.Converters.Add(new TimeSeriesDataTypeConverter());

            var timeseries = JsonSerializer.Deserialize<TimeSeries<string, double>>(jsonString, options);
            Assert.NotEmpty(timeseries.Data.DateTimes);
            Assert.Equal(10.02, timeseries.Data.Values[0]);
            Assert.Equal("2024-01-02T00:00:00", timeseries.Data.DateTimes.Last().ToString("s"));
        }
    }
}

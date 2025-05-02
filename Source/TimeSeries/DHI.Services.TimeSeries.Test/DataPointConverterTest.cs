namespace DHI.Services.TimeSeries.Test
{

    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Converters;
    using Xunit;

    public class DataPointConverterTest
    {
        [Fact]
        public void ConverterOnWrongStructureThrows()
        {
            var converter = new DataPointConverter<double, bool>();
            const string jsonString = "{\"Type\":\"Single\", \"Value\":1.4}";

            //var e = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converter));
            //Assert.Contains("Cannot deserialize the current JSON object", e.Message);
        }

        [Fact]
        public void ReadDataPointWFlagsWithWrongConverterThrows()
        {
            var converter = new DataPointConverter<double>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988, true],[\"2012-10-30T11:20:00\",217.025, true],[\"2012-10-30T11:30:00\",217.025, true],[\"2012-10-30T11:40:00\",217.015, true],[\"2012-10-30T11:50:00\",217.015, false]]";

            //var e = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<List<DataPointWFlag<bool>>>(jsonString, converter));
            //Assert.Contains("Cannot deserialize the current JSON array", e.Message);
        }

        [Fact]
        public void ReadDataPointSingleWithSimpleConverterIsOk()
        {
            var converter = new DataPointConverter<double>();
            var jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var timeseries = JsonSerializer.Deserialize< List<DataPoint<double>>>(jsonString, options);
            //var timeseries = JsonSerializer.Deserialize<TimeSeriesData<double>>(jsonString, options);
            //var converter = new DataPointConverter<double>();
            //const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";

            //var timeseries = JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converter);

            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }

        [Fact]
        public void ReadDataPointSingleWithFullConverterIsOk()
        {
            var converter = new DataPointConverter<double, bool>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";
           
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var timeseries = JsonSerializer.Deserialize<List<DataPoint<double>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converter);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converter);
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }

        [Fact]
        public void ReadDoubleArrayIsOk()
        {
            var converter = new DataPointConverter<double, bool>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var timeseries = JsonSerializer.Deserialize<List<DataPoint<double>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converter);
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }

        [Fact]
        public void ReadDoubleTwoConvertersIsOk()
        {
            var converterFull = new DataPointConverter<double, bool>();
            var converterSimple = new DataPointConverter<double>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988],[\"2012-10-30T11:20:00\",217.025],[\"2012-10-30T11:30:00\",217.025],[\"2012-10-30T11:40:00\",217.015],[\"2012-10-30T11:50:00\",217.015]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converterFull);
            options.Converters.Add(converterSimple);
            var timeseries = JsonSerializer.Deserialize<List<DataPoint<double>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPoint<double>>>(jsonString, converterFull, converterSimple);
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }

        [Fact]
        public void ReadDoubleWFlagIsOk()
        {
            //var converter = new DataPointConverter<double, bool>(); //old
            var converter = new DataPointWFlagConverter<double, bool>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988, true],[\"2012-10-30T11:20:00\",217.025, true],[\"2012-10-30T11:30:00\",217.025, true],[\"2012-10-30T11:40:00\",217.015, true],[\"2012-10-30T11:50:00\",217.015, false]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var timeseries = JsonSerializer.Deserialize<List<DataPointWFlag<double, bool>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPointWFlag<double, bool>>>(jsonString, converter);//old
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.True(timeseries[0].Flag);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
            Assert.False(timeseries.Last().Flag);
        }

        [Fact]
        public void ReadDoubleWFlagWithTwoConvertersIsOk()
        {
            //var converterFull = new DataPointConverter<double, bool>();//old
            //var converterSimple = new DataPointConverter<double>();//old
            var converterFull = new DataPointWFlagConverter<double, bool>();
            var converterSimple = new DataPointWFlagConverter<double>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988, true],[\"2012-10-30T11:20:00\",217.025, true],[\"2012-10-30T11:30:00\",217.025, true],[\"2012-10-30T11:40:00\",217.015, true],[\"2012-10-30T11:50:00\",217.015, false]]";

            var options = new JsonSerializerOptions();
            options.Converters.Add(converterFull);
            options.Converters.Add(converterSimple);
            var timeseries = JsonSerializer.Deserialize<List<DataPointWFlag<double, bool>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPointWFlag<double, bool>>>(jsonString, converterFull, converterSimple);
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.True(timeseries[0].Flag);
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
            Assert.False(timeseries.Last().Flag);
        }

        [Fact]
        public void ReadDoubleForecastedIsOk()
        {
            //var converter = new DataPointConverter<double, bool>(); //old
            var converter = new DataPointForecastedConverter<double, bool>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988, \"2012-10-29T11:10:00\"],[\"2012-10-30T11:20:00\",217.025, \"2012-10-29T11:20:00\"],[\"2012-10-30T11:30:00\",217.025, \"2012-10-29T11:30:00\"],[\"2012-10-30T11:40:00\",217.015, \"2012-10-29T11:40:00\"],[\"2012-10-30T11:50:00\",217.015, \"2012-10-29T11:50:00\"]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var timeseries = JsonSerializer.Deserialize<List<DataPointForecasted<double>>>(jsonString, options);
            //var timeseries = JsonSerializer.Deserialize<List<DataPointForecasted>>(jsonString, options);//old
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-29T11:10:00", timeseries[0].TimeOfForecast.ToString("s"));
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }

        [Fact]
        public void ReadDoubleForecastedWithTwoConvetersIsOk()
        {
            //var converterFull = new DataPointConverter<double, bool>();
            //var converterSimple = new DataPointConverter<double>();

            var converterFull = new DataPointForecastedConverter<double, bool>();
            var converterSimple = new DataPointForecastedConverter<double>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",217.034988, \"2012-10-29T11:10:00\"],[\"2012-10-30T11:20:00\",217.025, \"2012-10-29T11:20:00\"],[\"2012-10-30T11:30:00\",217.025, \"2012-10-29T11:30:00\"],[\"2012-10-30T11:40:00\",217.015, \"2012-10-29T11:40:00\"],[\"2012-10-30T11:50:00\",217.015, \"2012-10-29T11:50:00\"]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(converterFull);
            options.Converters.Add(converterSimple);
            var timeseries = JsonSerializer.Deserialize<List<DataPointForecasted<double>>>(jsonString, options);
            //var timeseries = JsonConvert.DeserializeObject<List<DataPointForecasted<double>>>(jsonString, converterFull, converterSimple);
            Assert.NotEmpty(timeseries);
            Assert.Equal(217.034988, timeseries[0].Value);
            Assert.Equal("2012-10-29T11:10:00", timeseries[0].TimeOfForecast.ToString("s"));
            Assert.Equal("2012-10-30T11:50:00", timeseries.Last().DateTime.ToString("s"));
        }
        [Fact]
        public void ReadVectorIsOk()
        {
            var converter = new TimeSeriesDataConverter<Vector<double>>();
            const string jsonString = "[[\"2012-10-30T11:10:00\",{ \"X\":5.4,\"Y\":3.6,\"Size\": 4,\"Direction\":90}]]";

            var options = new JsonSerializerOptions
            {
                Converters = { converter },
                PropertyNameCaseInsensitive = true
            };

            var timeseries = System.Text.Json.JsonSerializer.Deserialize<TimeSeriesData<Vector<double>>>(jsonString, options);
            Assert.Equal(typeof(Vector<double>), timeseries.Values[0].GetType());
            Assert.Equal(5.4, ((Vector<double>)timeseries.Values[0]).X);
            Assert.Equal("2012-10-30T11:10:00", timeseries.DateTimes.Last().ToString("s"));
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

        [Fact]
        public void ConvertWithDictionObjectIsOk()
        {
            var converter = new DataPointConverter<double, bool>();
            const string jsonString = "{\"Type\":\"Single\", \"Value\":1.4}";

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, options);
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString, converter);
            Assert.Equal("Single", dict["Type"].ToString());
        }
    }
}
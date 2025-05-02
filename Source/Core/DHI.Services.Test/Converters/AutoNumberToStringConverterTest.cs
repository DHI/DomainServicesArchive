namespace DHI.Services.Test.Converters
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using Xunit;

    public class AutoNumberToStringConverterTest
    {
        private readonly JsonSerializerOptions _options;

        public AutoNumberToStringConverterTest()
        {
            _options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new AutoNumberToStringConverter() }
            };
        }

        [Fact]
        public void CanConvert()
        {
            Assert.True(new AutoNumberToStringConverter().CanConvert(typeof(string)));
        }

        [Fact]
        public void CanSerializerNumberAsString()
        {
            var sample = new Sample
            {
                NumberAsString = "10",
                StringAsNumber = 20
            };

            var actual = JsonSerializer.Serialize(sample, _options);

            var expected = "{\"NumberAsString\":\"10\",\"StringAsNumber\":20}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanDeserializerNumberAsString()
        {
            var json = "{\"NumberAsString\": 100,\"StringAsNumber\":\"150\"}";

            var actual = JsonSerializer.Deserialize<Sample>(json, _options);

            Assert.Equal("100", actual.NumberAsString);
            Assert.Equal(150, actual.StringAsNumber);
        }

        private class Sample
        {
            public string NumberAsString { get; set; }

            public int StringAsNumber { get; set; }
        }
    }
}

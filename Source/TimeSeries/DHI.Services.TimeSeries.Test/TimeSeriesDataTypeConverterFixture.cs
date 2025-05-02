namespace DHI.Services.TimeSeries.Test
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    public class TimeSeriesDataTypeConverterFixture
    {
        public TimeSeriesDataTypeConverterFixture()
        {
            Settings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Settings.Converters.Add(new EnumerationConverter());
        }

        public JsonConverter Converter { get; } = new DHI.Services.Converters.EnumerationConverter();

        public JsonSerializerOptions Settings { get; }
    }
}
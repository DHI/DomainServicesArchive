namespace DHI.Services.TimeSeries
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class TimeSeriesDataTypeConverter : JsonConverter<TimeSeriesDataType>
    {
        public bool CanWrite => false;

        public override TimeSeriesDataType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var displayName = document.RootElement.GetString();
            return Enumeration.FromDisplayName<TimeSeriesDataType>(displayName);
        }

        public override void Write(Utf8JsonWriter writer, TimeSeriesDataType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSeriesDataType);
        }
    }
}
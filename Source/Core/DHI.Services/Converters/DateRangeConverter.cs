using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace DHI.Services.Converters
{
    public class DateRangeConverter : JsonConverter<DateRange>
    {
        public override DateRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var from = root.GetProperty("from").GetDateTime();
                var to = root.GetProperty("to").GetDateTime();
                return new DateRange(from, to);
            }
        }

        public override void Write(Utf8JsonWriter writer, DateRange value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("from", value.From);
            writer.WriteString("to", value.To);
            writer.WriteEndObject();
        }
    }

}


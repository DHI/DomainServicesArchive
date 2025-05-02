namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DataPointForecastedConverter<TValue, TFlag> : JsonConverter<DataPointForecasted<TValue>>
        where TValue : struct
    {
        public override DataPointForecasted<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;
            DateTime dateTime = root[0].GetDateTime();
            TValue value = JsonSerializer.Deserialize<TValue>(root[1].GetRawText(), options);
            DateTime timeOfForecast = root[2].GetDateTime();
            return new DataPointForecasted<TValue>(dateTime, value, timeOfForecast);
        }


        public override void Write(Utf8JsonWriter writer, DataPointForecasted<TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.DateTime);
            JsonSerializer.Serialize(writer, value.Value, options);

            if (value is DataPointWFlag<TValue, TFlag> dataPointWFlag && dataPointWFlag.Flag != null)
            {
                writer.WriteRawValue(dataPointWFlag.Flag.ToString());
            }
            else if (value is DataPointForecasted<TValue> dataPointForecasted && dataPointForecasted.TimeOfForecast != default)
            {
                writer.WriteStringValue(dataPointForecasted.TimeOfForecast);
            }



            writer.WriteEndArray();
        }
    }

    public class DataPointForecastedConverter<TValue> : JsonConverter<DataPointForecasted<TValue>> where TValue : struct
    {
        public override DataPointForecasted<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;
                DateTime dateTime = root[0].GetDateTime();
                TValue value = JsonSerializer.Deserialize<TValue>(root[1].GetRawText(), options);
                DateTime timeOfForecast = root[2].GetDateTime();
                return new DataPointForecasted<TValue>(dateTime, value, timeOfForecast);
            }
        }

        public override void Write(Utf8JsonWriter writer, DataPointForecasted<TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.DateTime);

            if (value is DataPointWFlag<TValue, DataPoint> dataPointWFlag && dataPointWFlag.Flag != null)
            {
                writer.WriteRawValue(dataPointWFlag.Flag.ToString());
            }
            else if (value is DataPointForecasted<TValue> dataPointForecasted && dataPointForecasted.TimeOfForecast != default)
            {
                writer.WriteStringValue(dataPointForecasted.TimeOfForecast);
            }


            writer.WriteEndArray();
        }
    }
}
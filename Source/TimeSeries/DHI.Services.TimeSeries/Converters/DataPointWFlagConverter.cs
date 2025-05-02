namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DataPointWFlagConverter<TValue, TFlag> : JsonConverter<DataPointWFlag<TValue, TFlag>>
        where TValue : struct
    {
        public override DataPointWFlag<TValue, TFlag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                TFlag flag = JsonSerializer.Deserialize<TFlag>(root[2].GetRawText(), options);
                return new DataPointWFlag<TValue, TFlag>(dateTime, value, flag);
            }
        }


        public override void Write(Utf8JsonWriter writer, DataPointWFlag<TValue, TFlag> value, JsonSerializerOptions options)
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
    public class DataPointWFlagConverter<TFlag> : JsonConverter<DataPointWFlag<TFlag>>
    {
        public override DataPointWFlag<TFlag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;
                DateTime dateTime = root[0].GetDateTime();
                var value = JsonSerializer.Deserialize<double>(root[1].GetRawText(), options);
                //DateTime timeOfForecast = root[2].GetDateTime();
                var flag = JsonSerializer.Deserialize<TFlag>(root[2].GetRawText(), options);
                return new DataPointWFlag<TFlag>(dateTime, value, flag);
            }
        }

        public override void Write(Utf8JsonWriter writer, DataPointWFlag<TFlag> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.DateTime);
            JsonSerializer.Serialize(writer, value.Value, options);

            if (value is DataPointWFlag<TFlag> dataPointWFlag && dataPointWFlag.Flag != null)
            {
                writer.WriteRawValue(dataPointWFlag.Flag.ToString());
            }
            else
            {
                throw new Exception("not DataPointWFlag");
            }

            writer.WriteEndArray();

        }
    }
}
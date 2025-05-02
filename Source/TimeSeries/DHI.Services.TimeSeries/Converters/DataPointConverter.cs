namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// JSON Converter for data points.
    /// Handles all types of data points (also data points with flags and forecast datetime).
    /// </summary>
    /// <typeparam name="TValue">The type of the data value.</typeparam>
    /// <typeparam name="TFlag">The type of the flag.</typeparam>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter{T}" />
    public class DataPointConverter<TValue, TFlag> : JsonConverter<DataPoint<TValue>>
        where TValue : struct
    {
        public override DataPoint<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;
            DateTime dateTime = root[0].GetDateTime();
            TValue value = JsonSerializer.Deserialize<TValue>(root[1].GetRawText(), options);

            if (root.GetArrayLength() == 2)
            {
                return new DataPoint<TValue>(dateTime, value);
            }

            if (root[2].ValueKind == JsonValueKind.String)
            {
                DateTime timeOfForecast = root[2].GetDateTime();
                return new DataPointForecasted<TValue>(dateTime, value, timeOfForecast);
            }
            else
            {
                TFlag flag = JsonSerializer.Deserialize<TFlag>(root[2].GetRawText(), options);
                return new DataPointWFlag<TValue, TFlag>(dateTime, value, flag);
            }
        }


        public override void Write(Utf8JsonWriter writer, DataPoint<TValue> value, JsonSerializerOptions options)
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

    /// <summary>
    /// JSON Converter for data points.
    /// Only handles data points without flags or forecast datetime.
    /// </summary>
    /// <typeparam name="TValue">The type of the data value.</typeparam>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter{T}" />
    public class DataPointConverter<TValue> : JsonConverter<DataPoint<TValue>> where TValue : struct
    {
        public override DataPoint<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;
            DateTime dateTime = root[0].GetDateTime();
            //TValue value = root[1].GetValue<TValue>();
            TValue value = JsonSerializer.Deserialize<TValue>(root[1].GetRawText(), options);

            if (root.GetArrayLength() == 2)
            {
                return new DataPoint<TValue>(dateTime, value);
            }

            if (root[2].ValueKind == JsonValueKind.String)
            {
                DateTime timeOfForecast = root[2].GetDateTime();
                return new DataPointForecasted<TValue>(dateTime, value, timeOfForecast);
            }
            else
            {
                DataPoint flag = JsonSerializer.Deserialize<DataPoint>(root[2].GetRawText(), options);
                return new DataPointWFlag<TValue, DataPoint>(dateTime, value, flag);
            }
            //return new DataPoint<TValue>(dateTime, value);
        }

        public override void Write(Utf8JsonWriter writer, DataPoint<TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.DateTime);
            //writer.WriteNumberValue(value.Value);

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
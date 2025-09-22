namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.TimeSeries.Extensions;

    public class TimeSeriesDataConverter<TValue> : JsonConverter<ITimeSeriesData<TValue>> where TValue : struct
    {

        /// <summary>
        ///     Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ITimeSeriesData<TValue>).IsAssignableFrom(objectType);
        }

        /// <summary>
        ///     Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read from.</param>
        /// <param name="typeToConvert">Type of the object.</param>
        /// <param name="options">The calling serializer options.</param>
        /// <returns>The object value.</returns>
        public override ITimeSeriesData<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeseriesList = JsonDocument.ParseValue(ref reader).RootElement;

            var timeseries = new SortedSet<DataPoint<TValue>>();
            if (timeseriesList.ValueKind != JsonValueKind.Null)
            {
                if (timeseriesList.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elements in timeseriesList.EnumerateArray())
                    {
                        var element = elements.EnumerateArray().ToArray();
                        var dataPoint = convertToDataPoint(element[0], element[1]);
                        timeseries.Add(dataPoint);
                    }
                }
                else
                {
                    JsonElement datetimesElement = timeseriesList.GetPropertyJsonElement("DateTimes", options) ?? 
                        throw new JsonException("Unable to read \"DateTimes\" property with the supplied serializer settings");
                    JsonElement valuesElement = timeseriesList.GetPropertyJsonElement("Values", options) ??
                        throw new JsonException("Unable to read \"Values\" property with the supplied serializer settings");

                    if (datetimesElement.GetArrayLength() > 0)
                    {
                        var datetimes = datetimesElement.EnumerateArray().ToArray();
                        var values = valuesElement.EnumerateArray().ToArray();
                        for (int i = 0; i < datetimesElement.GetArrayLength(); i++)
                        {
                            var dataPoint = convertToDataPoint(datetimes[i], values[i]);
                            timeseries.Add(dataPoint);
                        }
                    }
                }
            }

            return new TimeSeriesData<TValue>(timeseries);
        }

        private DataPoint<TValue> convertToDataPoint(JsonElement datetimes, JsonElement values)
        {
            var dateTime = datetimes.GetDateTime();
            if (typeof(TValue) == typeof(Vector<double>))
            {
                var _options = new JsonSerializerOptions();
                var _converter = new VectorConverter();

                _options.Converters.Add(_converter);
                var _jsonString = values.GetRawText();
                TValue value = JsonSerializer.Deserialize<TValue>(_jsonString, _options);
                return new DataPoint<TValue>(dateTime, value);
            }
            else
            {
                TValue value = JsonSerializer.Deserialize<TValue>(values.GetRawText());
                return new DataPoint<TValue>(dateTime, value);
            }
        }

        /// <summary>
        ///     Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The calling serializer options.</param>
        public override void Write(Utf8JsonWriter writer, ITimeSeriesData<TValue> value, JsonSerializerOptions options)
        {

            writer.WriteStartArray();
            for (var i = 0; i < value.DateTimes.Count; i++)
            {

                writer.WriteStartArray();
                writer.WriteStringValue(value.DateTimes[i]);
                if (typeof(TValue) == typeof(double))
                {
                    var _value = value.Values[i].HasValue ? (double.Parse(value.Values[i].Value.ToString())) : double.NaN;

                    if (_value is double.NaN)
                    {
                        if (CustomSerializationSettings.UseNullForNaN)
                            writer.WriteNullValue();
                        else
                            writer.WriteStringValue($"{double.NaN}");
                    } 
                    else
                        writer.WriteNumberValue(_value);
                }
                else if (typeof(TValue) == typeof(Vector<double>))
                {
                    if (value.Values[i] is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        var vector = (Vector<double>)(object)value.Values[i];
                        writer.WriteStartObject();
                        writer.WriteNumber("X", vector.X);
                        writer.WriteNumber("Y", vector.Y);
                        writer.WriteNumber("Size", vector.Size);
                        writer.WriteNumber("Direction", vector.Direction);
                        writer.WriteEndObject();
                    }
                }
                else
                {
                    throw new NotSupportedException($"Type '{typeof(TValue)}' is not supported");
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
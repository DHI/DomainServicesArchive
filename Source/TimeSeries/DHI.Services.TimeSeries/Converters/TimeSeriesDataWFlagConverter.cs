namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// JSON converter for time series data with flags.
    /// </summary>
    /// <typeparam name="TValue">The type of the data values.</typeparam>
    /// <typeparam name="TFlag">The type of the flag.</typeparam>
    public class TimeSeriesDataWFlagConverter<TValue, TFlag> : JsonConverter<ITimeSeriesDataWFlag<TValue, TFlag>> where TValue : struct
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(ITimeSeriesDataWFlag<TValue, TFlag>).IsAssignableFrom(typeToConvert);
        }

        public override ITimeSeriesDataWFlag<TValue, TFlag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ITimeSeriesDataWFlag<TValue, TFlag> value, JsonSerializerOptions options)
        {
            var timeSeriesData = (ITimeSeriesData<TValue>)value;
            var valuesWFlag = timeSeriesData as ITimeSeriesDataWFlag<TValue, TFlag>;

            writer.WriteStartArray();

            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                writer.WriteStartArray();

                writer.WriteStringValue(timeSeriesData.DateTimes[i]);

                if (typeof(TValue) == typeof(double))
                {
                    //writer.WriteNumberValue(timeSeriesData.Values[i]);
                    writer.WriteRawValue(timeSeriesData.Values[i].Value.ToString());
                }
                else if (typeof(TValue) == typeof(Vector<double>))
                {
                    if (timeSeriesData.Values[i] is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        var vector = (Vector<double>)(object)timeSeriesData.Values[i];
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

                if (valuesWFlag != null)
                {
                    TFlag flag;
                    if ((flag = valuesWFlag.Flags[i]) != null)
                    {
                        if (valuesWFlag.Flags[i] is Dictionary<string, object>)
                        {
                            writer.WriteStartObject();
                            foreach (var entry in valuesWFlag.Flags[i] as Dictionary<string, object>)
                            {
                                writer.WritePropertyName(entry.Key);
                                JsonSerializer.Serialize(writer, entry.Value, entry.Value?.GetType() ?? typeof(object), options);
                            }

                            writer.WriteEndObject();
                        }
                        else
                        {
                            writer.WritePropertyName("Flag");
                            JsonSerializer.Serialize(writer, flag, flag?.GetType() ?? typeof(object), options);
                        }
                    }
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
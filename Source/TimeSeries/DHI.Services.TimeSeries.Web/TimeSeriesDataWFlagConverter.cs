namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    ///     Class TimeSeriesDataWFlagConverter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TFlag">The type of the flag.</typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class TimeSeriesDataWFlagConverter<TValue, TFlag> : JsonConverter where TValue : struct
    {
        /// <summary>
        ///     Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.</value>
        public override bool CanRead => false;

        /// <summary>
        ///     Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ITimeSeriesDataWFlag<TValue, TFlag>).IsAssignableFrom(objectType);
        }

        /// <summary>
        ///     Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeSeriesData = (ITimeSeriesData<TValue>)value;
            var valuesWFlag = timeSeriesData as ITimeSeriesDataWFlag<TValue, TFlag>;
            writer.WriteStartArray();
            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                writer.WriteStartArray();
                writer.WriteValue(timeSeriesData.DateTimes[i]);
                if (typeof(TValue) == typeof(double))
                {
                    writer.WriteValue(timeSeriesData.Values[i]);
                }
                else if (typeof(TValue) == typeof(Vector<double>))
                {
                    if (timeSeriesData.Values[i] is null)
                    {
                        writer.WriteValue((TValue?)null);
                    }
                    else
                    {
                        var vector = (Vector<double>)(object)timeSeriesData.Values[i];
                        writer.WriteStartObject();
                        writer.WritePropertyName("X");
                        writer.WriteValue(vector.X);
                        writer.WritePropertyName("Y");
                        writer.WriteValue(vector.Y);
                        writer.WritePropertyName("Size");
                        writer.WriteValue(vector.Size);
                        writer.WritePropertyName("Direction");
                        writer.WriteValue(vector.Direction);
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
                                writer.WriteValue(entry.Value);
                            }

                            writer.WriteEndObject();
                        }
                        else
                        {
                            writer.WriteValue(flag);
                        }
                    }
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
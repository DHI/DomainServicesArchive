namespace DHI.Services.TimeSeries.Web
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    ///     Class DataPointConverter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TFlag">The type of the flag.</typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class DataPointConverter<TValue, TFlag> : JsonConverter where TValue : struct
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
            return objectType.IsSubclassOf(typeof(DataPoint<TValue>)) || objectType.IsAssignableFrom(typeof(DataPoint<TValue>));
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
            var dataPoint = (DataPoint<TValue>)value;
            writer.WriteStartArray();
            writer.WriteValue(dataPoint.DateTime);
            writer.WriteValue(dataPoint.Value);
            switch (dataPoint)
            {
                case DataPointWFlag<TValue, TFlag> dataPointWFlag when dataPointWFlag.Flag != null:
                    writer.WriteValue(dataPointWFlag.Flag);
                    break;
                case DataPointForecasted<TValue> dataPointForecasted when dataPointForecasted.TimeOfForecast != default(DateTime):
                    writer.WriteValue(dataPointForecasted.TimeOfForecast);
                    break;
            }

            writer.WriteEndArray();
        }
    }
}
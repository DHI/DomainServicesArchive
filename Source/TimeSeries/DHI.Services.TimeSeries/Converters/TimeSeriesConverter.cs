namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.TimeSeries.Extensions;

    public class TimeSeriesConverter<TId, TValue> : JsonConverter<TimeSeries<TId, TValue>>
        where TValue : struct
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var genericType = typeof(TimeSeries<,>).MakeGenericType(typeof(TId), typeof(TValue));
            if (genericType == typeToConvert &&
                typeToConvert.GenericTypeArguments[0].Equals(typeof(TId)) &&
                typeToConvert.GenericTypeArguments[1].Equals(typeof(TValue)))
            {
                return true;
            }

            return false;
        }

        public override TimeSeries<TId, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");

            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (JsonDocument.TryParseValue(ref reader, out var doc))
            {
                var root = doc.RootElement;

                var idProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Id), options) ??
                    throw new JsonException($"Required non null 'id' for {typeof(TimeSeries<,>)} type");

                var nameProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Name), options) ?? throw new JsonException($"Required non null 'name' for {typeof(TimeSeries<,>)} type");

                if (string.IsNullOrEmpty(nameProperty.GetString()))
                {
                    throw new JsonException($"Required non null 'name' for {typeof(TimeSeries<,>)} type");
                }

                var groupProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Group), options);
                var dimensionProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Dimension), options);
                var quantityProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Quantity), options);
                var unitProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Unit), options);

                ITimeSeriesData<TValue> data = null;
                var dataProperty = root.GetPropertyJsonElement(nameof(TimeSeries.Data), options);
                if (dataProperty.HasValue)
                {
                    data = dataProperty.Value.Deserialize<TimeSeriesData<TValue>>(options);
                }

                var id = (TId)Convert.ChangeType(idProperty.GetString(), typeof(TId));
                var timeSeries = new TimeSeries<TId, TValue>(id,
                    nameProperty.GetString(),
                    groupProperty?.GetString() ?? null,
                    dimensionProperty?.GetString(),
                    quantityProperty?.GetString(),
                    unitProperty?.GetString(),
                    data);

                var dataTypeProperty = root.GetPropertyJsonElement(nameof(TimeSeries.DataType), options);
                if (dataTypeProperty.HasValue)
                {
                    timeSeries.DataType = dataTypeProperty.Value.Deserialize<TimeSeriesDataType>(options);
                }

                return timeSeries;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TimeSeries<TId, TValue> value, JsonSerializerOptions options)
        {
            // We don't need any custom serialization logic for writing the json.
            // Ideally, this method should not be called at all. It's only called if you
            // supply JsonSerializerOptions that contains this JsonConverter in it's Converters list.
            // Don't do that, you will lose performance because of the cast needed below.
            // Cast to avoid infinite loop: https://github.com/dotnet/docs/issues/19268
            JsonSerializer.Serialize(writer, value);
        }
    }
}

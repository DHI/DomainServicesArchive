namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class VectorConverter<TValue> : JsonConverter<Vector<TValue>>
        where TValue : struct
    {
        public override Vector<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;
            root.TryGetProperty("X", out var tx);
            root.TryGetProperty("Y", out var ty);

            var x = (TValue)Convert.ChangeType(tx, typeof(TValue));
            var y = (TValue)Convert.ChangeType(ty, typeof(TValue));
            var vector = new Vector<TValue>(x, y);

            return vector;
        }

        public override void Write(Utf8JsonWriter writer, Vector<TValue> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class VectorConverter : JsonConverter<Vector<double>>
    {
        public override Vector<double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var x = 0.0;
            var y = 0.0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Vector<double>(x, y);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = JsonSerializer.Deserialize<double>(ref reader, options);
                        break;
                    case "Y":
                        y = JsonSerializer.Deserialize<double>(ref reader, options);
                        break;
                    default:

                        break;
                        //throw new JsonException($"Unknown property: {propertyName}");
                }
            }

            throw new JsonException();
            //if (reader.Read() && reader.TryGetDouble(out x) &&
            //    reader.Read() && reader.TryGetDouble(out y) &&
            //    reader.Read() && reader.TokenType == JsonTokenType.EndObject)
            //{
            //    return new Vector<double>(x, y);
            //}

            //throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Vector<double> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
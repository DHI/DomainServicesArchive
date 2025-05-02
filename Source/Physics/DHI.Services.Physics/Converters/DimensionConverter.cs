namespace DHI.Services.Physics.Converters
{
    using DHI.Physics;
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class DimensionConverter : JsonConverter<Dimension>
    {
        public override Dimension Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                if(reader.GetString() == "Exponents")
                {
                    reader.Read();
                    var exponents = JsonSerializer.Deserialize<double[]>(ref reader, options);
                    return new Dimension(exponents);
                }
                else reader.Skip();
            }
            return new Dimension();
        }


        public override void Write(Utf8JsonWriter writer, Dimension value, JsonSerializerOptions options)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fieldExponents = typeof(Dimension).GetField("Exponents", bindingFlags);
            writer.WriteStartObject();
            writer.WritePropertyName(fieldExponents.Name);
            writer.WriteStartArray();
            foreach(var exponentsValue in fieldExponents.GetValue(value) as double[])
            {
                writer.WriteNumberValue(exponentsValue);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}

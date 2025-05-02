namespace DHI.Services.Tables.WebApi.Converters
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    //
    // Summary:
    //     Custom System.Text.Json.Serialization.JsonConverter for handling multidimensional
    //     array of T
    //
    // Type parameters:
    //   T:
    public class TwoDimensionalArrayConverter<T> : JsonConverter<T[,]>
    {
        public override T[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Null && reader.TokenType == JsonTokenType.StartArray)
            {
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    int arrayLength = jsonDocument.RootElement.GetArrayLength();
                    int arrayLength2 = jsonDocument.RootElement.EnumerateArray().First().GetArrayLength();
                    var array = new T[arrayLength, arrayLength2];
                    int num = 0;
                    foreach (JsonElement item in jsonDocument.RootElement.EnumerateArray())
                    {
                        int num2 = 0;
                        foreach (JsonElement item2 in item.EnumerateArray())
                        {
                            array[num, num2] = item2.Deserialize<T>(options);
                            num2++;
                        }

                        num++;
                    }

                    return array;
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            writer.WriteStartArray();
            for (int i = 0; i < value.GetLength(0); i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < value.GetLength(1); j++)
                {
                    writer.WriteRawValue(JsonSerializer.Serialize(value[i, j], options));
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}

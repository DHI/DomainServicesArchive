namespace DHI.Services.Spreadsheets.Converters
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Custom <seealso cref="JsonConverter"/> for handling multidimensional array of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// ref: https://makolyte.com/csharp-serialize-and-deserialize-a-multidimensional-array-to-json/
    public class TwoDimensionalArrayConverter<T> : JsonConverter<T[,]>
    {
        public override T[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != (JsonTokenType.Null | JsonTokenType.None) &&
                reader.TokenType == JsonTokenType.StartArray)
            {
                using var jsonDoc = JsonDocument.ParseValue(ref reader);

                var rowLength = jsonDoc.RootElement.GetArrayLength();
                var columnLength = jsonDoc.RootElement.EnumerateArray().First().GetArrayLength();

                var matrix = new T[rowLength, columnLength];

                int row = 0;
                foreach (var array in jsonDoc.RootElement.EnumerateArray())
                {
                    int column = 0;
                    foreach (var value in array.EnumerateArray())
                    {
                        matrix[row, column] = JsonSerializer.Deserialize<T>(value, options);
                        column++;
                    }
                    row++;
                }

                return matrix;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
        {
            if (value == null) return;

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

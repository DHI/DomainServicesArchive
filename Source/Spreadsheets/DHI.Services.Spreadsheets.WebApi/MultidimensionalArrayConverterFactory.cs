namespace DHI.Services.Spreadsheets.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class MultidimensionalArrayConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type type)
        => type.IsArray && type.GetArrayRank() > 1;

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            var elementType = type.GetElementType();
            var converterType = typeof(MultidimensionalArrayConverter<>)
                                    .MakeGenericType(elementType);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    public class MultidimensionalArrayConverter<T> : JsonConverter<T[,]>
    {
        public override T[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Expected StartArray but got {reader.TokenType}");

            var rows = new List<List<T>>();
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException($"Expected StartArray for row but got {reader.TokenType}");

                reader.Read();
                var row = new List<T>();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    var element = JsonSerializer.Deserialize<T>(ref reader, options);
                    row.Add(element);
                    reader.Read();
                }

                rows.Add(row);
                reader.Read();
            }

            var d0 = rows.Count;
            var d1 = d0 > 0 ? rows[0].Count : 0;
            var result = new T[d0, d1];
            for (int i = 0; i < d0; i++)
                for (int j = 0; j < d1; j++)
                    result[i, j] = rows[i][j];

            return result;
        }


        public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
        {
            int d0 = value.GetLength(0);
            int d1 = value.GetLength(1);

            writer.WriteStartArray();
            for (int i = 0; i < d0; i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < d1; j++)
                {
                    JsonSerializer.Serialize(writer, value[i, j], options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}

namespace DHI.Services.Tables.WebApi.Converters
{
    using System;
    using System.Buffers.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DoubleConverter : JsonConverter<double>
    {
        // Set to true to prevent intermediate parsing. Be careful to ensure your raw JSON is well-formed.
        private const bool skipInputValidation = true;

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string text = reader.GetString();

                return text.ToLowerInvariant() switch
                {
                    "nan" => double.NaN,
                    "-infinity" => double.NegativeInfinity,
                    "infinity" => double.PositiveInfinity,
                    _ => double.NaN
                };
            }
            
            return reader.GetDouble();
        }

        //ref: https://stackoverflow.com/a/66358909
        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            // JsonConstants.MaximumFormatDecimalLength + 2, https://github.com/dotnet/runtime/blob/v6.0.11/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L85
            Span<byte> utf8bytes = stackalloc byte[33];

            if (Utf8Formatter.TryFormat(value, utf8bytes.Slice(0, utf8bytes.Length - 2), out var bytesWritten))
            {
                if (IsInteger(utf8bytes, bytesWritten))
                {
                    utf8bytes[bytesWritten++] = (byte)'.';
                    utf8bytes[bytesWritten++] = (byte)'0';
                }
                writer.WriteRawValue(utf8bytes.Slice(0, bytesWritten), skipInputValidation);
            }
            else
                writer.WriteNumberValue(value);
        }

        private static bool IsInteger(Span<byte> utf8bytes, int bytesWritten)
        {
            if (bytesWritten <= 0)
            {
                return false;
            }

            var start = utf8bytes[0] == '-' ? 1 : 0;

            for (var i = start; i < bytesWritten; i++)
            {
                if (!(utf8bytes[i] >= '0' && utf8bytes[i] <= '9'))
                {
                    return false;
                }
            }
            return start < bytesWritten;
        }
    }
}

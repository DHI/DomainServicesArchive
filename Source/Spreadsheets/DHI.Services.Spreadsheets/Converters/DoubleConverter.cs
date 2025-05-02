namespace DHI.Services.Spreadsheets.Converters
{
    using System;
    using System.Buffers.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    //ref: https://stackoverflow.com/a/66358909
    public class DoubleConverter : JsonConverter<double>
    {
        // Set to true to prevent intermediate parsing.  Be careful to ensure your raw JSON is well-formed.
        private const bool skipInputValidation = true;
        
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            // TODO: Handle "NaN", "Infinity", "-Infinity"
            reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            // JsonConstants.MaximumFormatDecimalLength + 2, https://github.com/dotnet/runtime/blob/v6.0.11/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L85
            Span<byte> utf8bytes = stackalloc byte[33];

            //if (!double.ISFinite(value))
            //    // Utf8JsonWriter does not take into account JsonSerializerOptions.NumberHandling so we have to make a recursive call to serialize
            //    JsonSerializer.Serialize(writer, value, new JsonSerializerOptions
            //    {
            //        NumberHandling = options.NumberHandling
            //    });
            //else 
            if (Utf8Formatter.TryFormat(value, utf8bytes.Slice(0, utf8bytes.Length - 2), out var bytesWritten))
            {
                // Check to make sure the value was actually serialized as an integer and not, say, using scientific notation for large values.
                if (IsInteger(utf8bytes, bytesWritten))
                {
                    utf8bytes[bytesWritten++] = (byte)'.';
                    utf8bytes[bytesWritten++] = (byte)'0';
                }
                writer.WriteRawValue(utf8bytes.Slice(0, bytesWritten), skipInputValidation);
            }
            else // Buffer was too small?
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

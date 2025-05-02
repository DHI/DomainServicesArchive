namespace DHI.Services.Places.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using DHI.Services.Converters;

    //TODO: That mean there is smell-code on DHI.Services.Converters.ObjectToInferredTypeConverter. this patch should be applied on next release of DHI.Services
    internal class ObjectToInferredTypeConverterPatch : ObjectToInferredTypeConverter
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    {
                        if (reader.TryGetInt32(out var value4))
                        {
                            return value4;
                        }

                        if (reader.TryGetInt64(out var value5))
                        {
                            return value5;
                        }

                        if (reader.TryGetDouble(out var value6))
                        {
                            return value6;
                        }

                        if (reader.TryGetDecimal(out var value7))
                        {
                            return value7;
                        }

                        break;
                    }
                case JsonTokenType.String:
                    {
                        if (reader.TryGetDateTime(out var value))
                        {
                            return value;
                        }

                        if (reader.TryGetGuid(out var value2))
                        {
                            return value2;
                        }

                        var stringValue = reader.GetString() ?? string.Empty;
                        if (isBase64String(stringValue))
                        {
                            if (reader.TryGetBytesFromBase64(out var value3))
                            {
                                return value3;
                            }
                        }

                        return stringValue;
                    }
            }

            return JsonDocument.ParseValue(ref reader).RootElement.Clone();
        }

        public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType());
        }

        static bool isBase64String(string? base64)
        {
            try
            {
                var output = Convert.FromBase64String(base64);
                return (output.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}

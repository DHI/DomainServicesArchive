using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobAutomator;
sealed class BoolFlexibleConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var n))
                    return n != 0;
                var num = reader.GetDouble();
                return Math.Abs(num) > double.Epsilon;
            case JsonTokenType.String:
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return false;
                if (bool.TryParse(s, out var b))
                    return b;
                // Accept 1/0 as strings too
                if (long.TryParse(s, out var i))
                    return i != 0;
                // Optional: on/off, yes/no
                var t = s.Trim().ToLowerInvariant();
                if (t is "on" or "yes" or "y")
                    return true;
                if (t is "off" or "no" or "n")
                    return false;
                throw new JsonException($"Cannot convert '{s}' to boolean.");
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} for boolean.");
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);
}

sealed class NullableBoolFlexibleConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        // Delegate to the non-nullable converter
        var tmp = new BoolFlexibleConverter();
        return tmp.Read(ref reader, typeof(bool), options);
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteBooleanValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

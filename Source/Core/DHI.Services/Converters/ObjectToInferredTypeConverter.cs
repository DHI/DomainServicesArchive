
namespace DHI.Services.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Deserialize object properties into inferred types instead of JsonElement
    /// </summary>
    /// <remarks>
    ///     https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-6-0#deserialize-inferred-types-to-object-properties
    /// </remarks>  
    public class ObjectToInferredTypeConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt32(out int int32) => int32,
            JsonTokenType.Number when reader.TryGetInt64(out long @long) => @long,
            JsonTokenType.Number when reader.TryGetDouble(out double @double) => @double,
            JsonTokenType.Number when reader.TryGetDecimal(out decimal @decimal) => @decimal,
            JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
            JsonTokenType.String when reader.TryGetGuid(out Guid guid) => guid,
            JsonTokenType.String when reader.TryGetBytesFromBase64(out byte[] bytes) => bytes,
            JsonTokenType.String => reader.GetString()!,
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };

        public override void Write(
          Utf8JsonWriter writer,
          object objectToWrite,
          JsonSerializerOptions options) =>
          JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType());
    }
}
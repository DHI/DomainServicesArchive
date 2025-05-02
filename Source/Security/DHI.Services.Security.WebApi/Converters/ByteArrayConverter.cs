namespace DHI.Services.Security.WebApi.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Authorization;

    public class ByteArrayConverter : JsonConverter<byte[]?>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (JsonDocument.TryParseValue(ref reader, out var document))
            {
                if (document.RootElement.TryGetProperty("$value", out var value))
                {
                    var pwdByte = value.GetBytesFromBase64();
                    return pwdByte;
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (UserGroup)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        writer.WriteStartObject();
                        writer.WriteString("$type", $"{type.ResolveAssemblyName()}, {type.Namespace}");

                        writer.WritePropertyName("$value");
                        writer.WriteStringValue(Convert.ToBase64String(value));

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}

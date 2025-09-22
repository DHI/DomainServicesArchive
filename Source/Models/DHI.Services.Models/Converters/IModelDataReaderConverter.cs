namespace DHI.Services.Models.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class IModelDataReaderConverter : JsonConverter<IModelDataReader>
    {
        public override IModelDataReader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("TypeName", out var typeNameProp))
                throw new JsonException("Missing TypeName discriminator");

            var typeName = typeNameProp.GetString();

            if (string.IsNullOrEmpty(typeName))
                throw new JsonException("Empty TypeName");

            var targetType = ModelDataReaderTypeRegistry.GetTypeFor(typeName);

            if (targetType == null)
                throw new JsonException($"No registered type for '{typeName}'");

            return (IModelDataReader?)JsonSerializer.Deserialize(root.GetRawText(), targetType, options);
        }

        public override void Write(Utf8JsonWriter writer, IModelDataReader value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

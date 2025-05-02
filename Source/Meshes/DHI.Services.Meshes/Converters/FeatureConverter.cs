namespace DHI.Services.Meshes.Converters
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Spatial;

    public class FeatureConverter : Spatial.GeoJson.FeatureConverter
    {

        public override void Write(Utf8JsonWriter writer, IFeature value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (IFeature)null, options);
                    break;
                default:
                    {
                        var feature = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteStringValue(nameof(Feature));

                        var serializer = new JsonSerializerOptions(options);
                        serializer.Converters.Clear();
                        serializer.AddConverters(new List<JsonConverter>()
                    {
                        new GeometryConverter(),
                        new DictionaryConverter<object?>(),
                    });

                        writer.WritePropertyName(nameof(IFeature.Geometry).ToCamelCase());
                        JsonSerializer.Serialize(writer, feature.Geometry, serializer);

                        writer.WritePropertyName("Properties".ToCamelCase());
                        JsonSerializer.Serialize(writer, feature.AttributeValues, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}
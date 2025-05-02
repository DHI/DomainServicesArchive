namespace DHI.Services.Meshes.Converters
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Spatial;
    using DHI.Spatial.GeoJson;


    public class FeatureCollectionConverter : Spatial.GeoJson.FeatureCollectionConverter
    {

        public override void Write(Utf8JsonWriter writer, IFeatureCollection value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (IFeatureCollection)null, options);
                    break;
                default:
                    {
                        var featureCollection = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteStringValue(nameof(FeatureCollection));

                        var serializer = new JsonSerializerOptions(options);
                        serializer.Converters.Clear();
                        serializer.AddConverters(new List<JsonConverter>()
                    {
                        new FeatureConverter(),
                        new AttributeConverter(),
                        new DictionaryConverter<object?>()

                    });

                        writer.WritePropertyName(nameof(IFeatureCollection.Features), true);
                        JsonSerializer.Serialize(writer, featureCollection.Features, serializer);

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}
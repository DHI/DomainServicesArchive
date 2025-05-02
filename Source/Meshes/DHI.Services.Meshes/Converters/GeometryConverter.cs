namespace DHI.Services.Meshes.Converters
{

    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using DHI.Spatial.GeoJson;
    using DHI.Spatial;

    public class GeometryConverter : Spatial.GeoJson.GeometryConverter
    {

        public override void Write(Utf8JsonWriter writer, IGeometry value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, null as IGeometry, options);
                    break;
                default:
                    {
                        var geometry = value;
                        writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteStringValue(geometry.Type);

                        var serializer = createSerializer(options, new JsonConverter[]
                        {
                        new PositionConverter(),
                        });


                        switch (options.DefaultIgnoreCondition)
                        {
                            case JsonIgnoreCondition.Never or JsonIgnoreCondition.WhenWritingDefault:

                                writer.WritePropertyName(nameof(IGeometry.Coordinates).ToCamelCase());
                                JsonSerializer.Serialize(writer, geometry.Coordinates, serializer);

                                writer.WritePropertyName(nameof(IGeometry.CRS).ToCamelCase());
                                JsonSerializer.Serialize(writer, geometry.CRS, serializer);
                                break;

                            case JsonIgnoreCondition.WhenWritingNull:
                                if (geometry.Coordinates != null)
                                {
                                    writer.WritePropertyName(nameof(IGeometry.Coordinates).ToCamelCase());
                                    JsonSerializer.Serialize(writer, geometry.Coordinates, serializer);
                                }
                                if (geometry.CRS != null)
                                {
                                    writer.WritePropertyName(nameof(IGeometry.CRS).ToCamelCase());
                                    JsonSerializer.Serialize(writer, geometry.CRS, serializer);
                                }
                                break;

                            default:
                                break;
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }

        JsonSerializerOptions createSerializer(JsonSerializerOptions options, IEnumerable<JsonConverter>? converters = null)
        {
            var serializer = new JsonSerializerOptions(options);
            serializer.Converters.Clear();
            if (converters?.Any() == true)
            {
                serializer.AddConverters(converters);
            }

            serializer.Converters.Add(new CoordinateReferenceSystemConverter());
            serializer.Converters.Add(new ObjectToInferredTypeConverter());
            serializer.Converters.Add(new TypeStringConverter());
            serializer.Converters.Add(new DictionaryConverter<object?>());

            return serializer;
        }
    }
}
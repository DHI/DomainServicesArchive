namespace DHI.Services.Physics
{
    using DHI.Physics;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;


    public class UnitConverter : JsonConverter<Unit>
    {
        //public override bool CanWrite => false;

        //public override bool CanConvert(Type objectType)
        //{
        //    return objectType == typeof (Unit);
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    var jo = JObject.Load(reader);
        //    var id = (string)jo["Id"];
        //    var description = (string)jo["Description"];
        //    var abbreviation = (string)jo["Abbreviation"];
        //    var factor = (double)jo["Factor"];
        //    var jdimension = jo["Dimension"];
        //    var dimension = jdimension.ToObject<Dimension>();
        //    var unit = new Unit(id, description, abbreviation, factor, dimension);
        //    return unit;
        //}

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    throw new NotImplementedException();
        //}

        private readonly DataContractJsonSerializer _dataContactSerializer = new DataContractJsonSerializer(typeof(Unit));

        public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string id = null;
            string description = null;
            string abbreviation = null;
            double factor = default;
            Dimension dimension = default;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                string propertyName = reader.GetString();
                switch (propertyName)
                {
                    case nameof(Unit.Id):
                        reader.Read();
                        id = reader.GetString();
                        break;
                    case nameof(Unit.Description):
                        reader.Read();
                        description = reader.GetString();
                        break;
                    case nameof(Unit.Abbreviation):
                        reader.Read();
                        abbreviation = reader.GetString();
                        break;
                    case nameof(Unit.Factor):
                        reader.Read();
                        factor = reader.GetDouble();
                        break;
                    case nameof(Unit.Dimension):
                        reader.Read();
                        dimension = JsonSerializer.Deserialize<Dimension>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var unit = new Unit(id, description, abbreviation, factor, dimension);
            return unit;
        }

        public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(nameof(Unit.Id), value.Id);
            writer.WriteString(nameof(Unit.Description), value.Description);
            writer.WriteString(nameof(Unit.Abbreviation), value.Abbreviation);
            writer.WriteNumber(nameof(Unit.Factor), value.Factor);
            
            writer.WritePropertyName(nameof(Unit.Dimension));
            JsonSerializer.Serialize(writer, value.Dimension);
            
            if(value.ShouldSerializeMetadata()) 
            {
                writer.WritePropertyName(nameof(Unit.Metadata));
                JsonSerializer.Serialize(writer, value.Metadata);
            }

            writer.WriteEndObject();
        }
    }
}
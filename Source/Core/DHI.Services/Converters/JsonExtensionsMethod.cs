namespace DHI.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class JsonExtensionsMethod
    {
        public static string ToJsonString(this JsonElement element)
            => element.ValueKind == JsonValueKind.Undefined ? "" : JsonSerializer.Serialize(element, new JsonSerializerOptions());

        public static JsonElement GetProperty(this JsonElement element, string propertyName, bool propertyNameCamelCase = false) =>
            propertyNameCamelCase ?
            element.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(propertyName)) :
            element.GetProperty(propertyName);

        public static bool TryGetProperty(this JsonElement element, string propertyName, out JsonElement jsonElement, bool propertyNameCamelCase = false) =>
            propertyNameCamelCase ?
            element.TryGetProperty(JsonNamingPolicy.CamelCase.ConvertName(propertyName), out jsonElement) :
            element.TryGetProperty(propertyName, out jsonElement);

        public static void WritePropertyName(this Utf8JsonWriter writer, string propertyName, bool propertyNameCamelCase = false)
        {
            if (propertyNameCamelCase)
                writer.WritePropertyName(JsonNamingPolicy.CamelCase.ConvertName(propertyName));
            else
                writer.WritePropertyName(propertyName);
        }

        public static string ToCamelCase(this string stringValue) => JsonNamingPolicy.CamelCase.ConvertName(stringValue);

        /// <summary>
        ///     Add list of custom <seeaslo cref="JsonConverter"/> to <paramref name="serializerOptions"/>
        /// </summary>
        /// <param name="serializerOptions"><seealso cref="JsonSerializerOptions"/></param>
        /// <param name="customConverters">List of custom <seealso cref="JsonConverter"/> to be added</param> 
        public static JsonSerializerOptions AddConverters(this JsonSerializerOptions serializerOptions, IEnumerable<JsonConverter> customConverters)
        {
            return serializerOptions.AddConverters(customConverters?.ToArray());
        }

        /// <summary>
        ///     Add list of custom <seeaslo cref="JsonConverter"/> to <paramref name="serializerOptions"/>
        /// </summary>
        /// <param name="serializerOptions"><seealso cref="JsonSerializerOptions"/></param>
        /// <param name="customConverters">List of custom <seealso cref="JsonConverter"/> to be added</param> 
        public static JsonSerializerOptions AddConverters(this JsonSerializerOptions serializerOptions, params JsonConverter[] customConverters)
        {
            if (customConverters?.Any() == true)
            {
                foreach (var converter in
                    customConverters.Where(x => serializerOptions.Converters.Any(y => y.GetType().Equals(x.GetType())) == false))
                {
                    serializerOptions.Converters.Add(converter);
                }
            }

            return serializerOptions;
        }
    }
}

namespace DHI.Services.TimeSeries.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    public static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyJsonElement(this JsonElement element, string propertyName, JsonSerializerOptions options)
        {
            JsonElement? valueElement = null;

            // Try to parse camel case.
            if (options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase)
            {
                if (element.TryGetProperty(propertyName, out var elem, true))
                {
                    valueElement = elem;
                }
            }

            // Try to parse as pascal case.
            if (valueElement == null && options.PropertyNamingPolicy != JsonNamingPolicy.CamelCase)
            {
                if (element.TryGetProperty(propertyName, out var elem))
                {
                    valueElement = elem;
                }
            }

            // Try to parse as case insensitive.
            if (valueElement == null && options.PropertyNameCaseInsensitive)
            {
                var property = element.EnumerateObject()
                    .FirstOrDefault(property => string.Compare(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);
                if (property.Value.ValueKind != JsonValueKind.Undefined)
                {
                    valueElement = property.Value;
                }
            }

            return valueElement;
        }
    }
}

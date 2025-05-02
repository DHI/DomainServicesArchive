namespace DHI.Services.JsonDocuments.WebApi
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static IEnumerable<JsonDocumentDTO> ToDTOs(this IEnumerable<JsonDocument<string>> jsonDocuments)
        {
            return jsonDocuments.Select(document => new JsonDocumentDTO(document));
        }

        public static JsonDocumentDTO ToDTO(this JsonDocument<string> document)
        {
            return new JsonDocumentDTO(document);
        }
    }
}
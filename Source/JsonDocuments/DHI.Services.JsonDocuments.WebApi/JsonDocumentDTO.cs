namespace DHI.Services.JsonDocuments.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using WebApiCore;

    /// <summary>
    ///     Data transfer object for a JSON document resource representation
    /// </summary>
    public class JsonDocumentDTO
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocumentDTO" /> class.
        /// </summary>
        public JsonDocumentDTO()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocumentDTO" /> class.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        public JsonDocumentDTO(JsonDocument<string> jsonDocument)
        {
            FullName = jsonDocument.FullName;
            Data = jsonDocument.Data;
            DateTime = jsonDocument.DateTime;
            Added = jsonDocument.Added;
            Updated = jsonDocument.Updated;
            Deleted = jsonDocument.Deleted;

            if (jsonDocument.Permissions.Any())
            {
                Permissions = new List<PermissionDTO>();
                foreach (var permission in jsonDocument.Permissions)
                {
                    Permissions.Add(new PermissionDTO(permission));
                }
            }

            if (jsonDocument.Metadata.Any())
            {
                Metadata = new Dictionary<object, object>();
                foreach (var data in jsonDocument.Metadata)
                {
                    Metadata.Add(data.Key, data.Value);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the fullname.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        ///     Gets or sets the JSON data.
        /// </summary>
        [Required]
        public string Data { get; set; }

        /// <summary>
        ///     Gets or sets the custom date time.
        /// </summary>
        public DateTime? DateTime { get; set; }

        /// <summary>
        ///     Gets the datetime the entity was added to the repository.
        /// </summary>
        public DateTime? Added { get; set; }

        /// <summary>
        ///     Gets the most recent time the entity was updated in the repository.
        /// </summary>
        public DateTime? Updated { get; set; }

        /// <summary>
        ///     Gets the time the entity was (soft) deleted.
        /// </summary>
        public DateTime? Deleted { get; set; }

        /// <summary>
        ///     Gets or sets the permissions.
        /// </summary>
        public List<PermissionDTO> Permissions { get; set; }

        /// <summary>
        ///     Gets or sets the metadata.
        /// </summary>
        public Dictionary<object, object> Metadata { get; set; }

        /// <summary>
        ///     Converts the DTO to a JSON document object.
        /// </summary>
        public JsonDocument<string> ToJsonDocument()
        {
            var fullName = DHI.Services.FullName.Parse(FullName);
            var jsonDocument = new JsonDocument<string>(fullName.ToString(), fullName.Name, fullName.Group, Data)
            {
                DateTime = DateTime,
                Updated =  Updated,
                Deleted = Deleted
            };

            if (Metadata != null && Metadata.Count > 0)
            {
                foreach (var pair in Metadata)
                {
                    jsonDocument.Metadata.Add((string)pair.Key, pair.Value);
                }
            }

            if (Permissions is null)
            {
                return jsonDocument;
            }

            foreach (var permissionDTO in Permissions)
            {
                jsonDocument.Permissions.Add(permissionDTO.ToPermission());
            }

            return jsonDocument;
        }
    }
}
namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Authorization;

    /// <summary>
    ///     Data transfer object for user group resource representation.
    /// </summary>
    [Serializable]
    public class UserGroupDTO
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the users.
        /// </summary>
        /// <value>The users.</value> 
        [JsonPropertyName("users")]
        public HashSet<string> Users { get; set; }

        /// <summary>
        ///     Gets or sets the metadata.
        /// </summary> 
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        ///     Converts the DTO to a UserGroup object.
        /// </summary>
        public UserGroup ToUserGroup()
        {
            var userGroup = new UserGroup(Id, Name, Users);

            if ((Metadata is not null))
            {
                foreach (var kvp in Metadata)
                {
                    userGroup.Metadata.Add(kvp.Key.AsString(), kvp.Value);
                }
            }

            return userGroup;
        }
    }
}
namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    [Serializable]
    public class AccountBaseDTO
    {
        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        [Required]
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the company.
        /// </summary>
        [JsonPropertyName("company")]
        public string Company { get; set; }

        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets the roles.
        /// </summary>
        [JsonPropertyName("roles")]
        public virtual string Roles { get; set; }

        /// <summary>
        ///     Gets or sets the metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        ///     Sets the user groups.
        /// </summary>
        [JsonPropertyName("userGroups")]
        public IEnumerable<string> UserGroups { get; set; }

        /// <summary>
        ///     Gets or sets the NumberOfLoginAttemp.
        /// </summary>
        [JsonPropertyName("noOfUnsuccessfulLoginAttempts")]
        public int NoOfUnsuccessfulLoginAttempts { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the LastLoginDate.
        /// </summary>
        [JsonPropertyName("lastLoginAttemptedDate")]
        public DateTime LastLoginAttemptedDate { get; set; } = DateTime.Now;

        /// <summary>
        ///     Gets or sets the Locked.
        /// </summary>
        [JsonPropertyName("locked")]
        public bool Locked { get; set; } = false;

        /// <summary>
        ///     Gets or sets the LockedDateEnd.
        /// </summary>
        [JsonPropertyName("lockedDateEnd")]
        public DateTime? LockedDateEnd { get; set; } = DateTime.Now;
    }
}
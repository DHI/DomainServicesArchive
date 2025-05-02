namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Accounts;

    /// <summary>
    ///     Data transfer object for a account registration resource representation.
    /// </summary>
    [Serializable]
    public class RegistrationDTO
    {
        /// <summary>
        ///     Gets or sets the company.
        /// </summary>
        [JsonPropertyName("company")]
        public string Company { get; set; }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

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
        ///     Gets or sets the password.
        /// </summary>
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        ///     Converts the DTO to an Account object.
        /// </summary>
        public Account ToAccount()
        {
            var account = new Account(Id, Name);
            account.SetPassword(Password);
            account.Company = Company;
            account.Email = Email;
            return account;
        }
    }
}
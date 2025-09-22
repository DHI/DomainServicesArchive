namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Accounts;

    /// <summary>
    ///     Data transfer object for a personal (me) account resource representation.
    /// </summary>
    [Serializable]
    public class MeDTO
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
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Converts the DTO to an Account object.
        /// </summary>
        public Account ToAccount()
        {
            var account = new Account(Id, Name);
            if (!string.IsNullOrWhiteSpace(Password))
            {
                account.SetPassword(Password);
            }

            account.Company = Company;
            account.PhoneNumber = PhoneNumber;
            account.Email = Email;

            return account;
        }
    }
}
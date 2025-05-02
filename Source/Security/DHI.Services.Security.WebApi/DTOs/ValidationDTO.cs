namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Data transfer object for an account validation resource representation.
    /// </summary>
    [Serializable]
    public class ValidationDTO
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        ///    Gets or sets a one-time password (pin code).
        /// </summary>
        [JsonPropertyName("otp")]
        public string Otp { get; set; }

        /// <summary>
        ///     Get or sets the OTP Authenticator to validate Otp against.
        /// </summary>
        [JsonPropertyName("otpAuthenticator")]
        public string OtpAuthenticator { get; set; }
    }
}
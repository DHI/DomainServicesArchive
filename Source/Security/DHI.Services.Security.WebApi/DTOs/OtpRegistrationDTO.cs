namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Data transfer object for a One-time password (OTP) registration resource representation.
    /// </summary>
    [Serializable]
    public class OtpRegistrationDTO
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
        ///     Gets or sets the OTP Authenticator to register with.
        /// </summary>
        [JsonPropertyName("otpAuthenticator")]
        public string OtpAuthenticator { get; set; }
    }
}
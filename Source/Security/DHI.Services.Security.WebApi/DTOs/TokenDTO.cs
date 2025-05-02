namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Data transfer object for a JWT token resource representation.
    /// </summary>
    [Serializable]
    public class TokenDTO
    {
        /// <summary>
        ///     Gets or sets the JWT token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        ///     Gets or sets the expiration time.
        /// </summary>
        [JsonPropertyName("expiration")]
        public DateTime Expiration { get; set; }
    }
}
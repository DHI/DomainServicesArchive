namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Data transfer object for JWT tokens (access- and refresh-tokens) resource representation.
    /// </summary>
    [Serializable]
    public class TokensDTO
    {
        /// <summary>
        ///     Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("accessToken")]
        public TokenDTO AccessToken { get; set; }

        /// <summary>
        ///     Gets or sets the refresh token.
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public TokenDTO RefreshToken { get; set; }
    }
}
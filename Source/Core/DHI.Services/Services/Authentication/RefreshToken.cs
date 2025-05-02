namespace DHI.Services.Authentication
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public class RefreshToken : BaseEntity<string>
    {
        public RefreshToken(string token, string accountId, DateTime expiration, string clientIp = null)
            : base(string.IsNullOrEmpty(clientIp) ? accountId : $"{accountId}-{clientIp}")
        {
            Guard.Against.NullOrEmpty(token, nameof(token));
            Guard.Against.NullOrEmpty(accountId, nameof(accountId));
            Token = token;
            AccountId = accountId;
            Expiration = expiration;
            ClientIp = clientIp;
        }

        public string Token { get; }

        public string AccountId { get; }

        public DateTime Expiration { get; }

        public string ClientIp { get; }

        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow > Expiration;
    }
}
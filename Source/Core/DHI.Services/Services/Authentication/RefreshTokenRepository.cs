namespace DHI.Services.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     JSON file based refresh token repository.
    /// </summary>
    public class RefreshTokenRepository : JsonRepository<RefreshToken, string>, IRefreshTokenRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public RefreshTokenRepository(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public RefreshTokenRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public RefreshTokenRepository(string filePath,
            System.Text.Json.JsonSerializerOptions serializerOptions,
            System.Text.Json.JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }

        /// <summary>
        ///     Gets all refresh tokens associated with the given account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public IEnumerable<RefreshToken> GetByAccount(string accountId)
        {
            return Get(t => t.AccountId == accountId);
        }

        /// <summary>
        ///     Gets a refresh token with the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        public Maybe<RefreshToken> GetByToken(string token)
        {
            var tokens = Get(t => t.Token == token).ToArray();
            return tokens.Length == 1 ? tokens.Single().ToMaybe() : Maybe.Empty<RefreshToken>();
        }
    }
}
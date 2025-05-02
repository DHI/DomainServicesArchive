namespace DHI.Services.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    /// <summary>
    ///     RefreshToken Service.
    /// </summary>
    public class RefreshTokenService : BaseUpdatableDiscreteService<RefreshToken, string>
    {
        private readonly IRefreshTokenRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public RefreshTokenService(IRefreshTokenRepository repository)
            : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IRefreshTokenRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IRefreshTokenRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard
        ///     (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IRefreshTokenRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Exchanges a valid refresh token for a new refresh token.
        /// </summary>
        /// <param name="token">The current refresh token.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="expirationTimeSpan">The expiration time span.</param>
        /// <param name="clientIp">The client ip.</param>
        public RefreshToken ExchangeRefreshToken(string token, string accountId, TimeSpan expirationTimeSpan, string clientIp = null)
        {
            Guard.Against.NullOrEmpty(accountId, nameof(accountId));
            var id = string.IsNullOrEmpty(clientIp) ? accountId : $"{accountId}-{clientIp}";

            var validRefreshToken = Exists(id) && TryGet(id, out var refreshToken) && refreshToken.Token == token && !refreshToken.IsExpired;
            if (!validRefreshToken)
            {
                throw new ArgumentException("The given refresh token is invalid. Either it does not exist, does not exist for the given account/clientIp or is expired.", nameof(token));
            }

            Remove(id);
            return CreateRefreshToken(accountId, expirationTimeSpan, clientIp);
        }

        /// <summary>
        ///     Creates a new refresh token.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="expirationTimeSpan">The expiration time span.</param>
        /// <param name="clientIp">The client ip.</param>
        public RefreshToken CreateRefreshToken(string accountId, TimeSpan expirationTimeSpan, string clientIp = null)
        {
            var refreshToken = new RefreshToken(GenerateToken(), accountId, DateTime.UtcNow.Add(expirationTimeSpan), clientIp);
            AddOrUpdate(refreshToken);
            return refreshToken;
        }

        /// <summary>
        ///     Gets all refresh tokens associated with the given account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public IEnumerable<RefreshToken> GetByAccount(string accountId)
        {
            return _repository.GetByAccount(accountId);
        }

        /// <summary>
        ///     Gets a refresh token with the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        public RefreshToken GetByToken(string token)
        {
            var maybe = _repository.GetByToken(token);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Refresh token '{token}' is not registered.");
            }

            return maybe.Value;
        }

        /// <summary>
        ///     Determines whether there are any refresh tokens associated with the give account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns><c>true</c> if there are any refresh tokens associated with the give account; otherwise, <c>false</c>.</returns>
        public bool ContainsAccount(string accountId)
        {
            return GetByAccount(accountId).Any();
        }

        /// <summary>
        ///     Removes all refresh tokens associated with the given account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>The number of removed tokens</returns>
        public int RemoveByAccount(string accountId)
        {
            var count = 0;
            foreach (var token in GetByAccount(accountId).ToArray())
            {
                Remove(token.Id);
                count++;
            }

            return count;
        }

        /// <summary>
        ///     Generates a refresh token.
        /// </summary>
        /// <param name="size">The random number size.</param>
        public static string GenerateToken(int size = 32)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
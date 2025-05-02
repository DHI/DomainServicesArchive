namespace DHI.Services.Authentication
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface IRefreshTokenRepository
    /// </summary>
    public interface IRefreshTokenRepository : IRepository<RefreshToken, string>, IDiscreteRepository<RefreshToken, string>, IUpdatableRepository<RefreshToken, string>
    {
        /// <summary>
        ///     Gets all refresh tokens associated with the given account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        IEnumerable<RefreshToken> GetByAccount(string accountId);

        /// <summary>
        ///     Gets a refresh token with the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        Maybe<RefreshToken> GetByToken(string token);
    }
}
namespace DHI.Services.Accounts
{
    using Authentication;

    /// <summary>
    ///     Interface IAccountRepository
    /// </summary>
    public interface IAccountRepository : IAuthenticationProvider, IDiscreteRepository<Account, string>, IUpdatableRepository<Account, string>
    {
        /// <summary>
        ///     Gets an account by token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Maybe&lt;Account&gt;.</returns>
        Maybe<Account> GetByToken(string token);

        /// <summary>
        ///     Gets an account by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Maybe&lt;Account&gt;.</returns>
        Maybe<Account> GetByEmail(string email);
    }
}
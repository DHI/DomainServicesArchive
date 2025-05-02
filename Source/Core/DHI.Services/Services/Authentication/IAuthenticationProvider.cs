namespace DHI.Services.Authentication
{
    using System;
    using System.Threading.Tasks;
    using Accounts;

    /// <summary>
    ///     Interface IAuthenticationProvider
    /// </summary>
    public interface IAuthenticationProvider : IRepository<Account, string>
    {
        /// <summary>
        ///     Validates the password of the account with the given identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if password is correct, <c>false</c> otherwise.</returns>
        Task<bool> ValidatePassword(string accountId, string password);

        /// <summary>
        ///     Unlock account after lock end date passed
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="loginDate">The last login date</param>
        void UnlockAccount(Account account);

        /// <summary>
        ///     Reset no of unsuccessful login attempts
        /// </summary>
        /// <param name="account">The account</param>
        void ResetAccount(Account account, int resetValue);

        /// <summary>
        ///     Lock account for a given period
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="loginDate">The last login date</param>
        /// <param name="noOfloginAttempts">The number of login attempts per given period</param>
        /// <param name="lockPeriod">The lock period</param>
        void LockAccount(Account account, TimeSpan lockPeriod);

    }
}
namespace DHI.Services.Authentication.PasswordHistory
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IPasswordHistoryRepository : IRepository<PasswordHistory, string>, IDiscreteRepository<PasswordHistory, string>, IUpdatableRepository<PasswordHistory, string>
    {
        /// <summary>
        /// Get password history records by account id
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="user">The claims principal</param>
        /// <returns></returns>
        Task<IEnumerable<PasswordHistory>> GetByAccountId(string accountId, ClaimsPrincipal user = null);

        /// <summary>
        /// Get the latest password history record by account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<PasswordHistory> GetMostRecentByAccountId(string accountId, ClaimsPrincipal user = null);

        /// <summary>
        /// Get the last (x) password history records by account id based on password policy
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="limit"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<PasswordHistory>> GetRecentByAccountId(string accountId, int limit, ClaimsPrincipal user = null);

    }
}

namespace DHI.Services.Authentication.PasswordHistory
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class PasswordHistoryRepository : JsonRepository<PasswordHistory, string>, IPasswordHistoryRepository
    {
        public PasswordHistoryRepository(string filePath)
            : base(filePath)
        {

        }

        public PasswordHistoryRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        public PasswordHistoryRepository(string filePath,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }

        /// <summary>
        /// Get password history records by account id
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="user">The claims principal</param>
        /// <returns></returns>
        public async Task<IEnumerable<PasswordHistory>> GetByAccountId(string accountId, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var passwordHistoryResult = Get(query.ToExpression(), user)
                .OrderByDescending(x => x.PasswordExpiryDate).ToList();

            return await Task.FromResult(passwordHistoryResult);
        }

        /// <summary>
        /// Get the latest password history record by account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<PasswordHistory> GetMostRecentByAccountId(string accountId, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var currentPasswordHistory = Get(query.ToExpression(), user)
                .OrderByDescending(x => x.PasswordExpiryDate)
                .FirstOrDefault();

            return await Task.FromResult(currentPasswordHistory);
        }

        /// <summary>
        /// Get the last (x) password history records by account id based on password policy
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="limit"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PasswordHistory>> GetRecentByAccountId(string accountId, int limit, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var lastUsedPasswordsHistory = Get(query.ToExpression(), user)
                .OrderByDescending(x => x.PasswordExpiryDate)
                 .Take(limit).ToList();

            return await Task.FromResult(lastUsedPasswordsHistory);
        }
    }
}

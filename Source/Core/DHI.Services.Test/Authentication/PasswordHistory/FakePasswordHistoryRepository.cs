namespace DHI.Services.Test.Authentication.PasswordHistory
{
    using DHI.Services.Authentication.PasswordHistory;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    internal class FakePasswordHistoryRepository : FakeRepository<PasswordHistory, string>, IPasswordHistoryRepository
    {
        public FakePasswordHistoryRepository()
        {
        }

        public FakePasswordHistoryRepository(IEnumerable<PasswordHistory> userGroups)
            : base(userGroups)
        {
        }

        public Maybe<PasswordHistory> GetByToken(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsPasswordAlreadyUsedAsync(string accountId, string accountPassword, PasswordExpirationPolicy passwordExpirationPolicy, ClaimsPrincipal user = null)
        {
            // Get the PasswordHistory data list by accountId
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };
            var passwordHistoryResult = Get(query.ToExpression())
                .OrderByDescending(x => x.PasswordExpiryDate)
                .Take(passwordExpirationPolicy.PreviousPasswordsReUseLimit)
                .ToList();

            // Check the PasswordHistory data list by accountPassword
            foreach (var passwordHistory in passwordHistoryResult)
            {
                // Seperate this method for a cleaner result
                // Extract salt and hash from the stored encrypted password
                var salt = new byte[16];
                Array.Copy(passwordHistory.EncryptedPassword, 0, salt, 0, 16);

                // Hash the provided password using PBKDF2 with the extracted salt
                var pbkdfs2 = new Rfc2898DeriveBytes(accountPassword, salt, 10000);
                var hash = new byte[20];
                Array.Copy(passwordHistory.EncryptedPassword, 16, hash, 0, 20);

                // Compare the resulting hash with the stored hash
                if (pbkdfs2.GetBytes(20).SequenceEqual(hash))
                {
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }

        public async Task<PasswordHistory> GetMatchingPasswordHistoryAsync(string accountId, string accountPassword, ClaimsPrincipal user = null)
        {
            // Get the PasswordHistory data by accountId
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };
            var passwordHistoryResult = Get(query.ToExpression()).OrderByDescending(x => x.PasswordExpiryDate).FirstOrDefault();

            if (passwordHistoryResult != null)
            {
                // Extract salt and hash from the stored encrypted password
                var salt = new byte[16];
                Array.Copy(passwordHistoryResult.EncryptedPassword, 0, salt, 0, 16);

                // Hash the provided password using PBKDF2 with the extracted salt
                var pbkdfs2 = new Rfc2898DeriveBytes(accountPassword, salt, 10000);
                var hash = new byte[20];
                Array.Copy(passwordHistoryResult.EncryptedPassword, 16, hash, 0, 20);

                // Compare the resulting hash with the stored hash
                if (pbkdfs2.GetBytes(20).SequenceEqual(hash))
                {
                    return await Task.FromResult(passwordHistoryResult);
                }
            }

            return null;
        }

        public async Task<IEnumerable<PasswordHistory>> GetByAccountId(string accountId, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var passwordHistoryResult = Get(query.ToExpression())
                .OrderByDescending(x => x.PasswordExpiryDate).ToList();

            return await Task.FromResult(passwordHistoryResult);
        }

        public async Task<PasswordHistory> GetMostRecentByAccountId(string accountId, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var currentPasswordHistory = Get(query.ToExpression())
                .OrderByDescending(x => x.PasswordExpiryDate)
                .FirstOrDefault();

            return await Task.FromResult(currentPasswordHistory);
        }

        public async Task<IEnumerable<PasswordHistory>> GetRecentByAccountId(string accountId, int limit, ClaimsPrincipal user = null)
        {
            var query = new Query<PasswordHistory>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, accountId)
            };

            var lastUsedPasswordsHistory = Get(query.ToExpression())
                .OrderByDescending(x => x.PasswordExpiryDate)
                 .Take(limit).ToList();

            return await Task.FromResult(lastUsedPasswordsHistory);
        }
    }
}

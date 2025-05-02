namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Accounts;

    internal class FakeAccountRepository : FakeRepository<Account, string>, IAccountRepository
    {
        public FakeAccountRepository()
        {
        }

        public FakeAccountRepository(List<Account> accountList)
            : base(accountList)
        {
        }

        public Maybe<Account> GetByToken(string token)
        {
            var accounts = Entities.Values.Where(a => a.Token == token).ToList();
            return accounts.Count == 1 ? accounts.Single().ToMaybe() : Maybe.Empty<Account>();
        }


        public Maybe<Account> GetByEmail(string email)
        {
            var accounts = Entities.Values.Where(a => a.Email == email).ToList();
            return accounts.Count == 1 ? accounts.Single().ToMaybe() : Maybe.Empty<Account>();
        }

        public Task<bool> ValidatePassword(string accountId, string password)
        {
            var maybe = Get(accountId);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Account with id '{accountId}' was not found.");
            }

            var account = maybe.Value;
            var salt = new byte[16];
            Array.Copy(account.EncryptedPassword, 0, salt, 0, 16);
            var pbkdfs2 = new Rfc2898DeriveBytes(password, salt, 10000);
            var hash = new byte[20];
            Array.Copy(account.EncryptedPassword, 16, hash, 0, 20);
            return Task.FromResult(pbkdfs2.GetBytes(20).SequenceEqual(hash));
        }

        public void UnlockAccount(Account account) { }

        public void ResetAccount(Account account, int resetValue) { }

        public void LockAccount(Account account, TimeSpan lockPeriod) { }
    }
}
namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Accounts;

    internal class FakeAccountRepository : FakeRepository<Account, string>, IAccountRepository
    {
        private readonly LoginAttemptPolicy _loginAttemptPolicy;

        public FakeAccountRepository(LoginAttemptPolicy loginAttemptPolicy = null)
        {
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
        }

        public FakeAccountRepository(List<Account> accountList, LoginAttemptPolicy loginAttemptPolicy = null)
            : base(accountList)
        {
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
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

        public void LockAccount(Account account, TimeSpan lokedPeriod)
        {
            if (Contains(account.Id))
            {
                account.LastLoginAttemptedDate = DateTime.Now;
                account.Locked = true;
                account.LockedDateEnd = DateTime.Now.Add(lokedPeriod);

                Update(account);
            }
        }

        public void ResetAccount(Account account, int resetValue)
        {
            if (Contains(account.Id))
            {
                account.LastLoginAttemptedDate = DateTime.Now;
                account.NoOfUnsuccessfulLoginAttempts = resetValue;

                Update(account);
            }
        }

        public void UnlockAccount(Account account)
        {
            if (Contains(account.Id))
            {
                account.LastLoginAttemptedDate = DateTime.Now;
                account.NoOfUnsuccessfulLoginAttempts = 0;
                account.Locked = false;
                account.LockedDateEnd = null;

                Update(account);
            }
        }
    }
}
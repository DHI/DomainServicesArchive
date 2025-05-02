namespace DHI.Services.Accounts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    ///     JSON Account Repository.
    /// </summary>
    /// <seealso cref="JsonRepository{Account, String}" />
    /// <seealso cref="IAccountRepository" />
    public class AccountRepository : JsonRepository<Account, string>, IAccountRepository
    {
        private readonly LoginAttemptPolicy _loginAttemptPolicy;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="loginAttemptPolicy"></param>
        public AccountRepository(string filePath, LoginAttemptPolicy loginAttemptPolicy = null)
            : base(filePath)
        {
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        /// <param name="loginAttemptPolicy"></param>
        public AccountRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null,
            LoginAttemptPolicy loginAttemptPolicy = null)
            : base(filePath, converters, comparer)
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer for entity</param>
        /// <param name="loginAttemptPolicy"></param>
        public AccountRepository(string filePath,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null,
            LoginAttemptPolicy loginAttemptPolicy = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {

        }

        /// <summary>
        ///     Gets an account by token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Maybe&lt;Account&gt;.</returns>
        public Maybe<Account> GetByToken(string token)
        {
            var accounts = Get(account => account.Token == token).ToList();
            return accounts.Count == 1 ? accounts.Single().ToMaybe() : Maybe.Empty<Account>();
        }

        /// <summary>
        ///     Gets an account by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Maybe&lt;Account&gt;.</returns>
        public Maybe<Account> GetByEmail(string email)
        {
            var accounts = Get(account => account.Email == email).ToList();
            return accounts.Count == 1 ? accounts.Single().ToMaybe() : Maybe.Empty<Account>();
        }

        /// <summary>
        ///     Validates the password of the account with the given identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if password is correct, <c>false</c> otherwise.</returns>
        public Task<bool> ValidatePassword(string accountId, string password)
        {
            var maybe = Get(accountId);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Account with id '{accountId}' was not found.");
            }

            var account = maybe.Value;

            // This is kept for backward compatibility so that existing account repositories using the old hashing mechanism can still be validated
            if (account.EncryptedPassword.Length == 20)
            {
                return Task.FromResult(Account.HashPassword(password).SequenceEqual(account.EncryptedPassword));
            }

            var salt = new byte[16];
            Array.Copy(account.EncryptedPassword, 0, salt, 0, 16);
            var pbkdfs2 = new Rfc2898DeriveBytes(password, salt, 10000);
            var hash = new byte[20];
            Array.Copy(account.EncryptedPassword, 16, hash, 0, 20);
            return Task.FromResult(pbkdfs2.GetBytes(20).SequenceEqual(hash));
        }

        public void LockAccount(Account account, TimeSpan lockPeriod)
        {
            if (Contains(account.Id))
            {
                account.LastLoginAttemptedDate = DateTime.Now;
                account.Locked = true;
                account.LockedDateEnd = DateTime.Now.Add(lockPeriod);

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
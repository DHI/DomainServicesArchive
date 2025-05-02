namespace DHI.Services.Accounts
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    ///     Account Service.
    /// </summary>
    public class AccountService : BaseUpdatableDiscreteService<Account, string>
    {
        private static readonly ConcurrentDictionary<string, int> _delays = new();
        private readonly IAccountRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public AccountService(IAccountRepository repository)
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
            return Service.GetProviderTypes<IAccountRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IAccountRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IAccountRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Adds the specified account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="ArgumentException">Cannot add an account with no password defined.</exception>
        public override void Add(Account account, ClaimsPrincipal user = null)
        {
            if (account.EncryptedPassword == null)
            {
                throw new ArgumentException("Cannot add an account with no password defined.");
            }

            account.Activated = true;
            base.Add(account, user);
        }

        /// <summary>
        ///     Try adding the specified account without existence check.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if account was successfully added, <c>false</c> otherwise.</returns>
        public override bool TryAdd(Account account, ClaimsPrincipal user = null)
        {
            if (account.EncryptedPassword == null)
            {
                throw new ArgumentException("Cannot add an account with no password defined.");
            }

            account.Activated = true;
            return base.TryAdd(account, user);
        }

        /// <summary>
        ///     Adds or updates the specified account.
        /// </summary>
        /// <param name="account">The entity.</param>
        /// <param name="user">The user.</param>
        public override void AddOrUpdate(Account account, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(account.Id, user))
            {
                if (account.EncryptedPassword == null)
                {
                    throw new ArgumentException("Cannot add an account with no password defined.");
                }

                account.Activated = true;
                var cancelEventArgs = new CancelEventArgs<Account>(account);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Add(account, user);
                    OnAdded(account);
                }
            }
            else
            {
                if (!TryGet(account.Id, out var currentAccount))
                {
                    return;
                }

                account.Activated = currentAccount.Activated;
                account.Token = currentAccount.Token;
                account.EncryptedPassword ??= currentAccount.EncryptedPassword;

                var cancelEventArgs = new CancelEventArgs<Account>(account);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(account, user);
                    OnUpdated(account);
                }
            }
        }

        /// <summary>
        ///     Gets the roles.
        /// </summary>
        /// <param name="accountId">ID of the account.</param>
        /// <returns>System.String[].</returns>
        [Obsolete("You should use user-groups instead of roles for authorization. This method will be removed in a future version.")]
        public string[] GetRoles(string accountId)
        {
            return Get(accountId).GetRoles();
        }

        /// <summary>
        ///     Removes the account with the specified identifier.
        /// </summary>
        /// <param name="id">The account identifier.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="ArgumentException">You are not allowed to remove the 'admin' account.</exception>
        public override void Remove(string id, ClaimsPrincipal user = null)
        {
            if (id == "admin")
            {
                throw new ArgumentException("You are not allowed to remove the 'admin' account.", id);
            }

            base.Remove(id, user);
        }

        /// <summary>
        ///     Updates the specified account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="user">The user.</param>
        public override void Update(Account account, ClaimsPrincipal user = null)
        {
            if (!TryGet(account.Id, out var currentAccount))
            {
                throw new KeyNotFoundException($"Account '{account.Id}' was not found.");
            }

            account.Activated = currentAccount.Activated;
            account.Token = currentAccount.Token;
            account.EncryptedPassword ??= currentAccount.EncryptedPassword;

            base.Update(account, user);
        }

        /// <summary>
        ///     Try updating the specified account without existence check.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if account was successfully updated, <c>false</c> otherwise.</returns>
        public override bool TryUpdate(Account account, ClaimsPrincipal user = null)
        {
            try
            {
                if (!TryGet(account.Id, out var currentAccount))
                {
                    return false;
                }

                account.Activated = currentAccount.Activated;
                account.Token = currentAccount.Token;
                account.EncryptedPassword ??= currentAccount.EncryptedPassword;

                var cancelEventArgs = new CancelEventArgs<Account>(account);
                OnUpdating(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                _repository.Update(account, user);
                OnUpdated(account);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Updates me.
        /// </summary>
        /// <param name="meUpdated">Me updated.</param>
        /// <returns>Account.</returns>
        public Account UpdateMe(Account meUpdated)
        {
            if (!TryGet(meUpdated.Id, out var me))
            {
                return default;
            }

            me.Email = meUpdated.Email;
            me.Company = meUpdated.Company;
            me.PhoneNumber = meUpdated.PhoneNumber;
            if (me.AllowMePasswordChange)
            {
                me.EncryptedPassword = meUpdated.EncryptedPassword;
            }

            me.Metadata.Clear();
            foreach (var kvp in meUpdated.Metadata)
            {
                me.Metadata.Add(kvp.Key, kvp.Value);
            }

            Update(me);
            return me;
        }

        /// <summary>
        ///     Validates the specified account.
        /// </summary>
        /// <remarks>
        ///     Checks whether the account exists, the password is correct and the account is activated.
        /// </remarks>
        /// <param name="accountId">ID of the account.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if account exists and password is valid, <c>false</c> otherwise.</returns>
        [Obsolete("You should validate accounts using the dedicated AuthenticationService. This method will be removed in a future version.")]
        public bool Validate(string accountId, string password)
        {
            var account = Get(accountId);
            if (account is null)
            {
                throw new KeyNotFoundException($"Account {accountId} was not found.");
            }

            return account.Activated && account.ValidatePassword(password);
        }

        /// <summary>
        ///     Validates the specified account.
        /// </summary>
        /// <remarks>
        ///     Checks whether the account exists, the password is correct and the account is activated.
        ///     Uses progressive delays in case of failed validation to prevent brute force attacks.
        /// </remarks>
        /// <param name="accountId">ID of the account.</param>
        /// <param name="password">The password.</param>
        /// <param name="clientIp">The IP address of the caller</param>
        /// <returns><c>true</c> if account exists and password is valid, <c>false</c> otherwise.</returns>
        [Obsolete("You should validate accounts using the dedicated AuthenticationService. This method will be removed in a future version.")]
        public async Task<bool> Validate(string accountId, string password, string clientIp)
        {
            if (clientIp is null)
            {
                throw new ArgumentNullException(nameof(clientIp));
            }

            // Wait for delay if there is one
            _delays.TryGetValue(clientIp, out var delay);
            await Task.Delay(delay * 1000);

            var account = Get(accountId);
            if (account is null)
            {
                throw new KeyNotFoundException($"Account {accountId} was not found.");
            }

            var validated = account.Activated && account.ValidatePassword(password);
            if (validated)
            {
                // Reset incremental delay on successful validation
                _delays.TryRemove(clientIp, out _);
                return true;
            }

            // Increment the delay on failed validation
            _delays.TryGetValue(clientIp, out delay);
            if (delay == 0)
            {
                delay = 1;
            }
            else
            {
                delay *= 2;
            }

            _delays[clientIp] = delay;
            return false;
        }
    }
}
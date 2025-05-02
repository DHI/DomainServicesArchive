namespace DHI.Services.Authentication
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Accounts;
    using Microsoft.Extensions.Logging;
    using Notifications;

    /// <summary>
    ///     Authentication Service.
    /// </summary>
    public class AuthenticationService : BaseService<Account, string>
    {
        private static readonly ConcurrentDictionary<string, int> _delays = new();
        private readonly IAuthenticationProvider _provider;
        //private readonly ILogger _logger;
        public LoginAttemptPolicy _loginAttemptPolicy;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="provider">The authentication provider for validating username and password.</param>
        /// <param name="loginAttemptPolicy"></param>
        public AuthenticationService(IAuthenticationProvider provider, LoginAttemptPolicy loginAttemptPolicy = null) : base(provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
        }

        /// <summary>
        ///     Validates the specified account.
        /// </summary>
        /// <remarks>
        ///     Checks whether the account exists, the password is correct and the account is activated.
        ///     Uses progressive delays in case of failed validation to prevent brute force attacks.
        ///     Failed validation is logged, if a logger is injected from the service constructor.
        ///     Also comes up with login attemps condition.
        ///     Failed validation is logged, if a logger is injected from the service constructor.
        /// </remarks>
        /// <param name="accountId">ID of the account.</param>
        /// <param name="password">The password.</param>
        /// <param name="clientIp">The IP address of the caller</param>
        /// <returns><c>true</c> if account exists and password is valid, <c>false</c> otherwise.</returns>
        public async Task<bool> Validate(string accountId, string password, string clientIp)
        {
            if (clientIp is null)
            {
                throw new ArgumentNullException(nameof(clientIp));
            }

            try
            {
                if (!TryGet(accountId, out var account))
                {
                    _logger?.LogWarning("User account '{AccountId}' from client IP '{ClientIp}' not found.", accountId, clientIp);
                    return false;
                }

                var passwordValidated = await _provider.ValidatePassword(accountId, password);

                // Condition when activate Account have a wrong password & not have been locked
                if (!passwordValidated && account.Activated && !account.Locked)
                {
                    // Condition when time interval between login tries with last login date is equal or less than a the settings
                    if ((DateTime.Now - account.LastLoginAttemptedDate) <= _loginAttemptPolicy.ResetInterval)
                    {
                        account.NoOfUnsuccessfulLoginAttempts += 1;

                        if (account.NoOfUnsuccessfulLoginAttempts == _loginAttemptPolicy.MaxNumberOfLoginAttempts)
                        {
                            _provider.LockAccount(account, _loginAttemptPolicy.LockedPeriod);
                            _logger?.LogError($"User Account '{accountId}' from client IP '{clientIp}' has been locked.", nameof(Validate));
                        }
                        else
                        {
                            ((IAccountRepository)_provider).Update(account);
                        }
                    }
                    else
                    {
                        // Condition when time interval between login tries with last login date is not equal or more than the configuration.
                        // So, if the login is failed and exceed the configured time interval, it'll reset to attempt 1
                        _provider.ResetAccount(account, 1);
                    }
                }

                // Condition when activate Account have a right password but has been locked
                if (passwordValidated && account.Activated && account.Locked)
                {
                    if (account.LastLoginAttemptedDate >= account.LockedDateEnd)
                    {
                        _provider.UnlockAccount(account);
                        _logger?.LogError($"User Account '{accountId}' from client IP '{clientIp}' has been unlocked.", nameof(Validate));
                    }
                }

                // user provided correct password to an active account and it is not locked out
                if (passwordValidated && account.Activated && !account.Locked)
                {
                    // Condition when user provided correct password to an active account and it is not locked out.
                    // So, if the login is success, it'll reset to attempt 0
                    _provider.ResetAccount(account, 0);

                    // Reset incremental delay on successful validation
                    _delays.TryRemove(clientIp, out _);
                    return true;
                }

            }
            catch (Exception)
            {
                // Deliberately swallow any exception thrown by password validator (e.g. account not found)
            }

            _logger?.LogWarning("Validation of user account '{AccountId}' from client IP '{ClientIp}' failed.", accountId, clientIp);

            // Wait for delay if there is one
            _delays.TryGetValue(clientIp, out var delay);
            await Task.Delay(delay * 1000);

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

        /// <summary>
        ///     Validates the specified account.
        /// </summary>
        /// <remarks>
        ///     Checks whether the account exists, the password is correct and the account is activated.
        /// </remarks>
        /// <param name="accountId">ID of the account.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if account exists and password is valid, <c>false</c> otherwise.</returns>
        [Obsolete("You should use the Validate method supporting progressive delays in case of failed validation. This method will be removed in a future version.")]
        public async Task<bool> Validate(string accountId, string password)
        {
            return await _provider.ValidatePassword(accountId, password) && Get(accountId).Activated;
        }

        /// <summary>
        ///     Gets the compatible authentication provider types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetAuthenticationProviderTypes()
        {
            return Service.GetProviderTypes<IAuthenticationProvider>();
        }

        /// <summary>
        ///     Gets the compatible authentication provider types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetAuthenticationProviderTypes(string path)
        {
            return Service.GetProviderTypes<IAuthenticationProvider>(path);
        }

        /// <summary>
        ///     Gets the compatible authentication provider types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard
        ///     (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetAuthenticationProviderTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IAuthenticationProvider>(path, searchPattern);
        }

    }
}
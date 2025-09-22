namespace DHI.Services.Authentication.PasswordHistory
{
    using DHI.Services.Accounts;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class PasswordHistoryService : BaseUpdatableDiscreteService<PasswordHistory, string>
    {
        private readonly IPasswordHistoryRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordExpirationPolicy _passwordExpirationPolicy;
        private readonly AccountService _accountService;
        private readonly ILogger _logger;
        private static readonly ConcurrentDictionary<string, int> _delays = new();

        public PasswordHistoryService(IPasswordHistoryRepository repository, PasswordExpirationPolicy passwordExpirationPolicy = null)
            : base(repository)
        {
            _repository = repository;
            _passwordExpirationPolicy = passwordExpirationPolicy ?? new PasswordExpirationPolicy();
        }

        public PasswordHistoryService(IPasswordHistoryRepository repository,
            ILogger logger,
            IAccountRepository accountRepository = null,
            PasswordExpirationPolicy passwordExpirationPolicy = null) : base(repository, logger)
        {
            _repository = repository;
            _logger = logger;
            _accountRepository = accountRepository;
            _passwordExpirationPolicy = passwordExpirationPolicy ?? new PasswordExpirationPolicy();
            _accountService = accountRepository != null ? new AccountService(accountRepository) : null;
        }

        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IPasswordHistoryRepository>();
        }

        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IPasswordHistoryRepository>(path);
        }

        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IPasswordHistoryRepository>(path, searchPattern);
        }

        /// <summary>
        /// Validate the current password and check whether it is expired
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="password">The password</param>
        /// <param name="loginDate">The login date</param>
        /// <param name="clientIp">The client ip</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> ValidateCurrentPassword(string accountId, string password, DateTime loginDate, string clientIp)
        {
            if (clientIp is null)
            {
                throw new ArgumentNullException(nameof(clientIp));
            }

            try
            {
                var currentPasswordHistory = await GetCurrentPasswordHistoryAsync(accountId, password);
                if (currentPasswordHistory != null)
                {
                    if (currentPasswordHistory.PasswordExpiryDate > loginDate)
                    {
                        // Reset incremental delay on successful validation
                        _delays.TryRemove(clientIp, out _);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Deliberately swallow any exception thrown by password validator (e.g. account not found)
            }

            _logger?.LogError($"Validation of user account password history '{accountId}' from client IP '{clientIp}' failed.", nameof(ValidateCurrentPassword));

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
        /// Added password history record after Account Registration or Updates
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountChangePassword"></param>
        /// <param name="addDate"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<PasswordHistory> AddPasswordHistoryAsync(Account account, string accountChangePassword, DateTime addDate, ClaimsPrincipal user = null)
        {
            var activePasswordMatch = await GetCurrentPasswordHistoryAsync(account.Id, accountChangePassword, user);

            if (activePasswordMatch == null)
            {
                var isPasswordAlreadyUsed = await IsPasswordAlreadyUsedAsync(account.Id, accountChangePassword, user);

                if (isPasswordAlreadyUsed)
                {
                    throw new ArgumentException("Cannot use the same password as before");
                }

                activePasswordMatch = new PasswordHistory(Guid.NewGuid().ToString())
                {
                    AccountId = account.Id,
                    EncryptedPassword = account.EncryptedPassword,
                    PasswordExpiryDate = addDate.AddDays(_passwordExpirationPolicy.PasswordExpiryDurationInDays),
                    Added = addDate
                };

                _repository.Add(activePasswordMatch, user);
            }

            return activePasswordMatch;
        }

        /// <summary>
        /// Checks whether the password is already used before according to the policy
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="password">The account password</param>
        /// <param name="user">The claims principal</param>
        /// <returns></returns>
        public async Task<bool> IsPasswordAlreadyUsedAsync(string accountId, string password, ClaimsPrincipal user = null)
        {
            var lastUsedPasswordsHistory = await _repository.GetRecentByAccountId(accountId, _passwordExpirationPolicy.PreviousPasswordsReUseLimit, user);

            foreach (var passwordHistory in lastUsedPasswordsHistory)
            {
                var isPasswordMatch = PasswordHistoryComparer.Comparer(passwordHistory.EncryptedPassword, password);

                if (isPasswordMatch)
                {
                    _logger?.LogError($"Password has already been used before", password);
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Get password history record of the current password
        /// </summary>
        /// <param name="accountId">The account Id</param>
        /// <param name="password">The password</param>
        /// <param name="user">The claims principal</param>
        /// <returns></returns>
        public async Task<PasswordHistory> GetCurrentPasswordHistoryAsync(string accountId, string password, ClaimsPrincipal user = null)
        {
            var currentPasswordHistory = await _repository.GetMostRecentByAccountId(accountId);

            if (currentPasswordHistory != null)
            {
                var isPasswordMatch = PasswordHistoryComparer.Comparer(currentPasswordHistory.EncryptedPassword, password);

                if (isPasswordMatch)
                {
                    return await Task.FromResult(currentPasswordHistory);
                }
            }

            return null;
        }
    }
}

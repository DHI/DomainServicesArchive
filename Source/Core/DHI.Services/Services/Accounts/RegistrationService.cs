namespace DHI.Services.Accounts
{
    using System;
    using System.Linq;
    using System.Net.Mail;
    using DHI.Services.Authentication.PasswordHistory;
    using Mails;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class RegistrationService.
    /// </summary>
    public class RegistrationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AccountService _accountService;
        private readonly MailTemplate _activationMailTemplate;
        private readonly IMailSender _mailSender;
        private readonly MailTemplate _passwordResetMailTemplate;
        private readonly TimeSpan _tokenLifeTime;
        private readonly IPasswordHistoryRepository _passwordHistoryRepository;
        private readonly PasswordHistoryService _passwordHistoryService;
        private readonly PasswordExpirationPolicy _passwordExpirationPolicy;
        private readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationService" /> class.
        /// </summary>
        /// <param name="accountRepository">The account repository.</param>
        /// <param name="mailSender">Mail sender</param>
        /// <param name="activationEmailTemplate">Mail template for activation mails</param>
        /// <param name="passwordResetEmailTemplate">Mail template for password reset mails</param>
        /// <param name="tokenLifeTime">The lifetime of the activation token</param>
        /// <param name="passwordHistoryRepository"></param>
        /// <param name="passwordExpirationPolicy"></param>
        /// <param name="logger"></param>
        public RegistrationService(IAccountRepository accountRepository, IMailSender mailSender,
            MailTemplate activationEmailTemplate, MailTemplate passwordResetEmailTemplate, TimeSpan tokenLifeTime = default,
            IPasswordHistoryRepository passwordHistoryRepository = null,
            PasswordExpirationPolicy passwordExpirationPolicy = null,
            ILogger logger = null)
        {
            _mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
            _activationMailTemplate = activationEmailTemplate ?? throw new ArgumentNullException(nameof(activationEmailTemplate));
            _passwordResetMailTemplate = passwordResetEmailTemplate ?? throw new ArgumentNullException(nameof(passwordResetEmailTemplate));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _accountService = new AccountService(accountRepository);
            _tokenLifeTime = tokenLifeTime == default ? TimeSpan.FromDays(1) : tokenLifeTime;
            _logger = logger;
            _passwordExpirationPolicy = passwordExpirationPolicy ?? new PasswordExpirationPolicy();
            _passwordHistoryService = passwordHistoryRepository != null && _passwordExpirationPolicy != null
                ? new PasswordHistoryService(passwordHistoryRepository, _logger, accountRepository, _passwordExpirationPolicy)
                : _passwordHistoryService;
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IAccountRepository>(path);
        }

        /// <summary>
        ///     Registers the specified user account, prepares an account activation by generating a temporary activation token and
        ///     sends an activation email.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="activationUri">The base URI for activation</param>
        /// <returns>Account.</returns>
        /// <exception cref="ArgumentException">account</exception>
        public Account Register(Account account, string activationUri)
        {
            if (!Mail.IsValidEmail(account.Email))
            {
                throw new ArgumentException($"Email '{account.Email}' of account '{account}' is not a valid email.", nameof(account));
            }

            account.Activated = false;
            account.Roles = "Guest, User";
            account.Token = CalculateNonce();
            account.TokenExpiration = DateTime.Now + _tokenLifeTime;
            _accountService.Add(account);

            var from = new MailAddress(_activationMailTemplate.From, _activationMailTemplate.FromDisplayName);
            var to = new MailAddress(account.Email, account.Name);
            var activationLink = $"{activationUri}?token={account.Token}";

            var body = string.Format(_activationMailTemplate.Body, account.Name, activationLink);
            var mailMessage = new MailMessage(from, to)
            {
                Subject = _activationMailTemplate.Subject,
                Body = body,
                IsBodyHtml = true
            };

            _mailSender.Send(mailMessage);

            return account;
        }

        /// <summary>
        ///     Activates the account using a temporary activation token.
        /// </summary>
        /// <param name="activationToken">The activation token.</param>
        /// <returns><c>true</c> if activation is successful, <c>false</c> otherwise.</returns>
        public bool Activate(string activationToken)
        {
            var maybe = _accountRepository.GetByToken(activationToken);
            if (!maybe.HasValue)
            {
                return false;
            }

            var account = maybe.Value;
            if (account.TokenExpiration.HasValue && DateTime.Now > account.TokenExpiration.Value)
            {
                return false;
            }

            if (account.Activated)
            {
                return true;
            }

            account.Token = null;
            account.TokenExpiration = null;
            account.Activated = true;
            _accountService.Update(account);
            return true;
        }

        /// <summary>
        ///     Prepares a password reset by generating a reset token and sending a password reset mail.
        /// </summary>
        /// <param name="id">Either the account ID or the account email.</param>
        /// <param name="resetUri">The base URI for password reset.</param>
        /// <param name="mailBodyName">The name of the email body to send.</param>
        /// <returns>Maybe&lt;Account&gt;.</returns>
        public Maybe<Account> ResetPassword(string id, string resetUri, string mailBodyName)
        {
            var maybe = _accountRepository.Get(id);
            if (!maybe.HasValue)
            {
                maybe = _accountRepository.GetByEmail(id);
                if (!maybe.HasValue)
                {
                    return maybe;
                }
            }

            var account = maybe.Value;
            if (!account.AllowMePasswordChange)
            {
                throw new InvalidOperationException($"You are not allowed to change the password for the account '{account}'.");
            }

            account.Token = CalculateNonce();
            account.TokenExpiration = DateTime.Now + _tokenLifeTime;
            _accountService.Update(account);

            var from = new MailAddress(_passwordResetMailTemplate.From, _passwordResetMailTemplate.FromDisplayName);
            var to = new MailAddress(account.Email, account.Name);
            var resetLink = $"{resetUri}?token={account.Token}";

            var mailBody = mailBodyName.Equals("default")
                ? _passwordResetMailTemplate.Body
                : _passwordResetMailTemplate.Bodies.ContainsKey(mailBodyName)
                    ? _passwordResetMailTemplate.Bodies[mailBodyName]
                    : throw new ArgumentOutOfRangeException(nameof(mailBodyName), mailBodyName, $"Mail template bodies does not include a definition for '{mailBodyName}'; Available bodies are {_passwordResetMailTemplate.Bodies.Aggregate("", (c, n) => $"{c}{n.Key},").TrimEnd(',')}.");

            var body = string.Format(mailBody, account.Name, resetLink, account.Token);
            var mailMessage = new MailMessage(from, to)
            {
                Subject = _passwordResetMailTemplate.Subject,
                Body = body,
                IsBodyHtml = true
            };

            _mailSender.Send(mailMessage);

            return account.ToMaybe();
        }

        /// <summary>
        ///     Updates the account password using a temporary reset token.
        /// </summary>
        /// <param name="token">The reset token.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if password update is successful, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Password cannot be null or empty. - password</exception>
        public bool UpdatePassword(string token, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            var maybe = _accountRepository.GetByToken(token);
            if (!maybe.HasValue)
            {
                return false;
            }

            var account = maybe.Value;
            if (account.TokenExpiration.HasValue && DateTime.Now > account.TokenExpiration.Value)
            {
                return false;
            }

            account.Token = null;
            account.TokenExpiration = null;
            account.SetPassword(password);

            // Get Service Method to Add PasswordHistory
            if (_passwordHistoryService != null)
            {
                var passwordHistory = _passwordHistoryService.AddPasswordHistoryAsync(account, password, DateTime.Now);
                if (passwordHistory == null)
                {
                    throw new ArgumentException(passwordHistory.AsString(), nameof(password));
                }
            }

            _accountService.Update(account);

            return true;
        }

        /// <summary>
        ///     Calculates the nonce.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string CalculateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        ///     Class AccountService.
        /// </summary>
        /// <seealso cref="BaseUpdatableService{Account, String}" />
        private class AccountService : BaseUpdatableService<Account, string>
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AccountService" /> class.
            /// </summary>
            /// <param name="repository">The repository.</param>
            public AccountService(IAccountRepository repository) : base(repository)
            {
            }
        }
    }
}
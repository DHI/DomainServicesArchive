namespace DHI.Services.Security.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Accounts;
    using Authorization;
    using DHI.Services.Authentication.PasswordHistory;
    using DTOs;
    using Mails;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using PwnedPasswords.Client;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Accounts API
    /// </summary>
    [Produces("application/json")]
    [Route("api/accounts")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing accounts, including account registration and activation.")]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AccountService _accountService;
        private readonly UserGroupService _userGroupService;
        private readonly RegistrationService _registrationService;
        private readonly IPwnedPasswordsClient _pwnedPasswordsClient;
        private readonly PasswordPolicy _passwordPolicy;
        private readonly LoginAttemptPolicy _loginAttemptPolicy;
        private readonly PasswordHistoryService _passwordHistoryService;
        private readonly PasswordExpirationPolicy _passwordExpirationPolicy;

        public AccountsController(IConfiguration configuration,
            IAccountRepository accountRepository,
            IUserGroupRepository userGroupRepository,
            IPwnedPasswordsClient pwnedPasswordsClient = null,
            PasswordPolicy passwordPolicy = null,
            IMailTemplateRepository mailTemplateRepository = null,
            LoginAttemptPolicy loginAttemptPolicy = null,
            IPasswordHistoryRepository passwordHistoryRepository = null,
            PasswordExpirationPolicy passwordExpirationPolicy = null,
            ILogger logger = null)
        {
            _configuration = configuration;
            _accountService = new AccountService(accountRepository);
            _userGroupService = new UserGroupService(userGroupRepository);
            _accountService.Deleted += (_, args) => { _userGroupService.RemoveUser(args.Item); };
            _pwnedPasswordsClient = pwnedPasswordsClient;
            _passwordPolicy = passwordPolicy;
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
            if (mailTemplateRepository == null)
            {
                return;
            }

            var mailTemplateService = new MailTemplateService(mailTemplateRepository);
            var smtpSetCredentials = _configuration.GetMandatoryValue<bool>("Registration:SmtpSetCredentials");
            var smtpHost = _configuration.GetMandatoryValue<string>("Registration:SmtpHost");
            var smtpPort = _configuration.GetMandatoryValue<int>("Registration:SmtpPort");
            IMailSender mailSender;
            if (smtpSetCredentials)
            {
                var smtpUsername = _configuration.GetMandatoryValue<string>("Registration:SmtpUsername");
                var smtpPassword = _configuration.GetMandatoryValue<string>("Registration:SmtpPassword");
                mailSender = new SmtpMailSender(smtpHost, smtpPort, smtpUsername, smtpPassword);
            }
            else
            {
                mailSender = new SmtpMailSender(smtpHost, smtpPort);
            }

            _ = mailTemplateService.TryGet("AccountActivation", out var activationMailTemplate);
            _ = mailTemplateService.TryGet("PasswordReset", out var passwordResetMailTemplate);
            var tokenLifeTime = _configuration.GetMandatoryValue<TimeSpan>("Registration:TokenLifeTime");
            _passwordExpirationPolicy = passwordExpirationPolicy ?? new PasswordExpirationPolicy();
            _passwordHistoryService = passwordHistoryRepository is null ?
                null :
                new PasswordHistoryService(passwordHistoryRepository, logger, accountRepository, _passwordExpirationPolicy);
            _registrationService = new RegistrationService(accountRepository,
                mailSender,
                activationMailTemplate,
                passwordResetMailTemplate,
                tokenLifeTime,
                passwordHistoryRepository,
                _passwordExpirationPolicy,
                logger);

        }

        /// <summary>
        ///     Activates an account using the specified activation token.
        /// </summary>
        /// <remarks>
        ///     The necessary activation token must be obtained from e.g. an email or a mobile text message.
        /// </remarks>
        /// <response code="400">Incorrect or invalidated activation token</response>
        /// <param name="token">The activation token.</param>
        [HttpPut("activation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public IActionResult Activate(string token)
        {
            if (_registrationService is null)
            {
                throw new Exception("Account registration is not activated.");
            }

            var activated = _registrationService.Activate(token);
            return activated ? Ok() : BadRequest("Incorrect or invalidated activation token");
        }

        /// <summary>
        ///     Adds a new account.
        /// </summary>
        /// <remarks>
        ///     Adding a new account automatically activates the account.
        /// </remarks>
        /// <param name="accountDTO">Account body.</param>
        /// <response code="201">Created</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [Authorize(Policy = "AdministratorsOnly")]
        public async Task<ActionResult<AccountDTO>> Add([FromBody] AccountDTO accountDTO)
        {
            if (_pwnedPasswordsClient != null)
            {
                if (await _pwnedPasswordsClient.HasPasswordBeenPwned(accountDTO.Password))
                {
                    return BadRequest("The given password has been breached (https://haveibeenpwned.com/Passwords). Please use another password.");
                }
            }

            if (_passwordPolicy != null)
            {
                var result = await _passwordPolicy.ValidateAsync(accountDTO.Password);
                if (!result.Success)
                {
                    return BadRequest(result.AsString());
                }
            }

            var user = HttpContext.User;
            var account = accountDTO.ToAccount();

            if (_passwordHistoryService != null)
            {
                var passwordHistory = await _passwordHistoryService.AddPasswordHistoryAsync(account, accountDTO.Password, DateTime.Now, user);
                if (passwordHistory == null)
                {
                    return BadRequest(passwordHistory.AsString());
                }
            }

            _accountService.Add(account, user);
            if (accountDTO.UserGroups != null)
            {
                foreach (var userGroup in accountDTO.UserGroups)
                {
                    _userGroupService.AddUser(userGroup, account.Id, user);
                }
            }

            return CreatedAtAction(nameof(Get), new { id = account.Id }, AccountDTO.FromAccount(account, accountDTO.UserGroups));
        }

        /// <summary>
        ///     Updates an existing account.
        /// </summary>
        /// <remarks>
        ///     The password can be omitted from an update, but all other properties must be present. Otherwise, they will be set
        ///     to null.
        /// </remarks>
        /// <response code="404">Account not found</response>
        /// <param name="accountUpdateDTO">Updated account body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [Authorize(Policy = "AdministratorsOnly")]
        public async Task<ActionResult<AccountDTO>> Update([FromBody] AccountUpdateDTO accountUpdateDTO)
        {
            if (!string.IsNullOrEmpty(accountUpdateDTO.Password))
            {
                if (_pwnedPasswordsClient != null)
                {
                    if (await _pwnedPasswordsClient.HasPasswordBeenPwned(accountUpdateDTO.Password))
                    {
                        return BadRequest(
                            "The given password has been breached (https://haveibeenpwned.com/Passwords). Please use another password.");
                    }
                }

                if (_passwordPolicy != null)
                {
                    var result = await _passwordPolicy.ValidateAsync(accountUpdateDTO.Password);
                    var json = result.AsString(); //Result As String
                    if (!result.Success)
                    {
                        return BadRequest(json);
                    }
                }
            }

            var user = HttpContext.User;
            var account = accountUpdateDTO.ToAccount();

            if (_passwordHistoryService != null)
            {
                var passwordHistory = await _passwordHistoryService.AddPasswordHistoryAsync(account, accountUpdateDTO.Password, DateTime.Now, user);
                if (passwordHistory == null)
                {
                    return BadRequest(passwordHistory.AsString());
                }
            }

            _accountService.Update(account, user);
            if (accountUpdateDTO.UserGroups != null)
            {
                UpdateUserGroups(accountUpdateDTO, account, user);
            }

            _ = _accountService.TryGet(account.Id, out var updatedAccount);
            return Ok(AccountDTO.FromAccount(updatedAccount, accountUpdateDTO.UserGroups));
        }

        /// <summary>
        ///     Deletes the account with the specified identifier.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Account not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdministratorsOnly")]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            _accountService.Remove(id, user);
            return NoContent();
        }

        /// <summary>
        ///     Gets an account with the specified identifier.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <response code="404">Account not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdministratorsOnly")]
        public ActionResult<AccountDTO> Get(string id)
        {
            var user = HttpContext.User;
            if (!_accountService.TryGet(id, out var account, user))
            {
                return NotFound();
            }

            var groups = _userGroupService.GetIds(account.Id, user).ToArray();
            return Ok(new AccountDTO(account, groups));
        }

        /// <summary>
        ///     Gets a list of all accounts.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Policy = "AdministratorsOnly")]
        public ActionResult<IEnumerable<AccountDTO>> GetAll()
        {
            var user = HttpContext.User;
            var dtos = new List<AccountDTO>();
            var groups = _userGroupService.GetAll(user).ToArray();
            foreach (var account in _accountService.GetAll(user))
            {
                var userGroups = groups.Where(group => group.Users.Contains(account.Id)).Select(u => u.Id);
                dtos.Add(new AccountDTO(account, userGroups.ToArray()));
            }

            return Ok(dtos);
        }

        /// <summary>
        ///     Gets the total number of accounts.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Policy = "AdministratorsOnly")]
        public IActionResult GetCount()
        {
            var user = HttpContext.User;
            return Ok(_accountService.Count(user));
        }

        /// <summary>
        ///     Performs an account registration.
        /// </summary>
        /// <remarks>
        ///     An account registration must be followed by an account activation before the account is activated and valid. A
        ///     one-time activation token is generated and an activation link with the generated reset token is sent to the user
        ///     account in an email.
        /// </remarks>
        /// <response code="202">Account registration accepted</response>
        /// <param name="registrationDTO">The registration body.</param>
        [HttpPost("registration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> Register([FromBody] RegistrationDTO registrationDTO)
        {
            if (_registrationService is null)
            {
                throw new Exception("Account registration is not activated.");
            }

            if (_pwnedPasswordsClient != null)
            {
                if (await _pwnedPasswordsClient.HasPasswordBeenPwned(registrationDTO.Password))
                {
                    return BadRequest("The given password has been breached (https://haveibeenpwned.com/Passwords). Please use another password.");
                }
            }

            if (_passwordPolicy != null)
            {
                var result = await _passwordPolicy.ValidateAsync(registrationDTO.Password);
                if (!result.Success)
                {
                    return BadRequest(result.AsString());
                }
            }

            var account = registrationDTO.ToAccount();
            _registrationService.Register(account, _configuration.GetMandatoryValue<string>("Registration:AccountActivationUri"));
            return Accepted();
        }

        /// <summary>
        ///     Modifies the account of the calling user.
        /// </summary>
        /// <remarks>
        ///     A user can modify his own Password as well as the Email, PhoneNumber and Company properties.
        ///     If any of these properties are omitted in the body, they will be set to null.
        ///     Other account properties cannot be modified.
        /// </remarks>
        /// <response code="403">Forbidden. This endpoint is only to update calling users own account.</response>
        /// <param name="meDTO">Me body.</param>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [Authorize]
        public async Task<ActionResult<AccountDTO>> UpdateMe([FromBody] MeDTO meDTO)
        {
            var user = HttpContext.User;
            if (user.GetUserId() != meDTO.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, $"Cannot update the account of '{meDTO.Id}'");
            }

            if (!string.IsNullOrEmpty(meDTO.Password))
            {
                if (_pwnedPasswordsClient != null)
                {
                    if (await _pwnedPasswordsClient.HasPasswordBeenPwned(meDTO.Password))
                    {
                        return BadRequest(
                            "The given password has been breached (https://haveibeenpwned.com/Passwords). Please use another password.");
                    }
                }

                if (_passwordPolicy != null)
                {
                    var result = await _passwordPolicy.ValidateAsync(meDTO.Password);
                    if (!result.Success)
                    {
                        return BadRequest(result.AsString());
                    }
                }
            }

            var account = meDTO.ToAccount(); // Password is Encrypted in here

            if (_passwordHistoryService != null)
            {
                var passwordHistory = await _passwordHistoryService.AddPasswordHistoryAsync(account, meDTO.Password, DateTime.Now, user);
                if (passwordHistory == null)
                {
                    return BadRequest(passwordHistory.AsString());
                }
            }

            _accountService.UpdateMe(account);


            if (!_accountService.TryGet(account.Id, out var updatedAccount, user))
            {
                return NotFound();
            }

            return Ok(meDTO);
        }

        /// <summary>
        ///     Gets the account of the calling user.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<AccountDTO> GetMe()
        {
            var user = HttpContext.User;
            var userId = user.GetUserIdFromAnyClaim();

            if (userId is null) return Unauthorized();

            if (!_accountService.TryGet(userId, out var account, user))
            {
                return NotFound();
            }

            var groups = _userGroupService.GetIds(account.Id, user).ToArray();
            return Ok(new AccountDTO(account, groups));
        }

        /// <summary>
        ///     Gets the password policy.
        /// </summary>
        [HttpGet("passwordpolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<PasswordPolicy> GetPasswordPolicy()
        {
            if (_passwordPolicy is null)
            {
                throw new Exception("No password policy is registered.");
            }

            return Ok(_passwordPolicy);
        }

        /// <summary>
        ///     Generates a one-time token that allows for a password reset.
        /// </summary>
        /// <remarks>
        ///     Generates a one-time token that allows for a password reset for the account identified by the given email address
        ///     or account ID. A password reset link with the generated reset token is sent to the user account in an email.
        /// </remarks>
        /// <param name="id">The email address or account ID.</param>
        /// <param name="mailBody">The key in the <see cref="MailTemplate.Bodies"/> to serve different templates, default is "default" to send <see cref="MailTemplate.Body"/> </param>
        /// <response code="202">Password reset accepted</response>
        /// <response code="404">Account not found</response>
        [HttpPost("passwordreset")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ResetPassword([FromBody] string id, [FromQuery] string mailBody = "default")
        {
            if (_registrationService is null)
            {
                throw new Exception("Account registration is not activated.");
            }

            var maybe = _registrationService.ResetPassword(id, _configuration.GetMandatoryValue<string>("Registration:PasswordResetUri"), mailBody);
            return maybe.HasValue ? Accepted() : NotFound();
        }

        /// <summary>
        ///     Updates the password for the user account associated with the given token.
        /// </summary>
        /// <remarks>
        ///     The necessary password reset token must be obtained from e.g. an email or a mobile text message.
        /// </remarks>
        /// <param name="token">The token.</param>
        /// <param name="password">The password.</param>
        /// <response code="400">Incorrect or invalidated token</response>
        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePassword(string token, [FromBody] string password)
        {
            if (_registrationService is null)
            {
                throw new Exception("Account registration is not activated.");
            }

            if (!string.IsNullOrEmpty(password))
            {
                if (_pwnedPasswordsClient != null)
                {
                    if (await _pwnedPasswordsClient.HasPasswordBeenPwned(password))
                    {
                        return BadRequest("The given password has been breached (https://haveibeenpwned.com/Passwords). Please use another password.");
                    }
                }

                if (_passwordPolicy != null)
                {
                    var result = await _passwordPolicy.ValidateAsync(password);
                    if (!result.Success)
                    {
                        return BadRequest(result.AsString());
                    }
                }
            }

            var updated = _registrationService.UpdatePassword(token, password);
            return updated ? Ok() : BadRequest("Incorrect or invalidated token");
        }

        /// <summary>
        ///     Update User Groups by providing a base account DTO and account Id.
        /// </summary>
        /// <param name="accountBaseDTO"></param>
        /// <param name="account"></param>
        /// <param name="user"></param>
        private void UpdateUserGroups(AccountBaseDTO accountBaseDTO, Account account, ClaimsPrincipal user)
        {
            var userGroups = _userGroupService.GetIds(account.Id, user);

            // Check User Request Admin Role
            if (accountBaseDTO.UserGroups.Contains("Administrators"))
            {
                // Check User Has Admin Role
                if (userGroups.Contains("Administrators"))
                {
                    // -> Remove existing user groups
                    foreach (var userGroup in userGroups)
                    {
                        _userGroupService.RemoveUser(userGroup, account.Id, user);
                    }

                    if (accountBaseDTO.UserGroups.Any())
                    {
                        // -> Add new user groups
                        foreach (var userGroup in accountBaseDTO.UserGroups)
                        {
                            _userGroupService.AddUser(userGroup, account.Id, user);
                        }
                    }
                }
                else
                {
                    // Produce error result
                    throw new Exception($"Cannot add Administrators privilage for the account of '{account.Id}'.");
                }
            }
            else
            {
                // -> Remove existing user groups
                foreach (var userGroup in userGroups)
                {
                    _userGroupService.RemoveUser(userGroup, account.Id, user);
                }

                if (accountBaseDTO.UserGroups.Any())
                {
                    // -> Add new user groups
                    foreach (var userGroup in accountBaseDTO.UserGroups)
                    {
                        _userGroupService.AddUser(userGroup, account.Id, user);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the Login Attempt Policy.
        /// </summary>
        [HttpGet("loginattemptpolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<LoginAttemptPolicy> GetLoginAttemptPolicy()
        {
            if (_loginAttemptPolicy is null)
            {
                throw new Exception("Login Attempt Policy is not registered.");
            }

            return Ok(_loginAttemptPolicy);
        }
    }
}
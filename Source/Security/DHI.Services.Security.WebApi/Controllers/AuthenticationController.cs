namespace DHI.Services.Security.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Accounts;
    using Authentication;
    using Authentication.Otp;
    using Authorization;
    using DHI.Services.Authentication.PasswordHistory;
    using DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Authentication API.
    /// </summary>
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for authentication.")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IConfiguration _configuration;
        private readonly OtpService _otpService;
        private readonly LoginAttemptPolicy _loginAttemptPolicy;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly UserGroupService _userGroupService;
        private readonly PasswordHistoryService _passwordHistoryService;
        private readonly PasswordExpirationPolicy _passwordExpirationPolicy;


        public AuthenticationController(
            IConfiguration configuration,
            IUserGroupRepository userGroupRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IAuthenticationProvider authenticationProvider,
            IPasswordHistoryRepository passwordHistoryRepository = null,
            IAccountRepository accountRepository = null,
            PasswordExpirationPolicy passwordExpirationPolicy = null,
            OtpService otpService = null,
            ILogger logger = null,
            LoginAttemptPolicy loginAttemptPolicy = null)
        {
            _configuration = configuration;
            _userGroupService = new UserGroupService(userGroupRepository);
            _refreshTokenService = new RefreshTokenService(refreshTokenRepository);
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
            _passwordExpirationPolicy = passwordExpirationPolicy ?? new PasswordExpirationPolicy(); ;
            _passwordHistoryService = passwordHistoryRepository is null ?
                null :
                new PasswordHistoryService(passwordHistoryRepository, logger, accountRepository, _passwordExpirationPolicy);
            _authenticationService = _loginAttemptPolicy is null ?
                new AuthenticationService(authenticationProvider) :
                new AuthenticationService(authenticationProvider, _loginAttemptPolicy);
            _otpService = otpService;
        }

        /// <summary>
        ///     Creates a JWT authorization token.
        /// </summary>
        /// <param name="validationDTO">The validation dto.</param>
        /// <response code="400">Account validation failed.</response>
        [AllowAnonymous]
        [HttpPost("api/tokens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateToken(ValidationDTO validationDTO)
        {
            Response.Headers.Add("Cache-Control", "no-store");
            Response.Headers.Add("Pragma", "no-cache");
            var clientIp = GetTrueRequestIP(Request.HttpContext)?.ToString();
            var validated = await _authenticationService.Validate(validationDTO.Id, validationDTO.Password, clientIp);
            _authenticationService.TryGet(validationDTO.Id, out var account);

            if (!validated)
            {
                if (account != null)
                {
                    if (account.Enabled == false)
                    {
                        return BadRequest("Account is disabled.");
                    }

                    if (account.Locked == true)
                    {
                        return BadRequest("Account is locked.");
                    }
                    else
                    {
                        return BadRequest("Account validation failed.");
                    }
                }
                else
                {
                    return BadRequest("Account validation failed.");
                }
            }
            else
            {
                if (account.Enabled == false)
                {
                    return BadRequest("Account is disabled.");
                }
            }

            // User migth or not use the PasswordService
            if (_passwordHistoryService != null)
            {
                // Get service  to validate PasswordHistory
                var passwordExpiryValidated = await _passwordHistoryService.ValidateCurrentPassword(validationDTO.Id, validationDTO.Password, DateTime.Now, clientIp);

                if (passwordExpiryValidated == false)
                {
                    // Check whether contain the data or not
                    var passwordHistory = await _passwordHistoryService.GetCurrentPasswordHistoryAsync(validationDTO.Id, validationDTO.Password);

                    // If there's no data in PasswordHistory for existing account
                    if (passwordHistory == null)
                    {
                        passwordHistory = await _passwordHistoryService.AddPasswordHistoryAsync(account, validationDTO.Password, DateTime.Now);
                    }
                    else
                    {
                        return BadRequest("Account password is expired.");
                    }
                }
            }

            if (_otpService != null && !_configuration.GetValue("Tokens:DisableOtp", false))
            {
                var twoFAConfig = _userGroupService.GetTwoFAMetadata(account, _configuration.GetValue("AppConfiguration:2FAMetadataKey", "2FAMetadata"));
                var otpConfiguration = _otpService.GetOtpConfiguration(twoFAConfig, cidr => IpWhitelist.Validate(cidr, clientIp));

                if (otpConfiguration.AccessForbidden)
                {
                    return Forbid();
                }

                if (otpConfiguration.OtpRequired && string.IsNullOrEmpty(validationDTO.Otp))
                {
                    return Ok(otpConfiguration);
                }

                if (otpConfiguration.OtpRequired && !string.IsNullOrEmpty(validationDTO.Otp) && !_otpService.ValidateOtp(validationDTO.Otp, twoFAConfig, validationDTO.OtpAuthenticator))
                {
                    return BadRequest("Illegal one-time password.");
                }
            }

            var claims = GetClaims(account);
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(claims);
            var expirationTimeSpan = TimeSpan.FromDays(_configuration.GetValue("Tokens:RefreshExpirationInDays", 365));
            var refreshToken = _refreshTokenService.CreateRefreshToken(account.Id, expirationTimeSpan, clientIp);

            return Ok(
                new
                {
                    AccessToken = new TokenDTO { Token = accessToken, Expiration = accessTokenExpiration },
                    TokenType = "bearer",
                    RefreshToken = new TokenDTO { Token = refreshToken.Token, Expiration = refreshToken.Expiration }
                }
            );
        }

        /// <summary>
        ///     Creates a new JWT authorization token (and a new refresh token) if given a valid refresh token.
        /// </summary>
        /// <response code="400">Refresh token does not exist for the given account or is expired.</response>
        [AllowAnonymous]
        [HttpPost("api/tokens/refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public IActionResult RefreshToken([FromBody] string token)
        {
            Response.Headers.Add("Cache-Control", "no-store");
            Response.Headers.Add("Pragma", "no-cache");
            var clientIp = GetTrueRequestIP(Request.HttpContext)?.ToString();
            var expirationTimeSpan = TimeSpan.FromDays(_configuration.GetValue("Tokens:RefreshExpirationInDays", 365));
            var refreshToken = _refreshTokenService.GetByToken(token);
            _authenticationService.TryGet(refreshToken.AccountId, out var account);
            if (account.Enabled == false)
            {
                return BadRequest("Account is disabled.");
            }

            var claims = GetClaims(account);
            var (newAccessToken, newAccessTokenExpiration) = GenerateAccessToken(claims);
            var newRefreshToken = _refreshTokenService.ExchangeRefreshToken(token, refreshToken.AccountId, expirationTimeSpan, clientIp);
            return Ok(
                new
                {
                    AccessToken = new TokenDTO { Token = newAccessToken, Expiration = newAccessTokenExpiration },
                    TokenType = "bearer",
                    RefreshToken = new TokenDTO { Token = newRefreshToken.Token, Expiration = newRefreshToken.Expiration }
                }
            );
        }

        /// <summary>
        ///     Registers a user account for two-factor authentication using a one-time password (OTP) authenticator.
        /// </summary>
        /// <param name="otpRegistrationDTO">The OTP registration dto.</param>
        /// <response code="400">Two-factor authentication using OTP is not enabled or not available.</response>
        /// <response code="400">Account is already registered.</response>
        /// <response code="400">Account validation failed.</response>
        [AllowAnonymous]
        [HttpPost("api/tokens/otp/registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> RegisterForOtp(OtpRegistrationDTO otpRegistrationDTO)
        {
            Response.Headers.Add("Cache-Control", "no-store");
            Response.Headers.Add("Pragma", "no-cache");
            var clientIp = GetTrueRequestIP(Request.HttpContext)?.ToString();
            if (_configuration.GetValue<bool>("Tokens:DisableOtp") || _otpService == null || !_otpService.OtpAuthenticatorIsAvailable)
            {
                return BadRequest("Two-factor authentication using OTP is not enabled or not available.");
            }

            var accountId = otpRegistrationDTO.Id;
            var validated = await _authenticationService.Validate(accountId, otpRegistrationDTO.Password, clientIp);
            if (!validated)
            {
                return BadRequest("Account validation failed.");
            }

            _authenticationService.TryGet(otpRegistrationDTO.Id, out var account);
            if (account.Enabled == false)
            {
                return BadRequest("Account is disabled.");
            }

            var otpMetadata = _userGroupService.GetTwoFAMetadata(account, _configuration.GetValue("AppConfiguration:2FAMetadataKey", "2FAMetadata"));
            var (manualEntryCode, qrCode) = _otpService.GenerateOtpAuthenticatorSetupCode(accountId, otpMetadata, otpRegistrationDTO.OtpAuthenticator);
            return Ok(
                new
                {
                    ManualEntryCode = manualEntryCode,
                    QrCode = qrCode
                }
            );
        }

        /// <summary>
        ///     Performs an account validation.
        /// </summary>
        /// <remarks>
        ///     Checks whether the account exists, the password is correct and the account is activated.
        /// </remarks>
        /// <param name="validationDTO">The validation body.</param>
        /// <response code="400">Account validation failed.</response>
        [HttpPost("api/accounts/validation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> Validate([FromBody] ValidationDTO validationDTO)
        {
            var clientIp = GetTrueRequestIP(Request.HttpContext)?.ToString();

            string validatedMsg = "";

            var validated = await _authenticationService.Validate(validationDTO.Id, validationDTO.Password, clientIp);

            var res = _authenticationService.TryGet(validationDTO.Id, out var account);

            if (!validated || !res)
            {
                validatedMsg = "Account validation failed.";
            }
            else
            {
                // User migth or not use the PasswordService
                if (_passwordHistoryService != null)
                {
                    // Get service  to validate PasswordHistory
                    var passwordExpiryValidated = await _passwordHistoryService.ValidateCurrentPassword(validationDTO.Id, validationDTO.Password, DateTime.Now, clientIp);

                    if (passwordExpiryValidated == false)
                    {
                        // Check whether contain the data or not
                        var passwordHistory = await _passwordHistoryService.GetCurrentPasswordHistoryAsync(validationDTO.Id, validationDTO.Password);

                        // If there's no data in PasswordHistory for existing account
                        if (passwordHistory == null)
                        {
                            await _passwordHistoryService.AddPasswordHistoryAsync(account, validationDTO.Password, DateTime.Now);
                        }
                        else
                        {
                            validatedMsg = "Account password is expired.";
                        }
                    }
                }
            }

            return validated ? Ok(account) : BadRequest(validatedMsg);
        }

        /// <summary>
        ///     Performs validation of token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <response code="400">Token is not valid</response>
        [HttpPost("api/tokens/validation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            return ValidateAccessToken(token) ? Ok("Token is valid") : BadRequest("The token is not valid");
        }

        private static IPAddress GetTrueRequestIP(HttpContext context)
        {
            IPAddress remoteIp;
            // https://developers.cloudflare.com/fundamentals/reference/http-request-headers/#cf-connecting-ip
            if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var forwardedFor))
            {
                if (!IPAddress.TryParse(forwardedFor.First(), out remoteIp))
                {
                    throw new Exception("Unable to parse CF-Connecting-IP IP address");
                }
            }
            else if (context.Request.Headers.TryGetValue("X-Forwarded-For", out forwardedFor))
            {
                var ipAddressChain = forwardedFor.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries);

                if (!IPAddress.TryParse(ipAddressChain.First(), out remoteIp))
                {
                    throw new Exception("Unable to parse X-Forwarded-For IP address");
                }
            }
            else
            {
                remoteIp = context.Connection.RemoteIpAddress;
            }

            return remoteIp;
        }

        private IEnumerable<Claim> GetClaims(Account account)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, account.Id),
                new(ClaimTypes.Name, account.Name)
            };

            if (!string.IsNullOrEmpty(account.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, account.Email));
            }

            if (!string.IsNullOrEmpty(account.Company))
            {
                claims.Add(new Claim("company", account.Company));
            }

            claims.AddRange(account.Metadata.Select(kvp => new Claim(kvp.Key.ToString().ToLower(), kvp.Value.ToString())));
            var roles = account.GetRoles();
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var userGroups = _userGroupService.GetIds(account.Id);
            claims.AddRange(userGroups.Select(userGroup => new Claim(ClaimTypes.GroupSid, userGroup)));
            return claims;
        }

        private bool ValidateAccessToken(string token)
        {
            var validator = new JwtSecurityTokenHandler();
            IdentityModelEventSource.ShowPII = true;

            // These need to match the values used to generate the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration.GetMandatoryValue<string>("Tokens:Issuer"),
                ValidAudience = _configuration.GetMandatoryValue<string>("Tokens:Audience"),
                IssuerSigningKey = RSA.BuildSigningKey(_configuration.GetMandatoryValue<string>("Tokens:PublicRSAKey").Resolve())
            };

            if (validator.CanReadToken(token))
            {
                try
                {
                    // This line throws if invalid
                    validator.ValidateToken(token, validationParameters, out _);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private (string token, DateTime expiration) GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = RSA.BuildSigningKey(_configuration.GetMandatoryValue<string>("Tokens:PrivateRSAKey").Resolve());
            var token = new JwtSecurityToken(
                _configuration.GetMandatoryValue<string>("Tokens:Issuer"),
                _configuration.GetMandatoryValue<string>("Tokens:Audience"),
                claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue("Tokens:ExpirationInMinutes", 30)),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest));

            return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
        }

        [AllowAnonymous]
        [HttpGet("api/tokens/PasswordHistory/Get/{id}/{password}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> GetValidatedPasswordHistory([FromRoute] ValidationDTO validationDTO)
        {
            string validatedMsg = "";
            var clientIp = GetTrueRequestIP(Request.HttpContext)?.ToString();
            var validated = await _authenticationService.Validate(validationDTO.Id, validationDTO.Password, clientIp);
            if (!validated)
            {
                validatedMsg = "Account validation failed.";
                return BadRequest(validatedMsg);
            }

            var account = _authenticationService.Get(validationDTO.Id);

            var passwordHistory = _passwordHistoryService.GetCurrentPasswordHistoryAsync(account.Id, validationDTO.Password);

            return validated ? Ok(passwordHistory) : BadRequest(validatedMsg);
        }
    }
}
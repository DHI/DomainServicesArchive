namespace DHI.Services.Authentication.Otp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Service enabling two-factor authentication using one or more one-time password (OTP) authenticators.
    /// </summary>
    public class OtpService
    {
        private const string _cidrCheck = "CIDR";
        private readonly Dictionary<string, IOtpAuthenticator> _otpAuthenticators;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OtpService" /> class.
        /// </summary>
        /// <param name="otpAuthenticator">The one-time password authenticator for two-factor authentication.</param>
        public OtpService(IOtpAuthenticator otpAuthenticator)
        {
            Guard.Against.Null(otpAuthenticator, nameof(otpAuthenticator));
            var name = otpAuthenticator.GetType().Name;
            _otpAuthenticators = new Dictionary<string, IOtpAuthenticator> {{name, otpAuthenticator}};
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OtpService" /> class.
        /// </summary>
        /// <param name="otpAuthenticators">A dictionary of one-time password authenticators.</param>
        public OtpService(Dictionary<string, IOtpAuthenticator> otpAuthenticators)
        {
            Guard.Against.NullOrEmpty(otpAuthenticators, nameof(otpAuthenticators));
            otpAuthenticators.Values.ToList().ForEach(authenticator => Guard.Against.Null(authenticator, $"{nameof(otpAuthenticators)}.Member"));
            _otpAuthenticators = otpAuthenticators;
        }

        /// <summary>
        ///     Gets a value indicating whether any OTP authenticator is available.
        /// </summary>
        /// <value><c>true</c> if OTP authenticator is available; otherwise, <c>false</c>.</value>
        public bool OtpAuthenticatorIsAvailable => !(_otpAuthenticators is null) && _otpAuthenticators.Any();

        /// <summary>
        ///     Validates the specified one-time password.
        /// </summary>
        /// <param name="otp">The one-time password.</param>
        /// <param name="twoFactorAuthenticators">Containing valid 2FA configurations.</param>
        /// <param name="otpAuthenticatorKey"></param>
        /// <returns><c>true</c> if one-time password is valid, <c>false</c> otherwise.</returns>
        public bool ValidateOtp(string otp, IEnumerable<string> twoFactorAuthenticators, string otpAuthenticatorKey = null)
        {
            if (_otpAuthenticators is null || !_otpAuthenticators.Any())
            {
                throw new Exception("The authentication services is not configured with a one-time password authenticator.");
            }

            if (string.IsNullOrEmpty(otpAuthenticatorKey))
            {
                return _otpAuthenticators.First().Value.Validate(otp, twoFactorAuthenticators);
            }

            if (!_otpAuthenticators.ContainsKey(otpAuthenticatorKey))
            {
                throw new Exception($"{otpAuthenticatorKey} does not reference a valid one-time password authenticator.");
            }

            var otpAuthenticator = _otpAuthenticators[otpAuthenticatorKey];
            return otpAuthenticator.Validate(otp, twoFactorAuthenticators);
        }

        /// <summary>
        ///     Returns the client OTP requirements and configuration for the supplied metadata.
        /// </summary>
        /// <remarks>
        ///     The twoFactorAuthenticators is a list of 2FA configurations
        ///     Each configurations should be of the format "{2faType}:{configuration}".
        ///     There is a built in {2faType} "CIDR" for IP whitelist data.
        /// </remarks>
        /// <param name="twoFactorAuthenticators">Containing valid 2FA configurations.</param>
        /// <param name="ipWhitelistMatch">Function to determine if the requesting IP-address is whitelisted.</param>
        /// <returns>Client OTP configuration</returns>
        public OtpConfiguration GetOtpConfiguration(IEnumerable<string> twoFactorAuthenticators, Func<string[], bool> ipWhitelistMatch)
        {
            Guard.Against.Null(twoFactorAuthenticators, nameof(twoFactorAuthenticators));
            
            if (!twoFactorAuthenticators.Any())
            {
                return new OtpConfiguration
                {
                    OtpRequired = false
                };
            }

            var splitSubStrings = twoFactorAuthenticators.Select(s => s.Split(new[] {':'}, 2));
            var groupedConfig = splitSubStrings.GroupBy(s => s[0]);
            var configDictionary = groupedConfig.ToDictionary(g => g.Key, g => g.Select(gg => gg.Last()));
            var otpConfig = configDictionary.Where(k => k.Key != _cidrCheck).ToDictionary(d => d.Key, d => d.Value);

            if (configDictionary.ContainsKey(_cidrCheck))
            {
                Guard.Against.Null(ipWhitelistMatch, nameof(ipWhitelistMatch));
                
                var cidrConfig = configDictionary[_cidrCheck].ToArray();
                if (cidrConfig.Any(string.IsNullOrEmpty))
                {
                    throw new InvalidOperationException("CIDR check values cannot be null or empty.");
                }

                if (ipWhitelistMatch(cidrConfig.ToArray()))
                {
                    return new OtpConfiguration { OtpRequired = false };
                }

                if (!otpConfig.Any())
                {
                    return new OtpConfiguration
                    {
                        AccessForbidden = true
                    };
                }
            }

            var validAuthenticators = _otpAuthenticators.Where(a => otpConfig.Select(otp => otp.Key).Contains(a.Key)).ToArray();
            if (!validAuthenticators.Any())
            {
                throw new InvalidOperationException("There is no available otp provider for this user.");
            }

            var result = new OtpConfiguration
            {
                OtpRequired = true,
                OtpAuthenticatorIds = validAuthenticators.Select(a => a.Key).ToArray()
            };

            return result;
        }

        /// <summary>
        ///     Returns OTP code for an account from configured authenticators.
        /// </summary>
        /// <param name="accountName">The account id.</param>
        /// <param name="twoFactorAuthenticators">Containing valid otp configuration.</param>
        /// <param name="otpAuthenticatorKey">Identifier for a valid authenticator.</param>
        /// <returns>A manual entry code and a QR code for scanning on the client.</returns>
        public (string manualEntryCode, string qrCode) GenerateOtpAuthenticatorSetupCode(string accountName, IEnumerable<string> twoFactorAuthenticators, string otpAuthenticatorKey)
        {
            Guard.Against.Null(twoFactorAuthenticators, nameof(twoFactorAuthenticators));
            Guard.Against.NullOrEmpty(otpAuthenticatorKey, nameof(otpAuthenticatorKey));
            Guard.Against.NullOrEmpty(accountName, nameof(accountName));

            var splitSubStrings = twoFactorAuthenticators.Select(s => s.Split(new[] { ':' }, 2));
            var groupedConfig = splitSubStrings.GroupBy(s => s[0]);

            if (_otpAuthenticators.ContainsKey(otpAuthenticatorKey) && groupedConfig.Any(gc => gc.Key == otpAuthenticatorKey))
            {
                return _otpAuthenticators[otpAuthenticatorKey].GenerateSetupCode(accountName);
            }

            throw new ArgumentException($"Specified OTP Authenticator '{otpAuthenticatorKey}' does not match valid types for this account or does not exist.");
        }
    }
}
using System.Collections.Generic;

namespace DHI.Services.Authentication.Otp
{
    /// <summary>
    ///     Abstraction of OTP (one-time password) authenticator to facilitate two-factor authentication
    /// </summary>
    public interface IOtpAuthenticator
    {
        /// <summary>
        ///     Generates the setup code.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <returns>A code for manual entry and a QR code for scanning</returns>
        (string manualEntryCode, string qrCode) GenerateSetupCode(string accountName);

        /// <summary>
        ///     Validates the specified one-time password.
        /// </summary>
        /// <param name="otp">The one-time password.</param>
        /// <param name="twoFactorAuthenticators">Containing valid 2FA configurations.</param>
        /// <returns><c>true</c> if one-time password is valid <c>false</c> otherwise.</returns>
        bool Validate(string otp, IEnumerable<string> twoFactorAuthenticators);
    }
}
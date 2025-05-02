#pragma warning disable CA1819 //per documentation warning disabled as this is a DTO type class
namespace DHI.Services.Authentication.Otp
{
    /// <summary>
    ///     OTP configuration
    /// </summary>
    public class OtpConfiguration
    {
        /// <summary>
        ///     Is OTP required flag to client.
        /// </summary>
        public bool OtpRequired { get; set; }
        
        /// <summary>
        ///     Is Access forbidden to this user.
        /// </summary>
        public bool AccessForbidden { get; set; }

        /// <summary>
        ///     Array of applicable OTP authenticator identifiers.
        /// </summary>
        public string[] OtpAuthenticatorIds { get; set; }
    }
}
#pragma warning restore CA1819
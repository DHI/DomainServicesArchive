namespace DHI.Services.Authentication.PasswordHistory
{
    public class PasswordExpirationPolicy
    {
        /// <summary>
        /// Password expiry duration in days, default set to 3 days
        /// </summary>
        public int PasswordExpiryDurationInDays { get; set; } = 3;

        /// <summary>
        /// Number of previous passwords user is unable to re-use
        /// </summary>
        public int PreviousPasswordsReUseLimit { get; set; } = 3;
    }
}

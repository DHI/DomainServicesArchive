namespace DHI.Services.Authentication.PasswordHistory
{
    using System;
    using DHI.Services;

    public class PasswordHistory : BaseEntity<string>
    {

        public PasswordHistory(string id)
            : base(id)
        {
        }

        /// <summary>
        ///     Gets or sets user account id
        /// </summary>
        public virtual string AccountId { get; set; }

        /// <summary>
        ///     Gets or sets encrypted password
        /// </summary>
        public virtual byte[] EncryptedPassword { get; set; }

        /// <summary>
        ///     Gets or sets password expiry date
        /// </summary>
        public virtual DateTime PasswordExpiryDate { get; set; }
    }
}

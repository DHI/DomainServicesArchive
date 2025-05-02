namespace DHI.Services.Security.WebApi.DTOs
{
    using System.Text.Json.Serialization;
    using Accounts;

    /// <summary>
    ///     Data transfer object for account update resource representation.
    /// </summary>
    public class AccountUpdateDTO : AccountBaseDTO
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountUpdateDTO" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public AccountUpdateDTO(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a user is allowed to change the password of his/her own account.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a user is allowed to change the password his/her own account; otherwise, <c>false</c>.
        /// </value>
        [JsonPropertyName("allowMePasswordChange")]
        public virtual bool AllowMePasswordChange { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating that Account is Enabled or Disabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a user account is enabled; otherwise, <c>false</c>.
        /// </value>
        [JsonPropertyName("enabled")]
        public virtual bool Enabled { get; set; } = true;

        /// <summary>
        ///     Converts the DTO to an Account object.
        /// </summary>
        public Account ToAccount()
        {
            var account = new Account(Id, Name);
            if (!string.IsNullOrWhiteSpace(Password))
            {
                account.SetPassword(Password);
            }

            account.AllowMePasswordChange = AllowMePasswordChange;
            account.Company = Company;
            account.PhoneNumber = PhoneNumber;
            account.Email = Email;
            account.Roles = Roles;
            account.Enabled = Enabled;

            account.NoOfUnsuccessfulLoginAttempts = NoOfUnsuccessfulLoginAttempts;
            account.LastLoginAttemptedDate = LastLoginAttemptedDate;
            account.Locked = Locked;
            account.LockedDateEnd = LockedDateEnd;

            if (!(Metadata is null))
            {
                foreach (var data in Metadata)
                {
                    account.Metadata.Add(data.Key.AsString(), data.Value);
                }
            }

            return account;
        }
    }
}
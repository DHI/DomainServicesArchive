namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Accounts;

    /// <summary>
    ///     Data transfer object for account resource representation.
    /// </summary>
    public sealed class AccountDTO : AccountBaseDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDTO" /> class.
        /// </summary>
        public AccountDTO() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountDTO" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public AccountDTO(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountDTO" /> class specified by account and userGroups.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="userGroups">The user groups.</param>
        public AccountDTO(Account account, string[] userGroups)
        {
            Id = account.Id;
            Name = account.Name;
            Activated = account.Activated;
            AllowMePasswordChange = account.AllowMePasswordChange;
            Company = account.Company;
            Email = account.Email;
            PhoneNumber = account.PhoneNumber;

            NoOfUnsuccessfulLoginAttempts = account.NoOfUnsuccessfulLoginAttempts;
            LastLoginAttemptedDate = account.LastLoginAttemptedDate;
            Locked = account.Locked;
            LockedDateEnd = account.LockedDateEnd;
            Enabled = account.Enabled;

            if (account.Metadata.Any())
            {
                Metadata = new Dictionary<string, object>();
                foreach (var data in account.Metadata)
                {
                    Metadata.Add(data.Key, data.Value);
                }
            }

            if (userGroups.Any())
            {
                UserGroups = userGroups;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Account" /> is activated.
        /// </summary>
        /// <value><c>true</c> if activated; otherwise, <c>false</c>.</value>
        [JsonPropertyName("activated")]
        public bool Activated { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets a token for account activation or password reset.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a user is allowed to change the password of his/her own account.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a user is allowed to change the password his/her own account; otherwise, <c>false</c>.
        /// </value>
        [JsonPropertyName("allowMePasswordChange")]
        public bool AllowMePasswordChange { get; set; } = true;


        /// <summary>
        ///     Gets or sets a value indicating that Account is Enabled or Disabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a user account is enabled; otherwise, <c>false</c>.
        /// </value>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Converts the DTO to an Account object.
        /// </summary>
        public Account ToAccount()
        {
            var account = new Account(Id, Name);
            account.SetPassword(Password);
            account.AllowMePasswordChange = AllowMePasswordChange;
            account.Company = Company;
            account.PhoneNumber = PhoneNumber;
            account.Email = Email;
            account.Roles = Roles;
            account.Activated = Activated;
            account.Token = Token;
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

        /// <summary>
        ///     Converts an Account object to a DTO.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="userGroups"></param>
        public static AccountDTO FromAccount(Account account, IEnumerable<string> userGroups)
        {
            var dto = new AccountDTO(account.Id, account.Name)
            {
                Activated = account.Activated,
                AllowMePasswordChange = account.AllowMePasswordChange,
                Company = account.Company,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Roles = account.Roles,
                Enabled = account.Enabled,

                NoOfUnsuccessfulLoginAttempts = account.NoOfUnsuccessfulLoginAttempts,
                LastLoginAttemptedDate = account.LastLoginAttemptedDate,
                Locked = account.Locked,
                LockedDateEnd = account.LockedDateEnd
            };

            if (account.Metadata.Any())
            {
                dto.Metadata = new Dictionary<string, object>();
                foreach (var data in account.Metadata)
                {
                    dto.Metadata.Add(data.Key, data.Value);
                }
            }

            dto.UserGroups = userGroups;

            return dto;
        }
    }
}
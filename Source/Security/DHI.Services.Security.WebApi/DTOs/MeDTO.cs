namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using Accounts;

    /// <summary>
    ///     Data transfer object for a personal (me) account resource representation.
    /// </summary>
    [Serializable]
    public class MeDTO : AccountBaseDTO
    {
        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Converts the DTO to an Account object.
        /// </summary>
        public Account ToAccount()
        {
            var account = new Account(Id, "NameCannotBeChangedAnyway");
            if (!string.IsNullOrWhiteSpace(Password))
            {
                account.SetPassword(Password);
            }

            account.Company = Company;
            account.PhoneNumber = PhoneNumber;
            account.Email = Email;
            if (Metadata != null)
            {
                foreach (var kvp in Metadata)
                {
                    account.Metadata.Add(kvp.Key.AsString(), kvp.Value);
                }
            }

            return account;
        }
    }
}
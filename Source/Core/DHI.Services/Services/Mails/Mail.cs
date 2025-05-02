namespace DHI.Services.Mails
{
    using System.Net.Mail;

    /// <summary>
    ///     Class Mail.
    /// </summary>
    public static class Mail
    {
        /// <summary>
        ///     Determines whether the given e-mail address is a valid e-mail address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns><c>true</c> if valid e-mail address; otherwise, <c>false</c>.</returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
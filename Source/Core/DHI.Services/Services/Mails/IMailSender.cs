namespace DHI.Services.Mails
{
    using System.Net.Mail;

    /// <summary>
    ///     Interface IMailSender
    /// </summary>
    public interface IMailSender
    {
        /// <summary>
        ///     Sends the specified mail message.
        /// </summary>
        /// <param name="mailMessage">The mail message.</param>
        void Send(MailMessage mailMessage);
    }
}
namespace DHI.Services.Mails
{
    using System.Net;
    using System.Net.Mail;

    /// <summary>
    ///     Class SmtpMailSender.
    /// </summary>
    /// <seealso cref="DHI.Services.Mails.IMailSender" />
    public class SmtpMailSender : IMailSender
    {
        private readonly SmtpClient _smtpClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmtpMailSender" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        public SmtpMailSender(string host)
        {
            _smtpClient = new SmtpClient(host) {UseDefaultCredentials = true};
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmtpMailSender" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public SmtpMailSender(string host, int port)
        {
            _smtpClient = new SmtpClient(host, port) {UseDefaultCredentials = true};
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmtpMailSender" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public SmtpMailSender(string host, int port, string userName, string password)
        {
            _smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(userName, password)
            };
        }

        /// <summary>
        ///     Sends the specified mail message.
        /// </summary>
        /// <param name="mailMessage">The mail message.</param>
        public void Send(MailMessage mailMessage)
        {
            _smtpClient.Send(mailMessage);
        }

        /// <summary>
        ///     Sends the specified mail message.
        /// </summary>
        /// <param name="from">The sender.</param>
        /// <param name="to">The receiver.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public void Send(string from, string to, string subject, string body)
        {
            var mailMessage = new MailMessage(from, to, subject, body);
            Send(mailMessage);
        }

        /// <summary>
        ///     Sends the specified mail message.
        /// </summary>
        /// <param name="from">The Sender.</param>
        /// <param name="to">The receiver.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public void Send(MailAddress from, MailAddress to, string subject, string body)
        {
            var mailMessage = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body
            };

            Send(mailMessage);
        }
    }
}
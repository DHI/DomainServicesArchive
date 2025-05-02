namespace DHI.Services.Test
{
    using System.Net.Mail;
    using Mails;

    public class FakeMailSender : IMailSender
    {
        public MailMessage Message { get; internal set; }

        public void Send(MailMessage mailMessage)
        {
            Message = mailMessage;
        }
    }
}
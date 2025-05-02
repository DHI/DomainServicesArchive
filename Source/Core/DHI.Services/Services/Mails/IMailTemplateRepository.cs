namespace DHI.Services.Mails
{
    /// <summary>
    ///     Interface IMailTemplateRepository
    /// </summary>
    /// <seealso cref="IRepository{MailTemplate, String}" />
    /// <seealso cref="IDiscreteRepository{MailTemplate, String}" />
    /// <seealso cref="IUpdatableRepository{MailTemplate, String}" />
    public interface IMailTemplateRepository : IRepository<MailTemplate, string>, IDiscreteRepository<MailTemplate, string>, IUpdatableRepository<MailTemplate, string>
    {
    }
}
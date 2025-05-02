namespace DHI.Services.Mails
{
    using System;

    /// <summary>
    ///     Class MailTemplateService.
    /// </summary>
    /// <seealso cref="BaseUpdatableDiscreteService{MailTemplate, String}" />
    public class MailTemplateService : BaseUpdatableDiscreteService<MailTemplate, string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MailTemplateService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public MailTemplateService(IMailTemplateRepository repository)
            : base(repository)
        {
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IMailTemplateRepository>(path);
        }
    }
}
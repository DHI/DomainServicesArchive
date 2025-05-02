namespace DHI.Services.Test
{
    using System.Collections.Generic;
    using Mails;

    internal class FakeMailTemplateRepository : FakeRepository<MailTemplate, string>, IMailTemplateRepository
    {
        public FakeMailTemplateRepository()
        {
        }

        public FakeMailTemplateRepository(IEnumerable<MailTemplate> templateList)
            : base(templateList)
        {
        }
    }
}
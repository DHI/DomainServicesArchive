namespace DHI.Services.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using Mails;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMailTemplateDataAttribute : AutoDataAttribute
    {
        public AutoMailTemplateDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var templates = fixture.CreateMany<MailTemplate>().ToList();
                    fixture.Register<IMailTemplateRepository>(() => new FakeMailTemplateRepository(templates));
                }
                else
                {
                    fixture.Register<IMailTemplateRepository>(() => new FakeMailTemplateRepository());
                }

                return fixture;
            })
        {
        }
    }
}
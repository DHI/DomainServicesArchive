namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using System.Net.Mail;
    using Accounts;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using Jobs;
    using Mails;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoJobDataAttribute : AutoDataAttribute
    {
        public AutoJobDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var jobList = fixture.CreateMany<Job<Guid, string>>().ToList();
                    fixture.Register<IJobRepository<Guid, string>>(() => new FakeJobRepository(jobList));
                }
                else
                {
                    fixture.Register<IJobRepository<Guid, string>>(() => new FakeJobRepository());
                }

                fixture.Customizations.Add(new TypeRelay(typeof(IDiscreteService<Account, string>), typeof(AccountService)));

                var taskList = fixture.CreateMany<FakeTask<string>>().ToList();
                var accountList = fixture.CreateMany<Account>().ToList();
                fixture.Register<ITaskRepository<FakeTask<string>, string>>(() => new FakeTaskRepository<string>(taskList));
                fixture.Register<ITaskService<FakeTask<string>, string>>(() => new TaskService<FakeTask<string>, string>(new FakeTaskRepository<string>(taskList)));
                fixture.Register<IAccountRepository>(() => new FakeAccountRepository(accountList));
                fixture.Customize<Account>(c => c.With(account => account.Email, fixture.Create<MailAddress>().Address));
                fixture.Customize<MailTemplate>(c => c.With(template => template.From, fixture.Create<MailAddress>().Address));
                fixture.Register<IMailSender>(() => new FakeMailSender());

                return fixture;
            })
        {
        }
    }
}
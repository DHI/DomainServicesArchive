namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using System.Net.Mail;
    using Accounts;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using Jobs;
    using Logging;
    using Mails;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoJobWorkerDataAttribute : AutoDataAttribute
    {
        public AutoJobWorkerDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
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
                var fakeTaskRepository = new FakeTaskRepository<string>(taskList);
                var accountList = fixture.CreateMany<Account>().ToList();
                fixture.Register<ITaskRepository<FakeTask<string>, string>>(() => fakeTaskRepository);
                fixture.Register<ITaskService<FakeTask<string>, string>>(() => new TaskService<FakeTask<string>, string>(fakeTaskRepository));
                fixture.Register<IAccountRepository>(() => new FakeAccountRepository(accountList));
                fixture.Customize<Account>(c => c.With(account => account.Email, fixture.Create<MailAddress>().Address));
                fixture.Customize<MailTemplate>(c => c.With(template => template.From, fixture.Create<MailAddress>().Address));
                fixture.Register<IMailSender>(() => new FakeMailSender());
                fixture.Register<IWorker<Guid, string>>(() => new FakeWorker());
                fixture.Register<IHostRepository>(() => new FakeHostRepository());
                fixture.Register<IGroupedHostRepository>(() => new FakeGroupedHostRepository());
                fixture.Register<ILogger>(() => NullLogger.Instance);

                return fixture;
            })
        {
        }
    }
}
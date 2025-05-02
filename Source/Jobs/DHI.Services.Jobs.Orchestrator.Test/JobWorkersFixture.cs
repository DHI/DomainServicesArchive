namespace DHI.Services.Jobs.Orchestrator.Test
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Workflows;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class JobWorkersFixture
    {
        public JobWorkersFixture()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            Logger = fixture.Create<ILogger>();
            var workflowRepository = fixture.Create<ICodeWorkflowRepository>();
            var workflowService = new CodeWorkflowService(workflowRepository);
            var jobRepository = fixture.Create<IJobRepository<Guid, string>>();
            JobService = new JobService<CodeWorkflow, string>(jobRepository, workflowService);
            var hostRepository = fixture.Create<IHostRepository>();
            var hostService = new HostService(hostRepository);
            var worker = fixture.Create<IWorker<Guid, string>>();
            JobWorker = new JobWorker<CodeWorkflow, string>("myJobWorker", worker, workflowService, JobService, hostService);
            JobWorkers = new List<JobWorker<CodeWorkflow, string>> { JobWorker };
        }

        public ILogger Logger { get; }

        public JobService<CodeWorkflow, string> JobService { get;  }

        public JobWorker<CodeWorkflow, string> JobWorker { get; }

        public List<JobWorker<CodeWorkflow, string>> JobWorkers { get; }
    }
}
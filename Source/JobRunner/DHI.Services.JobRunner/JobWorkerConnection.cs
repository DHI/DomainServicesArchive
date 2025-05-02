namespace DHI.Services.JobRunner
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Jobs;
    using Jobs.Workflows;
    using Logging;
    using Properties;

    /// <inheritdoc/>
    public class JobWorkerConnection : JobWorkerConnection<Workflow, string>
    {
        /// <inheritdoc/>
        public JobWorkerConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the repository logger connection string.
        /// </summary>
        public virtual string RepositoryLoggerConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the repository logger.
        /// </summary>
        public string RepositoryLoggerType { get; set; }

        /// <inheritdoc/>
        public override object Create(ILogger loadBalancerLogger)
        {
            try
            {
                ILogger repositoryLogger = null;
                if (RepositoryLoggerType != null)
                {
                    var repositoryLoggerType = Type.GetType(RepositoryLoggerType, true);
                    repositoryLogger = (ILogger)Activator.CreateInstance(repositoryLoggerType, RepositoryLoggerConnectionString.Resolve());
                }

                // Create services
                var (taskService, jobService) = CreateTaskAndJobServices(repositoryLogger);
                IHostService hostService = null;
                if (HostRepositoryType != null)
                {
                    hostService = CreateHostService(repositoryLogger);
                }

                // Create worker
                ILogger workerLogger = null;
                if (LoggerType != null)
                {
                    var loggerType = Type.GetType(LoggerType, true);
                    workerLogger = (ILogger)Activator.CreateInstance(loggerType, LoggerConnectionString.Resolve());
                }

                var workerType = Type.GetType(WorkerType, true);
                var worker = WorkerConnectionString != null ? Activator.CreateInstance(workerType, WorkerConnectionString, workerLogger) : Activator.CreateInstance(workerType, workerLogger);

                // Create load balancer
                ILoadBalancer loadBalancer;
                if (LoadBalancerType is null)
                {
                    loadBalancer = new LoadBalancer<Workflow, string>(Id, (IWorker<Guid, string>)worker, jobService, hostService, loadBalancerLogger, DefaultHostGroup);
                }
                else
                {
                    var loadBalancerType = Type.GetType(LoadBalancerType, true);
                    loadBalancer = (ILoadBalancer)Activator.CreateInstance(loadBalancerType, (IWorker<Guid, string>)worker, jobService, hostService, loadBalancerLogger, DefaultHostGroup);
                }

                // Create JobWorker
                return new JobWorker<Workflow, string>(Id, (IWorker<Guid, string>)worker, taskService, jobService, hostService, loadBalancer, Settings.Default.JobTimeout, Settings.Default.StartTimeout, Settings.Default.MaxAge, loadBalancerLogger);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        ///     Creates a job service to be used for calculation of scalar values.
        /// </summary>
        public JobService<Workflow, string> CreateJobService()
        {
            ILogger repositoryLogger = null;
            if (RepositoryLoggerType != null)
            {
                var repositoryLoggerType = Type.GetType(RepositoryLoggerType, true);
                repositoryLogger = (ILogger)Activator.CreateInstance(repositoryLoggerType, RepositoryLoggerConnectionString.Resolve());
            }

            var (_, jobService) = CreateTaskAndJobServices(repositoryLogger);
            return jobService;
        }

        private IHostService CreateHostService(ILogger repositoryLogger)
        {
            IHostService hostService;
            IHostRepository hostRepository;
            var hostRepositoryType = Type.GetType(HostRepositoryType, true);
            if (repositoryLogger is null)
            {
                hostRepository = (IHostRepository)Activator.CreateInstance(hostRepositoryType, HostRepositoryConnectionString.Resolve());
            }
            else
            {
                try
                {
                    hostRepository = (IHostRepository)Activator.CreateInstance(hostRepositoryType, HostRepositoryConnectionString.Resolve(), repositoryLogger);
                }
                catch (Exception)
                {
                    hostRepository = (IHostRepository)Activator.CreateInstance(hostRepositoryType, HostRepositoryConnectionString.Resolve());
                }
            }

            if (hostRepository is IGroupedHostRepository repository)
            {
                hostService = new GroupedHostService(repository);
            }
            else
            {
                hostService = new HostService(hostRepository);
            }

            return hostService;
        }

        private (TaskService<Workflow, string> taskService, JobService<Workflow, string> jobService) CreateTaskAndJobServices(ILogger repositoryLogger)
        {
            var jobRepositoryType = Type.GetType(JobRepositoryType, true);
            var taskRepositoryType = Type.GetType(TaskRepositoryType, true);
            IJobRepository<Guid, string> jobRepository;
            ITaskRepository<Workflow, string> taskRepository;
            if (repositoryLogger is null)
            {
                jobRepository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString.Resolve());
                taskRepository = (ITaskRepository<Workflow, string>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString.Resolve());
            }
            else
            {
                try
                {
                    jobRepository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString.Resolve(), repositoryLogger);
                }
                catch (Exception)
                {
                    jobRepository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString.Resolve());
                }

                try
                {
                    taskRepository = (ITaskRepository<Workflow, string>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString.Resolve(), repositoryLogger);
                }
                catch (Exception)
                {
                    taskRepository = (ITaskRepository<Workflow, string>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString.Resolve());
                }
            }

            var taskService = new TaskService<Workflow, string>(taskRepository);
            var jobService = new JobService<Workflow, string>(jobRepository, taskService);
            return (taskService, jobService);
        }
    }
}
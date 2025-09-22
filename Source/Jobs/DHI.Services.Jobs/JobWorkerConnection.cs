namespace DHI.Services.Jobs
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Notifications;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class JobWorkerConnection.
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">the task type</typeparam>
    [Obsolete("You should compose your JobWorker in code. This type will eventually be removed.")]
    public class JobWorkerConnection<TTask, TTaskId> : BaseConnection where TTask : ITask<TTaskId>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JobWorkerConnection{TTask, TTaskId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public JobWorkerConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the job repository connection string.
        /// </summary>
        public string JobRepositoryConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the job repository.
        /// </summary>
        public string JobRepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the task repository connection string.
        /// </summary>
        public virtual string TaskRepositoryConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the task repository.
        /// </summary>
        public string TaskRepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the worker connection string.
        /// </summary>
        public virtual string WorkerConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the worker.
        /// </summary>
        public string WorkerType { get; set; }

        /// <summary>
        ///     Gets or sets the host repository connection string.
        /// </summary>
        public virtual string HostRepositoryConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the host repository.
        /// </summary>
        public string HostRepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the default host group.
        /// </summary>
        /// <value>The default host group.</value>
        public string DefaultHostGroup { get; set; }

        /// <summary>
        ///     Gets or sets the type of the load balancer.
        /// </summary>
        public string LoadBalancerType { get; set; }

        /// <summary>
        ///     Gets or sets the logger connection string.
        /// </summary>
        public virtual string LoggerConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the logger.
        /// </summary>
        /// <value>The type of the worker.</value>
        public string LoggerType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : JobWorkerConnection<TTask, TTaskId>
        {
            var connectionType = new ConnectionType("JobWorkerConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("JobRepositoryType", JobWorker<TTask, TTaskId>.GetJobRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("TaskRepositoryType", JobWorker<TTask, TTaskId>.GetTaskRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("WorkerType", JobWorker<TTask, TTaskId>.GetWorkerTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("HostRepositoryType", JobWorker<TTask, TTaskId>.GetHostRepositoryTypes(path), false));
            connectionType.ProviderTypes.Add(new ProviderType("LoadBalancerType", JobWorker<TTask, TTaskId>.GetLoadBalancerTypes(path), false));
            connectionType.ProviderTypes.Add(new ProviderType("LoggerType", Logger.GetLoggerTypes(path), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("JobRepositoryConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("TaskRepositoryConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("WorkerConnectionString", typeof(string), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("HostRepositoryConnectionString", typeof(string), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("DefaultHostGroup", typeof(string), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("LoggerConnectionString", typeof(string), false));
            return connectionType;
        }

        /// <summary>
        ///     Creates a JobWorker instance without logging.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            return Create(null);
        }

        /// <summary>
        ///     Creates a JobWorker instance with logging.
        /// </summary>
        /// <returns>System.Object.</returns>
        public virtual object Create(ILogger logger)
        {
            try
            {
                var jobRepositoryType = Type.GetType(JobRepositoryType, true);
                var jobRepository = (IJobRepository<Guid, TTaskId>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString);
                var taskRepositoryType = Type.GetType(TaskRepositoryType, true);
                var taskRepository = (ITaskRepository<TTask, TTaskId>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString);
                var taskService = new TaskService<TTask, TTaskId>(taskRepository);
                ILogger workerLogger = null;
                if (LoggerType != null)
                {
                    var loggerType = Type.GetType(LoggerType, true);
                    workerLogger = (ILogger)Activator.CreateInstance(loggerType, LoggerConnectionString);
                }

                var workerType = Type.GetType(WorkerType, true);
                var worker = WorkerConnectionString != null ? Activator.CreateInstance(workerType, WorkerConnectionString, workerLogger) : Activator.CreateInstance(workerType, workerLogger);
                var jobService = new JobService<TTask, TTaskId>(jobRepository, taskService);

                IHostService hostService = null;
                if (HostRepositoryType != null)
                {
                    var hostRepositoryType = Type.GetType(HostRepositoryType, true);
                    var hostRepository = (IHostRepository)Activator.CreateInstance(hostRepositoryType, HostRepositoryConnectionString);
                    if (hostRepository is IGroupedHostRepository repository)
                    {
                        hostService = new GroupedHostService(repository);
                    }
                    else
                    {
                        hostService = new HostService(hostRepository);
                    }
                }

                ILoadBalancer loadBalancer;
                if (LoadBalancerType is null)
                {
                    loadBalancer = new LoadBalancer<TTask, TTaskId>(Id, (IWorker<Guid, TTaskId>)worker, jobService, hostService, logger, DefaultHostGroup);
                }
                else
                {
                    var loadBalancerType = Type.GetType(LoadBalancerType, true);
                    loadBalancer = (ILoadBalancer)Activator.CreateInstance(loadBalancerType, (IWorker<Guid, TTaskId>)worker, jobService, hostService, logger, DefaultHostGroup);
                }

                return new JobWorker<TTask, TTaskId>(Id, (IWorker<Guid, TTaskId>)worker, taskService, jobService, hostService, loadBalancer, logger: logger);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
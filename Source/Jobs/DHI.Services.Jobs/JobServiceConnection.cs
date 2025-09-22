namespace DHI.Services.Jobs
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class JobServiceConnection.
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">The task type</typeparam>
    public class JobServiceConnection<TTask, TTaskId> : BaseConnection where TTask : ITask<TTaskId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobServiceConnection{TTask, TTaskId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public JobServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the job repository connection string.
        /// </summary>
        /// <value>The job repository connection string.</value>
        public string JobRepositoryConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the job repository.
        /// </summary>
        /// <value>The type of the job repository.</value>
        public string JobRepositoryType { get; set; }

        /// <summary>
        /// Gets or sets the task repository connection string.
        /// </summary>
        /// <value>The task repository connection string.</value>
        public virtual string TaskRepositoryConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the task repository.
        /// </summary>
        /// <value>The type of the task repository.</value>
        public string TaskRepositoryType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : JobServiceConnection<TTask, TTaskId>
        {
            var connectionType = new ConnectionType("JobServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("JobRepositoryType", JobService<TTask, TTaskId>.GetJobRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("TaskRepositoryType", JobService<TTask, TTaskId>.GetTaskRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("JobRepositoryConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("TaskRepositoryConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a JobService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var jobRepositoryType = Type.GetType(JobRepositoryType, true);
                var repository = (IJobRepository<Guid, TTaskId>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString);
                var taskRepositoryType = Type.GetType(TaskRepositoryType, true);
                var taskRepository = (ITaskRepository<TTask, TTaskId>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString);
                var taskService = new TaskService<TTask, TTaskId>(taskRepository);
                return new JobService<TTask, TTaskId>(repository, taskService, null);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
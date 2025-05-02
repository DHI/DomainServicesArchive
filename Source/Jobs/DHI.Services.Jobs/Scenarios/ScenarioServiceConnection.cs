namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using DHI.Services;
    using Jobs;

    /// <summary>
    ///     Class ScenarioServiceConnection.
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">the task type</typeparam>
    public class ScenarioServiceConnection<TTask, TTaskId> : BaseConnection where TTask : ITask<TTaskId>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScenarioServiceConnection{TTask, TId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public ScenarioServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string RepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the connection string for the jobs repository.
        /// </summary>
        /// <value>The connection string for the jobs repository.</value>
        public virtual string JobRepositoryConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the jobs repository.
        /// </summary>
        /// <value>The type of the jobs repository.</value>
        public virtual string JobRepositoryType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : ScenarioServiceConnection<TTask, TTaskId>
        {
            return new ConnectionType(nameof(ScenarioServiceConnection<TTask, TTaskId>), typeof(TConnection))
            {
                ProviderTypes =
                {
                    new ProviderType("RepositoryType", ScenarioService.GetRepositoryTypes(path)),
                    new ProviderType("JobRepositoryType", JobService<TTask, TTaskId>.GetJobRepositoryTypes(path), false)
                },
                ProviderArguments =
                {
                    new ProviderArgument("ConnectionString", typeof(string)),
                    new ProviderArgument("JobRepositoryConnectionString", typeof(string), false)
                }
            };
        }

        /// <summary>
        ///     Creates a ScenarioService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IScenarioRepository)Activator.CreateInstance(repositoryType, ConnectionString);

                if (JobRepositoryType != string.Empty && JobRepositoryConnectionString != null)
                {
                    var jobRepositoryType = Type.GetType(JobRepositoryType, true);
                    var jobRepository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString);
                    return new ScenarioService(repository, jobRepository);
                }

                return new ScenarioService(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
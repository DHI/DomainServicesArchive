namespace DHI.Services.Models
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    public class ScenarioServiceConnection : BaseConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="repositoryType">The repository type.</param>
        /// <param name="connectionString">The repository connection string.</param>
        /// <param name="modelDataReaderRepositoryType"></param>
        /// <param name="modelDataReaderConnectionString"></param>
        /// <param name="workerType"></param>
        public ScenarioServiceConnection(string id, string name,
            string repositoryType, string connectionString,
            string modelDataReaderRepositoryType, string modelDataReaderConnectionString,
            string workerType) : base(id, name)
        {
            Guard.Against.NullOrEmpty(repositoryType, nameof(repositoryType));
            Guard.Against.NullOrEmpty(connectionString, nameof(connectionString));
            Guard.Against.NullOrEmpty(modelDataReaderRepositoryType, nameof(modelDataReaderRepositoryType));
            Guard.Against.NullOrEmpty(modelDataReaderConnectionString, nameof(modelDataReaderConnectionString));
            Guard.Against.NullOrEmpty(workerType, nameof(workerType));
            RepositoryType = repositoryType;
            ConnectionString = connectionString;
            ModelDataReaderRepositoryType = modelDataReaderRepositoryType;
            ModelDataReaderConnectionString = modelDataReaderConnectionString;
            WorkerType = workerType;
        }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        public string ModelDataReaderConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the repository.
        /// </summary>
        public string ModelDataReaderRepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string RepositoryType { get; set; }

        /// <summary>
        ///     Gets or sets the worker connection string.
        /// </summary>
        /// <value>The worker connection string.</value>
        public string? WorkerConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the worker.
        /// </summary>
        /// <value>The type of the worker.</value>
        public string WorkerType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string? path = null) where TConnection : ScenarioServiceConnection
        {
            var connectionType = new ConnectionType("ScenarioServiceConnection", typeof(TConnection));

            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", ScenarioService.GetRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("ModelDataReaderRepositoryType", ModelDataReaderService.GetRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("WorkerType", ScenarioService.GetWorkerTypes(path)));

            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ModelDataReaderRepositoryConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("WorkerConnectionString", typeof(string), false));

            return connectionType;
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
                var modelDataReaderRepositoryType = Type.GetType(ModelDataReaderRepositoryType, true);
                var workerType = Type.GetType(WorkerType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                var modelDataReaderRepository = Activator.CreateInstance(modelDataReaderRepositoryType, ModelDataReaderConnectionString);
                var worker = WorkerConnectionString != null ? Activator.CreateInstance(workerType, WorkerConnectionString) : Activator.CreateInstance(workerType);

                return new ScenarioService((IScenarioRepository)repository, new ModelDataReaderService((IModelDataReaderRepository)modelDataReaderRepository), (IScenarioWorker)worker);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
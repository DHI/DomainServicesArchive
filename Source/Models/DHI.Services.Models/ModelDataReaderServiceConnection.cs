namespace DHI.Services.Models
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Model data reader service connection
    /// </summary>
    public class ModelDataReaderServiceConnection : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="repositoryType">The repository type.</param>
        /// <param name="connectionString">The repository connection string.</param>
        public ModelDataReaderServiceConnection(string id, string name, string repositoryType, string connectionString) : base(id, name)
        {
            Guard.Against.NullOrEmpty(repositoryType, nameof(repositoryType));
            Guard.Against.NullOrEmpty(connectionString, nameof(connectionString));
            RepositoryType = repositoryType;
            ConnectionString = connectionString;
        }

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
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string? path = null) where TConnection : ModelDataReaderServiceConnection
        {
            var connectionType = new ConnectionType("ModelDataReaderServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", ModelDataReaderService.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a ModelDataReaderService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new ModelDataReaderService((IModelDataReaderRepository)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
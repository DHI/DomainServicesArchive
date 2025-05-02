namespace DHI.Services.GIS
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class UpdatableGisServiceConnection.
    /// </summary>
    /// <typeparam name="TCollectionId">The type of the feature collection identifier.</typeparam>
    /// <typeparam name="TFeatureId">The type of the feature identifier</typeparam>
    public class UpdatableGisServiceConnection<TCollectionId, TFeatureId> : BaseConnection 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref=" UpdatableGisServiceConnection{TCollectionId, TFeatureId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public UpdatableGisServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string RepositoryType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : UpdatableGisServiceConnection<TCollectionId, TFeatureId>
        {
            var connectionType = new ConnectionType("UpdatableGisServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", UpdatableGisService<TCollectionId, TFeatureId>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a UpdatableGisService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new UpdatableGisService<TCollectionId, TFeatureId>((IUpdatableGisRepository<TCollectionId, TFeatureId>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
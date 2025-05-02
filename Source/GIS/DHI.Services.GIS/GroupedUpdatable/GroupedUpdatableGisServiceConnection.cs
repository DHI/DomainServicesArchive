namespace DHI.Services.GIS
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Class GroupedUpdatableGisServiceConnection.
    /// </summary>
    /// <typeparam name="TCollectionId">The type of the feature collection identifier.</typeparam>
    /// <typeparam name="TFeatureId">The type of the feature identifier</typeparam>
    public class GroupedUpdatableGisServiceConnection<TCollectionId, TFeatureId> : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref=" GroupedUpdatableGisServiceConnection{TCollectionId, TFeatureId}" />
        ///     class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedUpdatableGisServiceConnection(string id, string name)
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
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : GroupedUpdatableGisServiceConnection<TCollectionId, TFeatureId>
        {
            var connectionType = new ConnectionType("GroupedUpdatableGisServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", GroupedUpdatableGisService<TCollectionId, TFeatureId>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a GroupedUpdatableGisService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new GroupedUpdatableGisService<TCollectionId, TFeatureId>((IGroupedUpdatableGisRepository<TCollectionId, TFeatureId>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class MapStyleServiceConnection.
    /// </summary>
    public class MapStyleServiceConnection : BaseConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapStyleServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public MapStyleServiceConnection(string id, string name)
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
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType(string path = null)
        {
            var connectionType = new ConnectionType("MapStyleServiceConnection", typeof (MapStyleService));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", MapStyleService.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a MapStyleService service instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new MapStyleService((IMapStyleRepository)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
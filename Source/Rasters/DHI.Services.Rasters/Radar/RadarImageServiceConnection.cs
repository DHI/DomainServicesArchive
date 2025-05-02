namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class RadarImageServiceConnection.
    /// </summary>
    /// <typeparam name="TImage">The type of the radar image</typeparam>
    public class RadarImageServiceConnection<TImage> : BaseConnection where TImage : IRadarImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadarImageServiceConnection{TImage}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public RadarImageServiceConnection(string id, string name) : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; }

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
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : RadarImageServiceConnection<TImage>
        {
            var connectionType = new ConnectionType($"RadarImageServiceConnection_{typeof(TImage).FullName}", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", RadarImageService<TImage>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a RadarImageService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new RadarImageService<TImage>((IRasterRepository<TImage>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
namespace DHI.Services.Authentication
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Class RefreshTokenServiceConnection.
    /// </summary>
    /// <seealso cref="DHI.Services.BaseConnection" />
    [Obsolete("In the newest Web API component, the RefreshToken Service is injected through the controller constructor using standard ASP.NET DI. This type will eventually be removed.")]
    public class RefreshTokenServiceConnection : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshTokenServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public RefreshTokenServiceConnection(string id, string name)
            : base(id, name)
        {
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
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : RefreshTokenServiceConnection
        {
            var connectionType = new ConnectionType("RefreshTokenServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", RefreshTokenService.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a RefreshToken service instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IRefreshTokenRepository)Activator.CreateInstance(repositoryType, ConnectionString);
                return new RefreshTokenService(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
namespace DHI.Services.Accounts
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Class AccountServiceConnection.
    /// </summary>
    /// <seealso cref="DHI.Services.BaseConnection" />
    [Obsolete("In the newest Web API component, the Account Service is injected through the controller constructor using standard ASP.NET DI. This type will eventually be removed.")]
    public class AccountServiceConnection : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public AccountServiceConnection(string id, string name)
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
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : AccountServiceConnection
        {
            var connectionType = new ConnectionType("AccountServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", AccountService.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates an account service instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IAccountRepository)Activator.CreateInstance(repositoryType, ConnectionString);
                return new AccountService(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
namespace DHI.Services.Scalars
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class GroupedScalarServiceConnection.
    /// </summary>
    /// <typeparam name="TId">The type of the scalar identifier.</typeparam>
    /// <typeparam name="TFlag">The type of the scalar quality flag.</typeparam>
    public class GroupedScalarServiceConnection<TId, TFlag> : BaseConnection where TFlag : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedScalarServiceConnection{TId, TEntity}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedScalarServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the repository connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string RepositoryConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string RepositoryType { get; set; }

        /// <summary>
        /// Gets or sets the logger connection string.
        /// </summary>
        /// <value>The logger connection string.</value>
        public virtual string LoggerConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the logger type.
        /// </summary>
        /// <value>The type of the logger.</value>
        public string LoggerType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : GroupedScalarServiceConnection<TId, TFlag>
        {
            var connectionType = new ConnectionType("GroupedScalarServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", GroupedScalarService<TId, TFlag>.GetRepositoryTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("LoggerType", Logger.GetLoggerTypes(path), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("RepositoryConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("LoggerConnectionString", typeof(string), false));
            return connectionType;
        }

        /// <summary>
        /// Creates a GroupedScalarService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, RepositoryConnectionString);
                if (LoggerType is null)
                {
                    return new GroupedScalarService<TId, TFlag>((IGroupedScalarRepository<TId, TFlag>)repository);
                }

                var loggerType = Type.GetType(LoggerType, true);
                var logger = Activator.CreateInstance(loggerType, LoggerConnectionString);
                return new GroupedScalarService<TId, TFlag>((IGroupedScalarRepository<TId, TFlag>)repository, (ILogger)logger);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
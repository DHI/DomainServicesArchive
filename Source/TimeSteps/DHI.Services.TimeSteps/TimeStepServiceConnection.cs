namespace DHI.Services.TimeSteps
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class TimeStepServiceConnection.
    /// </summary>
    /// <typeparam name="TItemId">The type of the item identifier.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class TimeStepServiceConnection<TItemId, TValue> : BaseConnection where TValue : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeStepServiceConnection{TItemId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TimeStepServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the server.
        /// </summary>
        /// <value>The type of the server.</value>
        public string ServerType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : TimeStepServiceConnection<TItemId, TValue>
        {
            var connectionType = new ConnectionType("TimeStepServiceConnection", typeof (TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("ServerType", TimeStepService<TItemId, TValue>.GetServerTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof (string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a TimeStepService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var serverType = Type.GetType(ServerType, true);
                var repository = Activator.CreateInstance(serverType, ConnectionString);
                return new TimeStepService<TItemId, TValue>((ITimeStepServer<TItemId, TValue>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
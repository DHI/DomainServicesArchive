namespace DHI.Services.TimeSeries
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Class TimeSeriesServiceConnection.
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public class TimeSeriesServiceConnection<TId, TValue> : BaseConnection where TValue : struct, IComparable<TValue>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesServiceConnection{TId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TimeSeriesServiceConnection(string id, string name)
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
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : TimeSeriesServiceConnection<TId, TValue>
        {
            var connectionType = new ConnectionType("TimeSeriesServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", TimeSeriesService<TId, TValue>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a TimeSeriesService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                ITimeSeriesRepository<TId, TValue> repository;
                if (ConnectionString is null)
                {
                    repository = (ITimeSeriesRepository<TId, TValue>)Activator.CreateInstance(repositoryType);
                }
                else
                {
                    repository = (ITimeSeriesRepository<TId, TValue>)Activator.CreateInstance(repositoryType, ConnectionString);
                }

                return new TimeSeriesService<TId, TValue>(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
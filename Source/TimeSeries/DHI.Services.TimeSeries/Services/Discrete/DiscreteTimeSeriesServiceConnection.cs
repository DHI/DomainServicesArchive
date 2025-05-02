namespace DHI.Services.TimeSeries
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Class DiscreteTimeSeriesServiceConnection.
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public class DiscreteTimeSeriesServiceConnection<TId, TValue> : BaseConnection where TValue : struct, IComparable<TValue>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscreteTimeSeriesServiceConnection{TId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="name">The name.</param>
        public DiscreteTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the repository.
        /// </summary>
        public string RepositoryType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : DiscreteTimeSeriesServiceConnection<TId, TValue>
        {
            var connectionType = new ConnectionType("DiscreteTimeSeriesServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", DiscreteTimeSeriesService<TId, TValue>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a DiscreteTimeSeriesService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new DiscreteTimeSeriesService<TId, TValue>((IDiscreteTimeSeriesRepository<TId, TValue>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
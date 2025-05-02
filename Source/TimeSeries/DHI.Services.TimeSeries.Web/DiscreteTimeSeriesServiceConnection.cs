namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     DiscreteTimeSeriesServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class DiscreteTimeSeriesServiceConnection : DiscreteTimeSeriesServiceConnection<string, double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscreteTimeSeriesServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public DiscreteTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a DiscreteTimeSeriesService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new DiscreteTimeSeriesService<string, double>((IDiscreteTimeSeriesRepository<string, double>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
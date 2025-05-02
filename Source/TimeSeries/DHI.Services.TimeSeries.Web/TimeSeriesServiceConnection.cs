namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     TimeSeriesServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class TimeSeriesServiceConnection : TimeSeriesServiceConnection<string, double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a TimeSeriesService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new TimeSeriesService<string, double>((ITimeSeriesRepository<string, double>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
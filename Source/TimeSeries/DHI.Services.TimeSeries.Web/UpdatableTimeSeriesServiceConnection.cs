namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     UpdatableTimeSeriesServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class UpdatableTimeSeriesServiceConnection : UpdatableTimeSeriesServiceConnection<string, double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdatableTimeSeriesServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public UpdatableTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a UpdatableTimeSeriesService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new UpdatableTimeSeriesService<string, double>((IUpdatableTimeSeriesRepository<string, double>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
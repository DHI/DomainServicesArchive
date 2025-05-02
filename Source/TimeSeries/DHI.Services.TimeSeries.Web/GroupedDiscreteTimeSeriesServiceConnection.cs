namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     GroupedDiscreteTimeSeriesServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class GroupedDiscreteTimeSeriesServiceConnection : GroupedDiscreteTimeSeriesServiceConnection<string, double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedDiscreteTimeSeriesServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedDiscreteTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a GroupedDiscreteTimeSeriesService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new GroupedDiscreteTimeSeriesService<string, double>((IGroupedDiscreteTimeSeriesRepository<string, double>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
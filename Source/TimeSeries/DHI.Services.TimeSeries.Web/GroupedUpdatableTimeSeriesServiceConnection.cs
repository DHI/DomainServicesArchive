namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     GroupedUpdatableTimeSeriesServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class GroupedUpdatableTimeSeriesServiceConnection : GroupedUpdatableTimeSeriesServiceConnection<string, double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedUpdatableTimeSeriesServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedUpdatableTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a GroupedUpdatableTimeSeriesService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new GroupedUpdatableTimeSeriesService<string, double>((IGroupedUpdatableTimeSeriesRepository<string, double>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
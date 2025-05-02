namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;

    /// <summary>
    ///     GroupedUpdatableGisServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class GroupedUpdatableGisServiceConnection : GroupedUpdatableGisServiceConnection<string, Guid>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedUpdatableGisServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedUpdatableGisServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a GroupedUpatableGisService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new GroupedUpdatableGisService<string, Guid>((IGroupedUpdatableGisRepository<string, Guid>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
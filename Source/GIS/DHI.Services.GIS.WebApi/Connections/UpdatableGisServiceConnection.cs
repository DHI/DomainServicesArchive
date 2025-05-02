namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using GIS;
    using WebApiCore;

    /// <summary>
    ///     UpdatableGisServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class UpdatableGisServiceConnection : UpdatableGisServiceConnection<string, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableGisServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public UpdatableGisServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a UpdatableGisService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new UpdatableGisService<string, Guid>((IUpdatableGisRepository<string, Guid>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
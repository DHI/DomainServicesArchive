namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;

    /// <summary>
    ///     GisServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class GisServiceConnection : GisServiceConnection<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GisServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GisServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a GisService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new GisService<string>((IGisRepository<string>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
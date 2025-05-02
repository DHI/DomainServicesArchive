namespace DHI.Services.Tables.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Tables;
    using WebApiCore;

    /// <summary>
    ///     TableServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="DHI.Services.Tables.TableServiceConnection" />
    public class TableServiceConnection : Tables.TableServiceConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TableServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates an TableService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new TableService((ITableRepository)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
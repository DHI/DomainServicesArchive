namespace DHI.Services.Documents.WebApi
{
    using WebApiCore;
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     GroupedDocumentServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class GroupedDocumentServiceConnection : GroupedDocumentServiceConnection<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedDocumentServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedDocumentServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a GroupedDocumentService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new GroupedDocumentService<string>((IGroupedDocumentRepository<string>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}

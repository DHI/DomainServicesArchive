namespace DHI.Services.Documents.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;

    /// <summary>
    ///     DocumentServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class DocumentServiceConnection : DocumentServiceConnection<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DocumentServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public DocumentServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a DocumentService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new DocumentService<string>((IDocumentRepository<string>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
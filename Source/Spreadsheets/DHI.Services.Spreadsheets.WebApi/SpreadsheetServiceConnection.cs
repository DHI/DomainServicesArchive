namespace DHI.Services.Spreadsheets.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;

    /// <summary>
    ///     SpreadsheetServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class SpreadsheetServiceConnection : SpreadsheetServiceConnection<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpreadsheetServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public SpreadsheetServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a SpreadsheetService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new SpreadsheetService((ISpreadsheetRepository<string>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
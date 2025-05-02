namespace DHI.Services.Spreadsheets
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Class SpreadsheetServiceConnection.
    /// </summary>
    /// <typeparam name="TId">The type of the spreadsheet identifier.</typeparam>
    public class SpreadsheetServiceConnection<TId> : BaseConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpreadsheetServiceConnection{TId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public SpreadsheetServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string RepositoryType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : SpreadsheetServiceConnection<TId>
        {
            var connectionType = new ConnectionType("SpreadsheetServiceConnection", typeof (TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", SpreadsheetService<TId>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof (string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a SpreadsheetService instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);
                return new SpreadsheetService<TId>((ISpreadsheetRepository<TId>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
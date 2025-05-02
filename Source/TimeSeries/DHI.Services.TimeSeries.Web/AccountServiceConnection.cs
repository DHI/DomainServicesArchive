namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Accounts;

    /// <summary>
    ///     AccountServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="DHI.Services.Accounts.AccountServiceConnection" />
    public class AccountServiceConnection : Accounts.AccountServiceConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public AccountServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates an AccountService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IAccountRepository)Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new AccountService(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
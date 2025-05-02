namespace DHI.Services.Scalars.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Logging;
    using Microsoft.Extensions.Logging;
    using WebApiCore;

    /// <summary>
    ///     ScalarServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class ScalarServiceConnection : ScalarServiceConnection<string, int>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScalarServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public ScalarServiceConnection(string id, string name) : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a ScalarService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, RepositoryConnectionString.Resolve());
                if (LoggerType is null)
                {
                    return new ScalarService<string, int>((IScalarRepository<string, int>)repository);
                }

                var loggerType = Type.GetType(LoggerType, true);
                var logger = Activator.CreateInstance(loggerType, LoggerConnectionString.Resolve());
                return new ScalarService<string, int>((IScalarRepository<string, int>)repository, (ILogger)logger);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
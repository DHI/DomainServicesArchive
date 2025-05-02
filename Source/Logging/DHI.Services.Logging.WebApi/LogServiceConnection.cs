namespace DHI.Services.Logging.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Text.Json.Serialization;
    using Logging;
    using WebApiCore;

    /// <summary>
    ///     LogServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="LogServiceConnection" />
    public class LogServiceConnection : BaseConnection
    {
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public LogServiceConnection(string id, string name) : base(id, name)
        {
        }


        /// <summary>
        ///     Creates a LogService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var logRepository = new ClefLogRepository(ConnectionString.Resolve());
                return new LogService(logRepository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
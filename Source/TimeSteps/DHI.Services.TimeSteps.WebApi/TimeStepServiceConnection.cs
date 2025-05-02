namespace DHI.Services.TimeSteps.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;

    /// <summary>
    ///     TimeStepServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class TimeStepServiceConnection : TimeStepServiceConnection<string, object>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeStepServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TimeStepServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a TimeStepService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var serverType = Type.GetType(ServerType, true);
                var repository = Activator.CreateInstance(serverType, ConnectionString.Resolve());
                return new TimeStepService<string, object>((ITimeStepServer<string, object>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
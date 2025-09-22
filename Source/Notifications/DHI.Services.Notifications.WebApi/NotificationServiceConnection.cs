namespace DHI.Services.Notifications.WebApi
{
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using WebApiCore;
    /// <summary>
    /// NotificationServiceConnection supporting connection string resolvation of [AppData]
    /// </summary>
    public class NotificationServiceConnection : BaseConnection
    {
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public NotificationServiceConnection(string id, string name) : base(id, name)
        {
        }


        /// <summary>
        ///     Creates a NotificationService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var notificationRepository = new NotificationRepository(ConnectionString.Resolve());
                return new NotificationService(notificationRepository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}

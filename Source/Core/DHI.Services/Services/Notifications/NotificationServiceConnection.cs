namespace DHI.Services.Notifications
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Connection class for the notification service.
    /// </summary>
    public class NotificationServiceConnection : BaseConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public NotificationServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the notification repository.
        /// </summary>
        /// <value>The type of the notification repository.</value>
        [JsonPropertyName("RepositoryType")]
        public string NotificationRepositoryType { get; set; }

        /// <summary>
        /// Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : NotificationServiceConnection
        {
            var connectionType = new ConnectionType("NotificationServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("NotificationRepositoryType", NotificationService.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        /// Creates a notification service instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(NotificationRepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString);

                var type = repository.GetType();
                if (typeof(INotificationRepository).IsAssignableFrom(type) == false)
                {
                    throw new Exception($"Repository type should be implementing '{typeof(INotificationRepository)}' type. Existing repository type value is '{NotificationRepositoryType ?? "null"})' and created as '{type}' type.");
                }
                return new NotificationService((INotificationRepository)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
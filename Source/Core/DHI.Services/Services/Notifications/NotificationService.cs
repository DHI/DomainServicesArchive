namespace DHI.Services.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Notification Service.
    /// </summary>
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationService" /> class.
        /// </summary>
        /// <param name="notificationRepository">The notification repository.</param>
        /// <exception cref="ArgumentNullException">notificationRepository</exception>
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<INotificationRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<INotificationRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<INotificationRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Gets an array of notification entries fulfilling the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>NotificationEntry[].</returns>
        public NotificationEntry[] Get(IEnumerable<QueryCondition> query)
        {
            return _notificationRepository.Get(query).ToArray();
        }

        /// <summary>
        ///     Gets the last notification entry that fulfills the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns><see cref="NotificationEntry"/>.</returns>
        public NotificationEntry Last(IEnumerable<QueryCondition> query)
        {
            return _notificationRepository.Last(query) | default(NotificationEntry);
        }

        /// <summary>
        ///     Writes a <see cref="NotificationEntry"/> to the <see cref="INotificationRepository">repository</see>.
        /// </summary>
        /// <param name="notificationEntry">The <see cref="NotificationEntry" /> to be logged.</param>
        public void Add(NotificationEntry notificationEntry)
        {
            _notificationRepository.Add(notificationEntry);
        }
    }
}
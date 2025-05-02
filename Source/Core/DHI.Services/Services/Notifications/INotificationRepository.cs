namespace DHI.Services.Notifications
{
    /// <summary>
    ///     Notification repository abstraction
    /// </summary>
    public interface INotificationRepository : INotificationReader
    {
        /// <summary>
        ///     Adds the specified entry.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        void Add(NotificationEntry entry);
    }
}
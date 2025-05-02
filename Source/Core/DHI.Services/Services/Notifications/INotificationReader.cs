namespace DHI.Services.Notifications
{
    using System.Collections.Generic;

    /// <summary>
    ///     Notification reader abstraction
    /// </summary>
    public interface INotificationReader
    {
        /// <summary>
        ///     Queries the notification using a list of query conditions.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns><see cref="IEnumerable{T}"/> of type <see cref="NotificationEntry"/></returns>
        IEnumerable<NotificationEntry> Get(IEnumerable<QueryCondition> query);

        /// <summary>
        ///     Queries the last notification entry using a list of query conditions.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns><see cref="NotificationEntry"/></returns>
        Maybe<NotificationEntry> Last(IEnumerable<QueryCondition> query);
    }
}
namespace DHI.Services.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Converters;

    /// <summary>
    ///     Class for adding notifications to a JSON file.
    /// </summary>
    public class JsonNotificationRepository : JsonRepository<NotificationEntry, Guid>, INotificationRepository
    {
        private static readonly IEnumerable<JsonConverter> _defaultJsonConverters = new List<JsonConverter>()
        {
            new DictionaryTypeResolverConverter<Guid, NotificationEntry>(),
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonNotificationRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public JsonNotificationRepository(string filePath) : base(filePath, _defaultJsonConverters)
        {
        }

        /// <summary>
        ///     Queries the notification using a list of query conditions.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IEnumerable&lt;NotificationEntry&gt;.</returns>
        public IEnumerable<NotificationEntry> Get(IEnumerable<QueryCondition> query)
        {
            var predicate = ExpressionBuilder.Build<NotificationEntry>(query);
            return Get(predicate);
        }

        /// <summary>
        ///     Queries the last notification entry using a list of query conditions.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IEnumerable&ltNotificationgEntry&gt;.</returns>
        public Maybe<NotificationEntry> Last(IEnumerable<QueryCondition> query)
        {
            return Count() == 0 ? Maybe.Empty<NotificationEntry>() : Get(query).OrderByDescending(entry => entry.DateTime).FirstOrDefault().ToMaybe();
        }

        /// <summary>
        ///     Adds the specified entry.
        /// </summary>
        /// <param name="entry">The entry</param>
        public void Add(NotificationEntry entry)
        {
            base.Add(entry);
        }
    }
}
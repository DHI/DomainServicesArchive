namespace DHI.Services.Notifications
{
    using System.Collections.Generic;
    using System.Linq;

    public class FakeNotificationRepository : INotificationRepository
    {
        private readonly List<NotificationEntry> _logEntries;

        public FakeNotificationRepository()
        {
            _logEntries = new List<NotificationEntry>();
        }

        public FakeNotificationRepository(IEnumerable<NotificationEntry> logEntries)
            : this()
        {
            foreach (var logEntry in logEntries)
            {
                _logEntries.Add(logEntry);
            }
        }

        public IEnumerable<NotificationEntry> Get(IEnumerable<QueryCondition> query)
        {
            var predicate = ExpressionBuilder.Build<NotificationEntry>(query);
            return _logEntries.AsQueryable().Where(predicate).ToArray();
        }

        public Maybe<NotificationEntry> Last(IEnumerable<QueryCondition> query)
        {
            var predicate = ExpressionBuilder.Build<NotificationEntry>(query);
            return _logEntries.AsQueryable().Where(predicate).Last().ToMaybe();
        }

        public void Add(NotificationEntry notificationEntry)
        {
            _logEntries.Add(notificationEntry);
        }
    }
}
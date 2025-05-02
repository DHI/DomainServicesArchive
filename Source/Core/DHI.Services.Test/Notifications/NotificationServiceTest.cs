namespace DHI.Services.Test.Notifications
{
    using System;
    using System.Collections.Generic;
    using DHI.Services.Notifications;
    using Xunit;

    public class NotificationServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationService(null));
        }

        [Fact]
        public void GetIsOk()
        {
            var entries = new List<NotificationEntry>
            {
                new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))),
                new NotificationEntry(NotificationLevel.Error, "second error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                new NotificationEntry(NotificationLevel.Information, "first info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                new NotificationEntry(NotificationLevel.Error, "third error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))),
                new NotificationEntry(NotificationLevel.Information, "second info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            };
            var repository = new FakeNotificationRepository(entries);
            var service = new NotificationService(repository);

            var queryErrors = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Error) };
            Assert.Equal(3, service.Get(queryErrors).Length);

            var queryInfos = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Information) };
            Assert.Equal(2, service.Get(queryInfos).Length);

            var queryErrorsLastTwoDays = new List<QueryCondition>
            {
                new QueryCondition("NotificationLevel", QueryOperator.Equal, NotificationLevel.Error),
                new QueryCondition("DateTime", QueryOperator.GreaterThan, DateTime.Now.Subtract(TimeSpan.FromDays(2.1)))
            };
            Assert.Equal(2, service.Get(queryErrorsLastTwoDays).Length);
        }

        [Fact]
        public void LastIsOk()
        {
            var entries = new List<NotificationEntry>
            {
                new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))),
                new NotificationEntry(NotificationLevel.Error, "second error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                new NotificationEntry(NotificationLevel.Information, "first info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                new NotificationEntry(NotificationLevel.Error, "third error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))),
                new NotificationEntry(NotificationLevel.Information, "second info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            };
            var repository = new FakeNotificationRepository(entries);
            var service = new NotificationService(repository);

            var queryErrors = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Error) };
            Assert.Equal("third error", service.Last(queryErrors).Text);

            var queryInfos = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Information) };
            Assert.Equal("second info", service.Last(queryInfos).Text);
            
            var queryErrorsOlderThanTwoDays = new List<QueryCondition>
            {
                new QueryCondition("NotificationLevel", QueryOperator.Equal, NotificationLevel.Error),
                new QueryCondition("DateTime", QueryOperator.LessThan, DateTime.Now.Subtract(TimeSpan.FromDays(2.1)))
            };
            Assert.Equal("first error", service.Last(queryErrorsOlderThanTwoDays).Text);
        }

        [Fact]
        public void AddIsOk()
        {
            var repository = new FakeNotificationRepository();
            var service = new NotificationService(repository);

            service.Add(new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))));
            var queryErrors = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Error) };
            Assert.Single(service.Get(queryErrors));
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var types = NotificationService.GetRepositoryTypes();

            Assert.Contains(typeof(FakeNotificationRepository), types);
        }
    }
}
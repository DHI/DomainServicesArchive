namespace DHI.Services.Test.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoFixture;
    using DHI.Services.Notifications;
    using Xunit;

    public class JsonNotificationRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__log.json");
        private readonly string _existingLogger = Path.Combine(Path.GetTempPath(), "__existing_log.json");
        private readonly JsonNotificationRepository _notificationRepository;
        private readonly Fixture _fixture;

        public JsonNotificationRepositoryTest()
        {
            _notificationRepository = new JsonNotificationRepository(_filePath);
            _fixture = new Fixture();
            _fixture.Register(() => NotificationLevel.Information);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
            File.Delete(_existingLogger);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonNotificationRepository(null));
        }

        [Fact]
        public void AddAndGetIsOk()
        {
            var entry = _fixture.Create<NotificationEntry>();
            _notificationRepository.Add(entry);
            var actual = _notificationRepository.Get(entry.Id).Value;
            Assert.Equal(entry.Id, actual.Id);
        }

        [Fact]
        public void AddAndGetWithMetadataIsOk()
        {
            var entry = new NotificationEntry(
                Guid.NewGuid(),
                NotificationLevel.Error,
                "first error",
                "my-source",
                "my-tag",
                "my-machine",
                DateTime.Now,
                new Dictionary<string, object> { { "Description", "My notification entry description" } }
            );
            _notificationRepository.Add(entry);
            Assert.True(entry.Metadata.ContainsKey("Description"));
            Assert.Equal("My notification entry description", entry.Metadata["Description"]);
        }

        [Fact]
        public void GetByQueryIsOk()
        {
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "second error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "first info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "third error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "second info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));

            var queryErrors = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Error) };
            Assert.Equal(3, _notificationRepository.Get(queryErrors).ToArray().Length);

            var queryInfos = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Information) };
            Assert.Equal(2, _notificationRepository.Get(queryInfos).Count());

            var queryErrorsLastTwoDays = new List<QueryCondition>
            {
                new QueryCondition("NotificationLevel", QueryOperator.Equal, NotificationLevel.Error),
                new QueryCondition("DateTime", QueryOperator.GreaterThan, DateTime.Now.Subtract(TimeSpan.FromDays(2.1)))
            };
            Assert.Equal(2, _notificationRepository.Get(queryErrorsLastTwoDays).Count());
        }

        [Fact]
        public void GetByQueryUsingAddLevelComparisonIsOk()
        {
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Warning, "first warning", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "first info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "second error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "second info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));

            var query = new List<QueryCondition> { new QueryCondition("NotificationLevel", QueryOperator.GreaterThan, NotificationLevel.Information) };
            Assert.Equal(3, _notificationRepository.Get(query).ToArray().Length);

            query = new List<QueryCondition> { new QueryCondition("NotificationLevel", QueryOperator.LessThanOrEqual, NotificationLevel.Warning) };
            Assert.Equal(3, _notificationRepository.Get(query).ToArray().Length);
        }

        [Fact]
        public void LastIsOk()
        {
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "first error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(3))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "second error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "first info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(2))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Error, "third error", "my-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));
            _notificationRepository.Add(new NotificationEntry(NotificationLevel.Information, "second info", "my-other-source", "my-tag", "my-machine", DateTime.Now.Subtract(TimeSpan.FromDays(1))));

            var queryErrors = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Error) };
            Assert.Equal("third error", _notificationRepository.Last(queryErrors).Value.Text);

            var queryInfos = new List<QueryCondition> { new QueryCondition("NotificationLevel", NotificationLevel.Information) };
            Assert.Equal("second info", _notificationRepository.Last(queryInfos).Value.Text);

            var queryErrorsOlderThanTwoDays = new List<QueryCondition>
            {
                new QueryCondition("NotificationLevel", QueryOperator.Equal, NotificationLevel.Error),
                new QueryCondition("DateTime", QueryOperator.LessThan, DateTime.Now.Subtract(TimeSpan.FromDays(2.1)))
            };
            Assert.Equal("first error", _notificationRepository.Last(queryErrorsOlderThanTwoDays).Value.Text);
        }

        [Fact]
        public void GetByQueryReturnsImmutables()
        {
            var entry = _fixture.Create<NotificationEntry>();
            _notificationRepository.Add(entry);
            Assert.Throws<NotSupportedException>(() => entry.Metadata.Add("Description", "MyDescription"));
        }

        [Fact]
        public void CreateRepositoryViaReflectionIsOk()
        {
            File.Copy("../../../Data/log.json", _existingLogger, true);
            const string repositoryType = "DHI.Services.Notifications.JsonNotificationRepository, DHI.Services";
            var args = new object[] { _existingLogger };

            var notificationRepositoryType = Type.GetType(repositoryType, true);
            var repository = Activator.CreateInstance(notificationRepositoryType, args);

            Assert.True(repository is JsonNotificationRepository);
            Assert.NotEmpty(((JsonNotificationRepository)repository).GetAll());
        }
    }
}
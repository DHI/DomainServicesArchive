namespace DHI.Services.JsonDocuments.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Argon;
    using AutoFixture.Xunit2;
    using Logging;
    using Notifications;
    using Xunit;

    public class JsonDocumentServiceTest
    {
        private const int _repeatCount = 3;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonDocumentService(null));
        }

        [Theory, AutoJsonDocumentData]
        public void CreateOverloadWithNullLogRepositoryThrows(IJsonDocumentRepository<string> repository)
        {
            Assert.Throws<ArgumentNullException>(() => new JsonDocumentService(repository, null));
        }

        [Theory, AutoJsonDocumentData]
        public void GetNonExistingThrows(JsonDocumentService jsonDocumentService)
        {
            Assert.Throws<KeyNotFoundException>(() => jsonDocumentService.Get("nonExistingJsonDocument"));
        }

        [Theory, AutoJsonDocumentData]
        public void UpdateNonExistingThrows(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            Assert.Throws<KeyNotFoundException>(() => jsonDocumentService.Update(jsonDocument));
        }

        [Theory, AutoJsonDocumentData]
        public void RemoveNonExistingThrows(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            Assert.Throws<KeyNotFoundException>(() => jsonDocumentService.Remove(jsonDocument.Id));
        }

        [Theory, AutoJsonDocumentData]
        public void AddExistingThrows(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocumentService.Add(jsonDocument);
            Assert.Throws<ArgumentException>(() => jsonDocumentService.Add(jsonDocument));
        }

        [Fact]
        public void GetLogEntriesThrowsIfNoLog()
        {
            var jsonDocumentService = new JsonDocumentService(new FakeJsonDocumentRepository());
            Assert.Throws<NotSupportedException>(() => jsonDocumentService.GetNotificationEntries("myDocument"));
        }

        [Theory, AutoJsonDocumentData]
        public void GetByGroupForNonExistingThrows(JsonDocumentService jsonDocumentService)
        {
            Assert.Throws<KeyNotFoundException>(() => jsonDocumentService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoJsonDocumentData]
        public void GetByGroupForNullGroupThrows(JsonDocumentService jsonDocumentService)
        {
            Assert.Throws<ArgumentNullException>(() => jsonDocumentService.GetByGroup(null));
        }

        [Theory, AutoJsonDocumentData]
        public void GetFullNamesForNonExistingGroupThrows(JsonDocumentService jsonDocumentService)
        {
            Assert.Throws<KeyNotFoundException>(() => jsonDocumentService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoJsonDocumentData]
        public void GetFullNamesForNullOrEmptyGroupThrows(JsonDocumentService jsonDocumentService)
        {
            Assert.Throws<ArgumentNullException>(() => jsonDocumentService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => jsonDocumentService.GetFullNames(""));
        }

        [Theory, AutoJsonDocumentData]
        public void GetByGroupIsOk(JsonDocumentService jsonDocumentService)
        {
            var group = jsonDocumentService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(jsonDocumentService.GetByGroup(group).Any());
        }

        [Theory, AutoJsonDocumentData]
        public void GetFullNamesByGroupIsOk(JsonDocumentService jsonDocumentService)
        {
            var group = jsonDocumentService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = jsonDocumentService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoJsonDocumentData]
        public void GetFullNamesIsOk(JsonDocumentService jsonDocumentService)
        {
            Assert.Equal(_repeatCount, jsonDocumentService.GetFullNames().Count());
        }

        [Theory, AutoJsonDocumentData]
        public void GetAllIsOk(JsonDocumentService jsonDocumentService)
        {
            Assert.Equal(_repeatCount, jsonDocumentService.GetAll().Count());
        }

        [Theory, AutoJsonDocumentData]
        public void GetIdsIsOk(JsonDocumentService jsonDocumentService)
        {
            Assert.Equal(_repeatCount, jsonDocumentService.GetIds().Count());
        }

        [Theory, AutoJsonDocumentData]
        public void GetInIntervalIsOk(JsonDocumentService jsonDocumentService)
        {
            var documents = jsonDocumentService.GetAll().ToList();
            var from = documents.Select(s => s.DateTime).Min().Value.AddSeconds(1);
            var to = documents.Select(s => s.DateTime).Max().Value.AddSeconds(-1);
            Assert.Single(jsonDocumentService.Get(from, to));
        }

        [Theory, AutoJsonDocumentData]
        public void GetByQueryIsOk(JsonDocumentService jsonDocumentService)
        {
            var document = jsonDocumentService.GetAll().First();
            var query = new Query<JsonDocument<string>>
            {
                new QueryCondition("Id", QueryOperator.Equal, document.Id)
            };

            Assert.Single(jsonDocumentService.Get(query));
        }

        [Theory, AutoJsonDocumentData]
        public void AddAndGetIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocumentService.Add(jsonDocument);
            var document = jsonDocumentService.Get(jsonDocument.Id);

            Assert.Equal(jsonDocument.Id, document.Id);
            Assert.Null(document.Updated);
            Assert.NotNull(document.Added);
            Assert.InRange(document.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoJsonDocumentData]
        public void CountIsOk(JsonDocumentService jsonDocumentService)
        {
            Assert.Equal(_repeatCount, jsonDocumentService.Count());
        }

        [Theory, AutoJsonDocumentData]
        public void ExistsIsOk(JsonDocumentService jsonDocumentService)
        {
            var jsonDocument = jsonDocumentService.GetAll().ToArray()[0];
            Assert.True(jsonDocumentService.Exists(jsonDocument.Id));
        }

        [Theory, AutoJsonDocumentData]
        public void DoesNotExistIsOk(JsonDocumentService jsonDocumentService)
        {
            Assert.False(jsonDocumentService.Exists("NonExistingJsonDocument"));
        }

        [Theory, AutoJsonDocumentData]
        public void EventsAreRaisedOnAdd(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            var raisedEvents = new List<string>();
            jsonDocumentService.Adding += (_, _) => { raisedEvents.Add("Adding"); };
            jsonDocumentService.Added += (_, _) => { raisedEvents.Add("Added"); };

            jsonDocumentService.Add(jsonDocument);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoJsonDocumentData]
        public void RemoveIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocumentService.Add(jsonDocument);
            jsonDocumentService.Remove(jsonDocument.Id);

            Assert.False(jsonDocumentService.Exists(jsonDocument.Id));
        }

        [Theory, AutoJsonDocumentData]
        public void EventsAreRaisedOnRemove(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            var raisedEvents = new List<string>();
            jsonDocumentService.Deleting += (_, _) => { raisedEvents.Add("Deleting"); };
            jsonDocumentService.Deleted += (_, _) => { raisedEvents.Add("Deleted"); };
            jsonDocumentService.Add(jsonDocument);

            jsonDocumentService.Remove(jsonDocument.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoJsonDocumentData]
        public void UpdateIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocumentService.Add(jsonDocument);
            jsonDocument.Updated = DateTime.Now;
            jsonDocumentService.Update(jsonDocument);
            var doc = jsonDocumentService.Get(jsonDocument.Id);

            Assert.Equal(jsonDocument.Name, doc.Name);
            Assert.NotNull(doc.Updated);
            Assert.Equal(doc.Updated.Value, jsonDocument.Updated.Value);
        }

        [Theory, AutoJsonDocumentData]
        public void AddOrUpdateIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            var raisedEvents = new List<string>();
            jsonDocumentService.Added += (_, _) => { raisedEvents.Add("Added"); };
            jsonDocumentService.Updated += (_, _) => { raisedEvents.Add("Updated"); };
            jsonDocumentService.AddOrUpdate(jsonDocument);
            var document = new JsonDocument(jsonDocument.Id, jsonDocument.Name, jsonDocument.Group, "{ \"string\": \"Howdy World\" }")
            {
                Updated = DateTime.Now
            };
            jsonDocumentService.AddOrUpdate(document);
            var doc = jsonDocumentService.Get(jsonDocument.Id);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            var o = (JObject) JsonConvert.DeserializeObject<object>(doc.Data);
            Assert.Equal("Howdy World", (string) o.SelectToken("string"));
            Assert.Equal(document.Name, doc.Name);
            Assert.NotNull(doc.Updated);
            Assert.Equal(doc.Updated.Value, document.Updated.Value);
            Assert.NotNull(doc.Added);
            Assert.InRange(doc.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoJsonDocumentData]
        public void TryAddIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            Assert.True(jsonDocumentService.TryAdd(jsonDocument));
            var document = jsonDocumentService.Get(jsonDocument.Id);
            Assert.Equal(jsonDocument.Id, document.Id);
            Assert.Null(document.Updated);
            Assert.NotNull(document.Added);
            Assert.InRange(document.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoJsonDocumentData]
        public void TryUpdateIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocumentService.Add(jsonDocument);
            var document = new JsonDocument(jsonDocument.Id, jsonDocument.Name, jsonDocument.Group, "{ \"string\": \"Howdy World\" }")
            {
                Updated = DateTime.Now
            };

            Assert.True(jsonDocumentService.TryUpdate(document));
            var doc = jsonDocumentService.Get(document.Id);
            var o = (JObject) JsonConvert.DeserializeObject<object>(doc.Data);
            Assert.Equal("Howdy World", (string) o.SelectToken("string"));
            Assert.Equal(document.Name, doc.Name);
            Assert.NotNull(doc.Added);
            Assert.NotNull(doc.Updated);
            Assert.Equal(doc.Updated.Value, document.Updated);
        }

        [Theory, AutoJsonDocumentData]
        public void EventsAreRaisedOnUpdate(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            var raisedEvents = new List<string>();
            jsonDocumentService.Updating += (_, _) => { raisedEvents.Add("Updating"); };
            jsonDocumentService.Updated += (_, _) => { raisedEvents.Add("Updated"); };
            jsonDocumentService.Add(jsonDocument);

            var document = new JsonDocument(jsonDocument.Id, jsonDocument.Name, jsonDocument.Group, "{ \"string\": \"Howdy World\" }");
            jsonDocumentService.Update(document);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Fact(Skip = "Skip until query on metadata is supported in fake log repository")]
        public void GetLogEntriesIsOk()
        {
            var logRepository = new FakeNotificationRepository();
            var jsonDocumentService = new JsonDocumentService(new FakeJsonDocumentRepository(), logRepository);
            var jsonDocument = new JsonDocument("myDocument", "My Document", "{ \"string\": \"Hello World\" }");
            jsonDocumentService.Add(jsonDocument);
            var logEntries = jsonDocumentService.GetNotificationEntries(jsonDocument.Id).ToArray();

            Assert.Single(logEntries);
            Assert.Equal("JSON document 'myDocument' was added.", logEntries[0].Text);
        }

        [Theory, AutoData]
        public void AddLogEntryIsOk(FakeJsonDocument jsonDocument)
        {
            var logRepository = new FakeNotificationRepository();
            var jsonDocumentService = new JsonDocumentService(new FakeJsonDocumentRepository(), logRepository);
            jsonDocumentService.AddNotificationEntry(jsonDocument.Id, new NotificationEntry(NotificationLevel.Information, "I did something", "Me"));
            var logService = new NotificationService(logRepository);

            var entry = logService.Last(new Query<JsonDocument>(new QueryCondition("NotificationLevel", NotificationLevel.Information)));
            Assert.Equal(jsonDocument.Id, entry.Metadata["jsonDocumentId"]);
        }

        [Theory, AutoJsonDocumentData]
        public void TrySoftRemoveIsOk(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocument.Deleted = null;
            jsonDocumentService.Add(jsonDocument);

            Assert.True(jsonDocumentService.TrySoftRemove(jsonDocument.Id));
            var removedDocument = jsonDocumentService.Get(jsonDocument.Id);
            Assert.NotNull(removedDocument.Deleted);
            Assert.InRange(removedDocument.Deleted.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoJsonDocumentData]
        public void EventsAreRaisedOnTrySoftRemove(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            var raisedEvents = new List<string>();
            jsonDocumentService.Deleting += (_, _) => { raisedEvents.Add("Deleting"); };
            jsonDocumentService.Deleted += (_, _) => { raisedEvents.Add("Deleted"); };
            jsonDocument.Deleted = null;
            jsonDocumentService.Add(jsonDocument);

            jsonDocumentService.TrySoftRemove(jsonDocument.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoJsonDocumentData]
        public void TrySoftRemoveIsIdempotent(JsonDocumentService jsonDocumentService, FakeJsonDocument jsonDocument)
        {
            jsonDocument.Deleted = null;
            jsonDocumentService.Add(jsonDocument);

            Assert.True(jsonDocumentService.TrySoftRemove(jsonDocument.Id));
            var removedDocument = jsonDocumentService.Get(jsonDocument.Id);
            Assert.NotNull(removedDocument.Deleted);
            var deletedTime = removedDocument.Deleted.Value;
            Assert.InRange(deletedTime, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

            Assert.True(jsonDocumentService.TrySoftRemove(jsonDocument.Id));
            Assert.Equal(deletedTime, jsonDocumentService.Get(jsonDocument.Id).Deleted);
        }

        [Theory, AutoJsonDocumentData]
        public void TrySoftRemoveNonExistingReturnsFalse(JsonDocumentService jsonDocumentService, string jsonDocumentId)
        {
            Assert.False(jsonDocumentService.TrySoftRemove(jsonDocumentId));
        }
    }
}
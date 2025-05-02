namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mails;
    using Xunit;

    public class MailTemplateServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new MailTemplateService(null));
        }

        [Theory, AutoMailTemplateData]
        public void GetNonExistingThrows(MailTemplateService mailTemplateService)
        {
            Assert.False(mailTemplateService.TryGet("UnknownTemplate", out _));
        }

        [Theory, AutoMailTemplateData]
        public void UpdateNonExistingThrows(MailTemplateService mailTemplateService, MailTemplate template)
        {
            Assert.Throws<KeyNotFoundException>(() => mailTemplateService.Update(template));
        }

        [Theory, AutoMailTemplateData]
        public void RemoveNonExistingThrows(MailTemplateService mailTemplateService, MailTemplate template)
        {
            Assert.Throws<KeyNotFoundException>(() => mailTemplateService.Remove(template.Id));
        }

        [Theory, AutoMailTemplateData(RepeatCount)]
        public void GetAllIsOk(MailTemplateService mailTemplateService)
        {
            Assert.Equal(RepeatCount, mailTemplateService.GetAll().Count());
        }

        [Theory, AutoMailTemplateData(RepeatCount)]
        public void GetIdsIsOk(MailTemplateService mailTemplateService)
        {
            Assert.Equal(RepeatCount, mailTemplateService.GetIds().Count());
        }

        [Theory, AutoMailTemplateData]
        public void AddAndGetIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            mailTemplateService.Add(template);
            mailTemplateService.TryGet(template.Id, out var myEntity);
            Assert.Equal(template.Id, myEntity.Id);
        }

        [Theory, AutoMailTemplateData(RepeatCount)]
        public void CountIsOk(MailTemplateService mailTemplateService)
        {
            Assert.Equal(RepeatCount, mailTemplateService.Count());
        }

        [Theory, AutoMailTemplateData(RepeatCount)]
        public void ExistsIsOk(MailTemplateService mailTemplateService)
        {
            var template = mailTemplateService.GetAll().ToArray()[0];
            Assert.True(mailTemplateService.Exists(template.Id));
        }

        [Theory, AutoMailTemplateData(RepeatCount)]
        public void DoesNotExistIsOk(MailTemplateService mailTemplateService)
        {
            Assert.False(mailTemplateService.Exists("NonExistingHost"));
        }

        [Theory, AutoMailTemplateData]
        public void EventsAreRaisedOnAdd(MailTemplateService mailTemplateService, MailTemplate template)
        {
            var raisedEvents = new List<string>();
            mailTemplateService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            mailTemplateService.Added += (s, e) => { raisedEvents.Add("Added"); };

            mailTemplateService.Add(template);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoMailTemplateData]
        public void RemoveIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            mailTemplateService.Add(template);
            mailTemplateService.Remove(template.Id);

            Assert.False(mailTemplateService.Exists(template.Id));
            Assert.Equal(0, mailTemplateService.Count());
        }

        [Theory, AutoMailTemplateData]
        public void EventsAreRaisedOnRemove(MailTemplateService mailTemplateService, MailTemplate template)
        {
            var raisedEvents = new List<string>();
            mailTemplateService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            mailTemplateService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            mailTemplateService.Add(template);

            mailTemplateService.Remove(template.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoMailTemplateData]
        public void UpdateIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            mailTemplateService.Add(template);
            var updatedTemplate = new MailTemplate(template.Id, "Updated name");
            mailTemplateService.Update(updatedTemplate);

            mailTemplateService.TryGet(template.Id, out var myEntity);
            Assert.Equal(updatedTemplate.Name, myEntity.Name);
        }

        [Theory, AutoMailTemplateData]
        public void AddOrUpdateIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            var raisedEvents = new List<string>();
            mailTemplateService.Added += (s, e) => { raisedEvents.Add("Added"); };
            mailTemplateService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            mailTemplateService.AddOrUpdate(template);
            var updated = new MailTemplate(template.Id, "Updated name");
            mailTemplateService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            mailTemplateService.TryGet(template.Id, out var myEntity);
            Assert.Equal(updated.Name, myEntity.Name);
        }

        [Theory, AutoMailTemplateData]
        public void TryAddIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            Assert.True(mailTemplateService.TryAdd(template));
            mailTemplateService.TryGet(template.Id, out var myEntity);
            Assert.Equal(template.Id, myEntity.Id);
        }

        [Theory, AutoMailTemplateData]
        public void TryAddExistingReturnsFalse(MailTemplateService mailTemplateService, MailTemplate template)
        {
            mailTemplateService.Add(template);
            Assert.False(mailTemplateService.TryAdd(template));
        }

        [Theory, AutoMailTemplateData]
        public void TryUpdateIsOk(MailTemplateService mailTemplateService, MailTemplate template)
        {
            mailTemplateService.Add(template);
            var updatedTemplate = new MailTemplate(template.Id, "Updated name");

            Assert.True(mailTemplateService.TryUpdate(updatedTemplate));
            mailTemplateService.TryGet(template.Id, out var myEntity);
            Assert.Equal(updatedTemplate.Name, myEntity.Name);
        }

        [Theory, AutoMailTemplateData]
        public void TryUpdateNonExistingReturnsFalse(MailTemplateService mailTemplateService, MailTemplate template)
        {
            Assert.False(mailTemplateService.TryUpdate(template));
        }

        [Theory, AutoMailTemplateData]
        public void EventsAreRaisedOnUpdate(MailTemplateService mailTemplateService, MailTemplate template)
        {
            var raisedEvents = new List<string>();
            mailTemplateService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            mailTemplateService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            mailTemplateService.Add(template);

            var updatedTemplate = new MailTemplate(template.Id, "Updated name");
            mailTemplateService.Update(updatedTemplate);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = MailTemplateService.GetRepositoryTypes();

            Assert.Contains(typeof(MailTemplateRepository), repositoryTypes);
        }
    }
}
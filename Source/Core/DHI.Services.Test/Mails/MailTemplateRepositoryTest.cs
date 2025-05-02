namespace DHI.Services.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Mails;
    using Xunit;

    public class MailTeplateRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"__mailtemplates-{Guid.NewGuid()}.json");
        private readonly MailTemplateRepository _repository;

        public MailTeplateRepositoryTest()
        {
            _repository = new MailTemplateRepository(_filePath);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new MailTemplateRepository(null));
        }

        [Theory, AutoData]
        public void AddExistingThrows(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            Assert.Throws<ArgumentException>(() => _repository.Add(mailTemplate));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            var actual = _repository.Get(mailTemplate.Id).Value;
            Assert.Equal(mailTemplate.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            Assert.True(_repository.Contains(mailTemplate.Id));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_repository.Contains("NonExistingConnection"));
        }

        [Theory, AutoData]
        public void CountIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            Assert.Single(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetIdsIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            Assert.Equal(mailTemplate.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            _repository.Remove(mailTemplate.Id);

            Assert.False(_repository.Contains(mailTemplate.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void RemoveUsingPredicateIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            _repository.Remove(e => e.Id == mailTemplate.Id);
            Assert.False(_repository.Contains(mailTemplate.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void UpdateIsOk(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            var updatedMailTemplate = new MailTemplate(mailTemplate.Id, "Updated name");
            _repository.Update(updatedMailTemplate);

            Assert.Equal("Updated name", _repository.Get(mailTemplate.Id).Value.Name);
        }

        [Fact]
        public void CaseInsensitiveComparerIsOk()
        {
            var repository = new JsonRepository<FakeEntity, string>(_filePath, comparer: StringComparer.InvariantCultureIgnoreCase);
            repository.Add(new FakeEntity("MyEntity", "My Entity"));
            Assert.True(repository.Contains("myentity"));
        }

        [Theory, AutoData]
        public void GetAllReturnsClones(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(mailTemplate.Id).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {
            Assert.Empty(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetReturnsClone(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            var e = _repository.Get(mailTemplate.Id).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(mailTemplate.Id).Value.Metadata);
        }

        [Theory, AutoData]
        public void GetByPreidcateReturnsClones(MailTemplate mailTemplate)
        {
            _repository.Add(mailTemplate);
            var e = _repository.Get(ent => ent.Id == mailTemplate.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(mailTemplate.Id).Value.Metadata);
        }
    }
}
namespace DHI.Services.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;

    public class ConnectionRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"connections-{DateTime.Now.Ticks}.json");
        private readonly ConnectionRepository _repository;

        public ConnectionRepositoryTest()
        {
            _repository = new ConnectionRepository(_filePath);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ConnectionRepository(null));
        }

        [Theory, AutoData]
        public void AddExistingThrows(FakeConnection connection)
        {
            _repository.Add(connection);
            Assert.Throws<ArgumentException>(() => _repository.Add(connection));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            var actual = _repository.Get(connection.Id).Value;
            Assert.Equal(connection.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            Assert.True(_repository.Contains(connection.Id));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_repository.Contains("NonExistingConnection"));
        }

        [Theory, AutoData]
        public void CountIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            Assert.Single(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetIdsIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            Assert.Equal(connection.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            _repository.Remove(connection.Id);

            Assert.False(_repository.Contains(connection.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void RemoveUsingPredicateIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            _repository.Remove(e => e.Id == connection.Id);
            Assert.False(_repository.Contains(connection.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void UpdateIsOk(FakeConnection connection)
        {
            _repository.Add(connection);
            var updatedConnection = new FakeConnection(connection.Id, "Updated name");
            _repository.Update(updatedConnection);

            Assert.Equal("Updated name", _repository.Get(connection.Id).Value.Name);
        }

        [Fact]
        public void CaseInsensitiveComparerIsOk()
        {
            var repository = new JsonRepository<FakeEntity, string>(_filePath, comparer: StringComparer.InvariantCultureIgnoreCase);
            repository.Add(new FakeEntity("MyEntity", "My Entity"));
            Assert.True(repository.Contains("myentity"));
        }

        [Theory, AutoData]
        public void GetAllReturnsClones(FakeConnection connection)
        {
            _repository.Add(connection);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(connection.Id).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {
            Assert.Empty(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetReturnsClone(FakeConnection connection)
        {
            _repository.Add(connection);
            var e = _repository.Get(connection.Id).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(connection.Id).Value.Metadata);
        }

        [Theory, AutoData]
        public void GetByPreidcateReturnsClones(FakeConnection connection)
        {
            _repository.Add(connection);
            var e = _repository.Get(ent => ent.Id == connection.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(connection.Id).Value.Metadata);
        }
    }
}
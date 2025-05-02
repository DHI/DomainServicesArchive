namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Jobs;
    using Xunit;

    public class GroupedHostRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly string _emptyHostfilePath;
        private readonly GroupedHostRepository _repository;

        public GroupedHostRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "grouped-hosts2.json");
            File.Copy(@"../../../Data/grouped-hosts2.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _emptyHostfilePath = Path.Combine(Path.GetTempPath(), "grouped-hosts.json");
            File.Copy(@"../../../Data/grouped-hosts.json", _emptyHostfilePath, true);
            new FileInfo(_emptyHostfilePath).IsReadOnly = false;
            _repository = new GroupedHostRepository(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedHostRepository(null));
        }

        [Fact]
        public void CreateWithNonExistingFilePathThrows()
        {
            Assert.Throws<FileNotFoundException>(() => new GroupedHostRepository("C:\\NonExistingFile.json"));
        }

        [Fact]
        public void AddWithNoGroupThrows()
        {
            var host = new Host("194.123.123.123", "MyHost", null);
            Assert.Throws<ArgumentException>(() => _repository.Add(host));
        }

        [Fact(Skip = "Contains() method not found just returning false")]
        public void ContainsWithInvalidGroupedIdThrows()
        {
            Assert.Throws<ArgumentException>(() => _repository.Contains("InvalidGroupedHostId"));
        }

        [Fact]
        public void GetWithInvalidGroupedIdThrows()
        {
            Assert.Throws<ArgumentException>(() => _repository.Get("InvalidGroupedHostId"));
        }

        [Fact]
        public void RemoveWithInvalidGroupedIdThrows()
        {
            Assert.Throws<ArgumentException>(() => _repository.Remove("InvalidGroupedHostId"));
        }

        [Fact]
        public void GetNonExistingReturnsEmpty()
        {
            Assert.False(_repository.Get("NonExistingGroup/NonExistingName").HasValue);
        }

        [Theory, AutoHostData]
        public void GetNonExistingFromExistingGroupReturnsEmpty(Host host)
        {
            _repository.Add(host);
            var id = $"{host.Group}/NonExistingName";
            Assert.False(_repository.Get(id).HasValue);
        }

        [Theory, AutoHostData]
        public void AddAndGetIsOk(Host host)
        {
            _repository.Add(host);
            var actual = _repository.Get(host.FullName).Value;
            Assert.Equal(host.Id, actual.Id);
        }

        [Theory, AutoHostData]
        public void ContainsIsOk(Host host)
        {
            _repository.Add(host);
            Assert.True(_repository.Contains(host.FullName));
        }

        [Theory, AutoHostData]
        public void DoesNotContainIsOk(Host host)
        {
            Assert.False(_repository.Contains(host.FullName));
        }

        [Theory, AutoHostData]
        public void ContainsGroupIsOk(Host host)
        {
            _repository.Add(host);
            Assert.True(_repository.ContainsGroup(host.Group));
        }

        [Theory, AutoHostData]
        public void DoesNotContainGroupIsOk(Host host)
        {
            Assert.False(_repository.ContainsGroup(host.Group));
        }

        [Theory, AutoHostData]
        public void CountIsOk(Host host)
        {
            _repository.Add(host);
            Assert.Equal(4, _repository.Count());
        }

        [Theory, AutoHostData]
        public void GetAllIsOk(Host host)
        {
            _repository.Add(host);
            var hosts = _repository.GetAll();
            Assert.True(hosts.Any());
        }


        [Theory, AutoHostData]
        public void GetByGroupIsOk(Host host1, Host host2)
        {
            _repository.Add(host1);
            _repository.Add(host2);
            var host3 = new Host("194.123.123.123", "MyHost", host1.Group);
            _repository.Add(host3);
            Assert.Equal(2, _repository.GetByGroup(host1.Group).Count());
            Assert.Single(_repository.GetByGroup(host2.Group));
        }

        [Theory, AutoHostData]
        public void GetFullNamesByGroupIsOk(Host host1, Host host2)
        {
            _repository.Add(host1);
            _repository.Add(host2);
            var host3 = new Host("194.123.123.123", "MyHost", host1.Group);
            _repository.Add(host3);
            Assert.Equal(2, _repository.GetFullNames(host1.Group).Count());
            Assert.Single(_repository.GetFullNames(host2.Group));
            Assert.Equal(host2.FullName, _repository.GetFullNames(host2.Group).First());
        }

        [Theory, AutoHostData]
        public void GetFullNamesIsOk(Host host1, Host host2)
        {
            _repository.Add(host1);
            _repository.Add(host2);
            var host3 = new Host("194.123.123.123", "MyHost", host1.Group);
            _repository.Add(host3);
            Assert.Equal(6, _repository.GetFullNames().Count());
        }

        [Theory, AutoHostData]
        public void GetIdsIsOk(Host host)
        {
            _repository.Add(host);
            Assert.Contains(host.Id, _repository.GetIds());
        }

        [Theory, AutoHostData]
        public void RemoveIsOk(Host host)
        {
            _repository.Add(host);
            _repository.Remove(host.FullName);
            Assert.False(_repository.Contains(host.FullName));
            Assert.Equal(3, _repository.Count());
        }

        [Theory, AutoHostData]
        public void RemoveUsingPredicateIsOk(Host host1, Host host2)
        {
            _repository.Add(host1);
            _repository.Add(host2);
            _repository.Remove(e => e.Id == host1.Id);
            Assert.False(_repository.Contains(host1.FullName));
            Assert.Equal(4, _repository.Count());
        }

        [Theory, AutoHostData]
        public void UpdateIsOk(Host host)
        {
            _repository.Add(host);
            host.Priority = 1;
            _repository.Update(host);
            Assert.Equal(1, _repository.Get(host.FullName).Value.Priority);
        }

        [Theory, AutoHostData]
        public void GetAllReturnsClones(Host host)
        {
            _repository.Add(host);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(host.FullName).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {

            var repository = new GroupedHostRepository(_emptyHostfilePath);
            Assert.Empty(repository.GetAll());
        }

        [Theory, AutoHostData]
        public void GetReturnsClone(Host host)
        {
            _repository.Add(host);
            var e = _repository.Get(host.FullName).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(host.FullName).Value.Metadata);
        }

        [Theory, AutoHostData]
        public void GetByPredicateReturnsClones(Host host)
        {
            _repository.Add(host);
            var e = _repository.Get(ent => ent.Id == host.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(host.FullName).Value.Metadata);
        }

        [Theory, AutoHostData]
        public void GetByGroupReturnsClones(Host host)
        {
            _repository.Add(host);
            var e = _repository.GetByGroup(host.Group).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(host.FullName).Value.Metadata);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
            File.Delete(_emptyHostfilePath);
        }
    }
}
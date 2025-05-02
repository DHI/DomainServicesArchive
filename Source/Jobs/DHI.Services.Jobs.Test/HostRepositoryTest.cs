namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Jobs;
    using AutoFixture.Xunit2;
    using Xunit;

    public class HostRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly HostRepository _repository;

        public HostRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "hosts.json");
            File.Copy(@"../../../Data/hosts.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _repository = new HostRepository(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new HostRepository(null));
        }

        [Fact]
        public void CreateWithNonExistingFilePathThrows()
        {
            Assert.Throws<FileNotFoundException>(() => new HostRepository("C:\\NonExistingFile.json"));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(Host host)
        {
            _repository.Add(host);
            var actual = _repository.Get(host.Id).Value;
            Assert.Equal(host.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(Host host)
        {
            _repository.Add(host);
            Assert.True(_repository.Contains(host.Id));
        }

        [Theory, AutoData]
        public void DoesNotContainIsOk(string id)
        {
            Assert.False(_repository.Contains(id));
        }

        [Theory, AutoData]
        public void CountIsOk(Host host)
        {
            _repository.Add(host);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(Host host)
        {
            _repository.Add(host);
            Assert.Single(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetIdsIsOk(Host host)
        {
            _repository.Add(host);
            Assert.Equal(host.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(Host host)
        {
            _repository.Add(host);
            _repository.Remove(host.Id);
            Assert.False(_repository.Contains(host.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void UpdateIsOk(Host host)
        {
            _repository.Add(host);
            host.Priority = 1;
            _repository.Update(host);
            Assert.Equal(1, _repository.Get(host.Id).Value.Priority);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}
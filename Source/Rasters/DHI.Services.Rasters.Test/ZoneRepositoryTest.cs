namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Xunit;
    using Zones;

    public class ZoneRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"__zones{DateTime.Now.ToString("sss")}.json");
        private readonly ZoneRepository _repository;

        public ZoneRepositoryTest()
        {        
            _repository = new ZoneRepository(_filePath, new List<JsonConverter>() {
                new ZoneConverter()
            });
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneRepository(null));
        }

        [Fact]
        public void AddAndGetIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            var actual = _repository.Get(zone.Id).Value;
            Assert.Equal(zone.Id, actual.Id);
        }

        [Fact]
        public void ContainsIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            Assert.True(_repository.Contains(zone.Id));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_repository.Contains("NonExistingAccount"));
        }

        [Fact]
        public void CountIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            Assert.Equal(1, _repository.Count());
        }

        [Fact]
        public void GetAllIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            Assert.Single(_repository.GetAll());
        }

        [Fact]
        public void GetIdsIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            Assert.Equal(zone.Id, _repository.GetIds().First());
        }

        [Fact]
        public void RemoveIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            _repository.Remove(zone.Id);
            Assert.False(_repository.Contains(zone.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Fact]
        public void UpdateIsOk()
        {
            var zone = new Zone("MyZone", "My Zone");
            _repository.Add(zone);
            var updatedAccount = new Zone(zone.Id, "Updated name");
            _repository.Update(updatedAccount);
            Assert.Equal("Updated name", _repository.Get(zone.Id).Value.Name);
        }
    }
}
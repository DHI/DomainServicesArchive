namespace DHI.Services.Physics.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;
    using Unit = Unit;

    public class UnitRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__units.json");
        private readonly UnitRepository _repository;

        public UnitRepositoryTest()
        {
            _repository = new UnitRepository(_filePath);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitRepository(null));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(Unit unit)
        {
            _repository.Add(unit);
            var actual = _repository.Get(unit.Id).Value;
            Assert.Equal(unit.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(Unit unit)
        {
            _repository.Add(unit);
            Assert.True(_repository.Contains(unit.Id));
        }

        [Theory, AutoData]
        public void DoesNotContainIsOk(string id)
        {
            Assert.False(_repository.Contains(id));
        }

        [Theory, AutoData]
        public void CountIsOk(Unit unit)
        {
            _repository.Add(unit);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(Unit unit)
        {
            _repository.Add(unit);
            Assert.NotEmpty(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetIdsIsOk(Unit unit)
        {
            _repository.Add(unit);
            Assert.Equal(unit.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(Unit unit)
        {
            _repository.Add(unit);
            _repository.Remove(unit.Id);
            Assert.False(_repository.Contains(unit.Id));
            Assert.Equal(0, _repository.Count());
        }

        //[Theory, AutoData]
        //public void UpdateIsOk(Unit unit)
        //{
        //    _repository.Add(unit);
        //    unit.Description = "New description";
        //    _repository.Update(unit);
        //    Assert.Equal("New description", _repository.Get(unit.Id).Value.Description);
        //}
    }
}
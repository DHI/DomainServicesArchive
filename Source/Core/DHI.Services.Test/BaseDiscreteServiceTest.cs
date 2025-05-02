namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;

    public class BaseDiscreteServiceTest : IClassFixture<RepositoryFixture>
    {
        private readonly DiscreteService _service;
        private readonly int _repeatCount;

        public BaseDiscreteServiceTest(RepositoryFixture fixture)
        {
            var repository = fixture.Repository;
            _service = new DiscreteService(repository);
            _repeatCount = fixture.RepeatCount;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new DiscreteService(null));
        }

        [Theory, AutoData]
        public void GetIsOk(FakeEntity entity)
        {
            var repository = new FakeRepository<FakeEntity, string>();
            repository.Add(entity);
            var service = new DiscreteService(repository);
            service.TryGet(entity.Id, out var myEntity);
            Assert.Equal(entity.Id, myEntity.Id);
        }

        [Theory, AutoData]
        public void GetManyIsOk(FakeEntity[] entities)
        {
            var repository = new FakeRepository<FakeEntity, string>();
            foreach (var entity in entities)
            {
                repository.Add(entity);
            }

            var service = new DiscreteService(repository);
            var myEntities = service.Get(entities.Select(e => e.Id)).ToArray();
            Assert.Equal(_repeatCount, myEntities.Length);
            Assert.Contains(entities[0].Id, myEntities.Select(e => e.Id));
        }

        [Fact]
        public void GetAllIsOk()
        {
            Assert.True(_service.GetAll().Any());
        }

        [Fact]
        public void GetIdsIsOk()
        {
            Assert.True(_service.GetIds().Any());
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.True(_service.Count() > 0);
        }

        [Fact]
        public void ExistsIsOk()
        {
            var entity = _service.GetAll().ToArray()[0];
            Assert.True(_service.Exists(entity.Id));
        }

        [Fact]
        public void DoesNotExistsIsOk()
        {
            Assert.False(_service.Exists("NonExistingEntity"));
        }

        private class DiscreteService : BaseDiscreteService<FakeEntity, string>
        {
            public DiscreteService(IDiscreteRepository<FakeEntity, string> repository)
                : base(repository)
            {
            }
        }
    }
}
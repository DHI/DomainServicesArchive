namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;

    public class BaseServiceTest : IClassFixture<RepositoryFixture>
    {
        private readonly Service _service;
        private readonly int _repeatCount;

        public BaseServiceTest(RepositoryFixture fixture)
        {
            var repository = fixture.Repository;
            _service = new Service(repository);
            _repeatCount = fixture.RepeatCount;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Service(null));
        }

        [Fact]
        public void GetNonExistingReturnsNullIsOk()
        {
            Assert.False(_service.TryGet("UnknownEntity", out _));
        }

        [Theory, AutoData]
        public void GetIsOk(FakeEntity entity)
        {
            var repository = new FakeRepository<FakeEntity, string>();
            repository.Add(entity);
            var service = new Service(repository);
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

            var service = new Service(repository);
            var myEntities = service.Get(entities.Select(e => e.Id)).ToArray();
            Assert.Equal(_repeatCount, myEntities.Length);
            Assert.Contains(entities[0].Id, myEntities.Select(e => e.Id));
        }

        private class Service : BaseService<FakeEntity, string>
        {
            public Service(IRepository<FakeEntity, string> repository)
                : base(repository)
            {
            }
        }
    }
}
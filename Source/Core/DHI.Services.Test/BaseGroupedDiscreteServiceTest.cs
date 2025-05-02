namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using AutoFixture.Xunit2;
    using Xunit;

    public class BaseGroupedDiscreteServiceTest : IClassFixture<RepositoryFixture>
    {
        private readonly GroupedDiscreteService _service;
        private readonly int _repeatCount;

        public BaseGroupedDiscreteServiceTest(RepositoryFixture fixture)
        {
            var repository = fixture.Repository;
            _service = new GroupedDiscreteService(repository);
            _repeatCount = fixture.RepeatCount;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedDiscreteService(null));
        }

        [Fact]
        public void GetByGroupForNonExistingThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetByGroup("NonExistingGroup"));
        }

        [Fact]
        public void GetByGroupForNullGroupThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _service.GetByGroup(null));
        }

        [Fact]
        public void GetByGroupForEmptyGroupThrows()
        {
            Assert.Throws<ArgumentException>(() => _service.GetByGroup(""));
        }

        [Fact]
        public void GetFullNamesForNonExistingGroupThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetFullNames("NonExistingGroup"));
        }

        [Fact]
        public void GetFullNamesForNullOrEmptyGroupThrows()
        {
            Assert.Throws<ArgumentException>(() => _service.GetFullNames(""));
            Assert.Throws<ArgumentNullException>(() => _service.GetFullNames(null, ClaimsPrincipal.Current));
        }

        [Theory, AutoData]
        public void GetIsOk(FakeEntity entity)
        {
            var repository = new FakeGroupedRepository<FakeEntity, string>();
            repository.Add(entity);
            var service = new GroupedDiscreteService(repository);
            service.TryGet(entity.Id, out var myEntity);
            Assert.Equal(entity.Id, myEntity.Id);
        }

        [Theory, AutoData]
        public void GetManyIsOk(FakeEntity[] entities)
        {
            var repository = new FakeGroupedRepository<FakeEntity, string>();
            foreach (var entity in entities)
            {
                repository.Add(entity);
            }

            var service = new GroupedDiscreteService(repository);
            var myEntities = service.Get(entities.Select(e => e.Id)).ToArray();
            Assert.Equal(_repeatCount, myEntities.Length);
            Assert.Contains(entities[0].Id, myEntities.Select(e => e.Id));
        }

        [Fact]
        public void NonExistingGroupReturnsFalse()
        {
            Assert.False(_service.GroupExists("nonExistingGroup"));
        }

        [Fact]
        public void GroupExistsIsOk()
        {
            var group = _service.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(_service.GroupExists(group));
        }

        [Fact]
        public void GetByGroupIsOk()
        {
            var group = _service.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(_service.GetByGroup(group).Any());
        }

        [Fact]
        public void GetByGroupsIsOk()
        {
            var group = _service.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(_service.GetByGroups(new List<string> { group, group }).Any());
        }

        [Fact]
        public void GetFullNamesByGroupIsOk()
        {
            var group = _service.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = _service.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Fact]
        public void GetFullNamesIsOk()
        {
            Assert.True(_service.GetFullNames().Any());
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

        private class GroupedDiscreteService : BaseGroupedDiscreteService<FakeEntity, string>
        {
            public GroupedDiscreteService(IGroupedRepository<FakeEntity> repository)
                : base(repository)
            {
            }
        }
    }
}
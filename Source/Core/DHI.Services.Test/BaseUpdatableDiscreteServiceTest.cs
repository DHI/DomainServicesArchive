namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;

    public class BaseUpdatableDiscreteServiceTest : IClassFixture<RepositoryFixture>
    {
        private readonly UpdatableDiscreteService _service;
        private readonly int _repeatCount;

        public BaseUpdatableDiscreteServiceTest(RepositoryFixture fixture)
        {
            var repository = fixture.Repository;
            _service = new UpdatableDiscreteService(repository);
            _repeatCount = fixture.RepeatCount;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatableDiscreteService(null));
        }

        [Theory, AutoFakeEntityData]
        public void UpdateNonExistingThrows(UpdatableDiscreteService service, FakeEntity entity)
        {
            Assert.Throws<KeyNotFoundException>(() => service.Update(entity));
        }

        [Theory, AutoFakeEntityData]
        public void RemoveNonExistingThrows(UpdatableDiscreteService service, FakeEntity entity)
        {
            Assert.Throws<KeyNotFoundException>(() => service.Remove(entity.Id));
        }

        [Theory, AutoData]
        public void GetIsOk(FakeEntity entity)
        {
            var repository = new FakeRepository<FakeEntity, string>();
            repository.Add(entity);
            var service = new UpdatableDiscreteService(repository);
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

            var service = new UpdatableDiscreteService(repository);
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

        [Theory, AutoFakeEntityData]
        public void AddAndGetIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            service.Add(entity);
            service.TryGet(entity.Id, out var e);
            Assert.Equal(entity.Id, e.Id);
            Assert.Null(e.Updated);
            Assert.NotNull(e.Added);
            Assert.InRange(e.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void UpdateIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            service.Add(entity);
            var updatedEntity = new FakeEntity(entity.Id, "Updated name");
            service.Update(updatedEntity);
            service.TryGet(entity.Id, out var e);

            Assert.Equal(updatedEntity.Name, e.Name);
            Assert.NotNull(e.Updated);
            Assert.InRange(e.Updated.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void AddOrUpdateIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Added += (s, _) => { raisedEvents.Add("Added"); };
            service.Updated += (s, _) => { raisedEvents.Add("Updated"); };
            service.AddOrUpdate(entity);
            var updated = new FakeEntity(entity.Id, "Updated name");
            service.AddOrUpdate(updated);
            service.TryGet(entity.Id, out var e);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Name, e.Name);
            Assert.NotNull(e.Updated);
            Assert.InRange(e.Updated.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void TryAddIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            Assert.True(service.TryAdd(entity));
            service.TryGet(entity.Id, out var e);
            Assert.Equal(entity.Id, e.Id);
            Assert.Null(e.Updated);
            Assert.NotNull(e.Added);
            Assert.InRange(e.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void TryAddExistingReturnsFalse(UpdatableDiscreteService service, FakeEntity entity)
        {
            service.Add(entity);
            Assert.False(service.TryAdd(entity));
        }

        [Theory, AutoFakeEntityData]
        public void TryUpdateIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            service.Add(entity);
            var updatedEntity = new FakeEntity(entity.Id, "Updated name");

            Assert.True(service.TryUpdate(updatedEntity));
            service.TryGet(entity.Id, out var e);
            Assert.Equal(updatedEntity.Name, e.Name);
            Assert.NotNull(e.Added);
            Assert.NotNull(e.Updated);
            Assert.InRange(e.Updated.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void TryUpdateNonExistingReturnsFalse(UpdatableDiscreteService service, FakeEntity entity)
        {
            Assert.False(service.TryUpdate(entity));
        }

        [Theory, AutoFakeEntityData]
        public void RemoveIsOk(UpdatableDiscreteService service, FakeEntity entity)
        {
            service.Add(entity);
            service.Remove(entity.Id);

            Assert.False(service.TryGet(entity.Id, out _));
        }

        [Theory, AutoFakeEntityData]
        public void EventsAreRaisedOnAdd(UpdatableDiscreteService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            service.Added += (s, e) => { raisedEvents.Add("Added"); };

            service.Add(entity);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoFakeEntityData]
        public void EventsAreRaisedOnUpdate(UpdatableDiscreteService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            service.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            service.Add(entity);

            var updatedAccount = new FakeEntity(entity.Id, "Updated name");
            service.Update(updatedAccount);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoFakeEntityData]
        public void EventsAreRaisedOnRemove(UpdatableDiscreteService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            service.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            service.Add(entity);

            service.Remove(entity.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        public class UpdatableDiscreteService : BaseUpdatableDiscreteService<FakeEntity, string>
        {
            public UpdatableDiscreteService(IUpdatableRepository<FakeEntity, string> repository)
                : base(repository)
            {
            }
        }
    }
}
namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class BaseUpdatableServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatableService(null));
        }

        [Theory, AutoFakeEntityData]
        public void UpdateNonExistingThrows(UpdatableService service, FakeEntity entity)
        {
            Assert.Throws<KeyNotFoundException>(() => service.Update(entity));
        }

        [Theory, AutoFakeEntityData]
        public void RemoveNonExistingThrows(UpdatableService service, FakeEntity entity)
        {
            Assert.Throws<KeyNotFoundException>(() => service.Remove(entity.Id));
        }

        [Theory, AutoFakeEntityData]
        public void AddAndGetIsOk(UpdatableService service, FakeEntity entity)
        {
            service.Add(entity);
            service.TryGet(entity.Id, out var e);
            Assert.Equal(entity.Id, e.Id);
            Assert.Null(e.Updated);
            Assert.NotNull(e.Added);
            Assert.InRange(e.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void GetManyIsOk(UpdatableService service, FakeEntity[] entities)
        {
            foreach (var entity in entities)
            {
                service.Add(entity);
            }

            var myEntities = service.Get(entities.Select(e => e.Id)).ToArray();
            Assert.NotEmpty(myEntities);
            Assert.Contains(entities[0].Id, myEntities.Select(e => e.Id));
        }

        [Theory, AutoFakeEntityData]
        public void UpdateIsOk(UpdatableService service, FakeEntity entity)
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
        public void AddOrUpdateIsOk(UpdatableService service, FakeEntity entity)
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
        public void TryAddIsOk(UpdatableService service, FakeEntity entity)
        {
            Assert.True(service.TryAdd(entity));
            service.TryGet(entity.Id, out var e);
            Assert.Equal(entity.Id, e.Id);
            Assert.Null(e.Updated);
            Assert.NotNull(e.Added);
            Assert.InRange(e.Added.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoFakeEntityData]
        public void TryAddExistingReturnsFalse(UpdatableService service, FakeEntity entity)
        {
            service.Add(entity);
            Assert.False(service.TryAdd(entity));
        }

        [Theory, AutoFakeEntityData]
        public void TryUpdateIsOk(UpdatableService service, FakeEntity entity)
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
        public void TryUpdateNonExistingReturnsFalse(UpdatableService service, FakeEntity entity)
        {
            Assert.False(service.TryUpdate(entity));
        }

        [Theory, AutoFakeEntityData]
        public void RemoveIsOk(UpdatableService service, FakeEntity entity)
        {
            service.Add(entity);
            service.Remove(entity.Id);

            Assert.False(service.TryGet(entity.Id, out _));
        }

        [Theory, AutoFakeEntityData]
        public void EventsAreRaisedOnAdd(UpdatableService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            service.Added += (s, e) => { raisedEvents.Add("Added"); };

            service.Add(entity);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoFakeEntityData]
        public void EventsAreRaisedOnUpdate(UpdatableService service, FakeEntity entity)
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
        public void EventsAreRaisedOnRemove(UpdatableService service, FakeEntity entity)
        {
            var raisedEvents = new List<string>();
            service.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            service.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            service.Add(entity);

            service.Remove(entity.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        public class UpdatableService : BaseUpdatableService<FakeEntity, string>
        {
            public UpdatableService(IUpdatableRepository<FakeEntity, string> repository)
                : base(repository)
            {
            }
        }
    }
}
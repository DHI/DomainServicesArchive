namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;

    public class BaseImmutableDiscreteServiceTest : IClassFixture<ImmutableRepositoryFixture>
    {
        private readonly ImmutableDiscreteService _service;
        private readonly int _repeatCount;

        public BaseImmutableDiscreteServiceTest(ImmutableRepositoryFixture fixture)
        {
            var repository = fixture.Repository;
            _service = new ImmutableDiscreteService(repository);
            _repeatCount = fixture.RepeatCount;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ImmutableDiscreteService(null));
        }

        [Fact]
        public void RemoveNonExistingThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.Remove(Guid.NewGuid()));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(FakeImmutableEntity entity)
        {
            _service.Add(entity);
            _service.TryGet(entity.Id, out var myEntity);
            Assert.Equal(entity.Id, myEntity.Id);
        }

        [Theory, AutoData]
        public void GetManyIsOk(FakeImmutableEntity[] entities)
        {
            foreach (var entity in entities)
            {
                _service.Add(entity);
            }

            var myEntities = _service.Get(entities.Select(e => e.Id)).ToArray();
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
            Assert.False(_service.Exists(Guid.NewGuid()));
        }

        [Theory, AutoData]
        public void RemoveIsOk(FakeImmutableEntity entity)
        {
            _service.Add(entity);
            _service.Remove(entity.Id);

            Assert.False(_service.TryGet(entity.Id, out _));
            Assert.False(_service.Exists(entity.Id));
        }

        [Theory, AutoData]
        public void EventsAreRaisedOnAdd(FakeImmutableEntity entity)
        {
            var raisedEvents = new List<string>();
            _service.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            _service.Added += (s, e) => { raisedEvents.Add("Added"); };

            _service.Add(entity);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoData]
        public void EventsAreRaisedOnRemove(FakeImmutableEntity entity)
        {
            var raisedEvents = new List<string>();
            _service.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            _service.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            _service.Add(entity);

            _service.Remove(entity.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        private class ImmutableDiscreteService : BaseImmutableDiscreteService<FakeImmutableEntity>
        {
            public ImmutableDiscreteService(IImmutableRepository<FakeImmutableEntity> repository)
                : base(repository)
            {
            }
        }
    }
}
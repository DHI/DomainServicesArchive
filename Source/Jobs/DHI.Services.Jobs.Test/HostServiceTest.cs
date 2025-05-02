namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Jobs;
    using Xunit;

    public class HostServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new HostService(null));
        }

        [Theory, AutoHostData]
        public void GetNonExistingThrows(HostService hostService)
        {
            Assert.False(hostService.TryGet("UnknownHost", out _));
        }

        [Theory, AutoHostData]
        public void UpdateNonExistingThrows(HostService hostService, Host host)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.Update(host));
        }

        [Theory, AutoHostData]
        public void RemoveNonExistingThrows(HostService hostService, Host host)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.Remove(host.Id));
        }

        [Theory, AutoHostData]
        public void CreateHostThrowsIfNotSupported(HostService hostService)
        {
            var exception = Assert.Throws<NotSupportedException>(() => hostService.CreateHost());
            Assert.Equal("This repository cannot create hosts dynamically.", exception.Message);
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetAllIsOk(HostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.GetAll().Count());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetIdsIsOk(HostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.GetIds().Count());
        }

        [Theory, AutoHostData]
        public void AddAndGetIsOk(HostService hostService, Host host)
        {
            hostService.Add(host);
            hostService.TryGet(host.Id, out var jb);
            Assert.Equal(host.Id, jb.Id);
        }

        [Theory, AutoHostData(RepeatCount)]
        public void CountIsOk(HostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.Count());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void ExistsIsOk(HostService hostService)
        {
            var host = hostService.GetAll().ToArray()[0];
            Assert.True(hostService.Exists(host.Id));
        }

        [Theory, AutoHostData(RepeatCount)]
        public void DoesNotExistIsOk(HostService hostService)
        {
            Assert.False(hostService.Exists("NonExistingHost"));
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnAdd(HostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            hostService.Added += (s, e) => { raisedEvents.Add("Added"); };

            hostService.Add(host);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoHostData]
        public void RemoveIsOk(HostService hostService, Host host)
        {
            hostService.Add(host);
            hostService.Remove(host.Id);

            Assert.False(hostService.Exists(host.Id));
            Assert.Equal(0, hostService.Count());
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnRemove(HostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            hostService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            hostService.Add(host);

            hostService.Remove(host.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoHostData]
        public void UpdateIsOk(HostService hostService, Host host)
        {
            hostService.Add(host);
            var updatedHost = new Host(host.Id, "Updated name");
            hostService.Update(updatedHost);

            hostService.TryGet(host.Id, out var jb);
            Assert.Equal(updatedHost.Name, jb.Name);
        }

        [Theory, AutoHostData]
        public void AddOrUpdateIsOk(HostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Added += (s, e) => { raisedEvents.Add("Added"); };
            hostService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            hostService.AddOrUpdate(host);
            var updated = new Host(host.Id, "Updated name");
            hostService.AddOrUpdate(updated);

            hostService.TryGet(host.Id, out var jb);
            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Name, jb.Name);
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnUpdate(HostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            hostService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            hostService.Add(host);

            var updatedHost = new Host(host.Id, "Updated name");
            hostService.Update(updatedHost);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }
    }
}
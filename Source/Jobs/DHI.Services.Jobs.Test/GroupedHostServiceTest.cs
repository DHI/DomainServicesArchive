namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Jobs;
    using Xunit;

    public class GroupedHostServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedHostService(null));
        }

        [Theory, AutoHostData]
        public void GetNonExistingThrows(GroupedHostService hostService)
        {
            Assert.False(hostService.TryGet("Group/NonExistingHost", out _));
        }

        [Theory, AutoHostData]
        public void UpdateNonExistingThrows(GroupedHostService hostService, Host host)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.Update(host));
        }

        [Theory, AutoHostData]
        public void RemoveNonExistingThrows(GroupedHostService hostService, Host host)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.Remove(host.FullName));
        }

        [Theory, AutoHostData]
        public void GetByGroupForNonExistingThrows(GroupedHostService hostService)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoHostData]
        public void GetByGroupForNullGroupThrows(GroupedHostService hostService)
        {
            Assert.Throws<ArgumentNullException>(() => hostService.GetByGroup(null));
        }

        [Theory, AutoHostData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedHostService hostService)
        {
            Assert.Throws<KeyNotFoundException>(() => hostService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoHostData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedHostService hostService)
        {
            Assert.Throws<ArgumentNullException>(() => hostService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => hostService.GetFullNames(""));
        }

        [Theory, AutoHostData]
        public void AddExistingThrows(GroupedHostService hostService, Host host)
        {
            hostService.Add(host);
            Assert.Throws<ArgumentException>(() => hostService.Add(host));
        }

        [Theory, AutoHostData]
        public void AddWithExistingIdThrows(GroupedHostService hostService, Host host)
        {
            hostService.Add(host);
            var newHost = new Host(host.Id, "NewName", host.Group);
            Assert.Throws<ArgumentException>(() => hostService.Add(newHost));
        }

        [Theory, AutoHostData]
        public void UpdateToExistingIdThrows(GroupedHostService hostService, Host host1, Host host2)
        {
            hostService.Add(host1);
            hostService.Add(host2);
            var updatedHost1 = new Host(host2.Id, host1.Name, host1.Group);
            Assert.Throws<ArgumentException>(() => hostService.Update(updatedHost1));
        }

        [Theory, AutoHostData]
        public void AddOrUpdateToExistingIdThrows(GroupedHostService hostService, Host host1, Host host2)
        {
            hostService.Add(host1);
            hostService.Add(host2);
            var host3 = new Host(host2.Id, "Name", "Group");
            Assert.Throws<ArgumentException>(() => hostService.AddOrUpdate(host3));
            host3 = new Host(host2.Id, host1.Name, host1.Group);
            Assert.Throws<ArgumentException>(() => hostService.AddOrUpdate(host3));
        }

        [Theory, AutoHostData]
        public void CreateHostThrowsIfNotSupported(GroupedHostService hostService)
        {
            var exception = Assert.Throws<NotImplementedException>(() => hostService.CreateHost());
            Assert.Equal("This repository cannot create hosts dynamically.", exception.Message);
        }

        [Theory, AutoHostData(RepeatCount)]
        public void NonExistingGroupReturnsFalse(GroupedHostService hostService)
        {
            Assert.False(hostService.GroupExists("nonExistingGroup"));
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GroupExistsIsOk(GroupedHostService hostService)
        {
            var group = hostService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(hostService.GroupExists(group));
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetByGroupIsOk(GroupedHostService hostService)
        {
            var group = hostService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(hostService.GetByGroup(group).Any());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetFullNamesByGroupIsOk(GroupedHostService hostService)
        {
            var group = hostService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = hostService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetFullNamesIsOk(GroupedHostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.GetFullNames().Count());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetAllIsOk(GroupedHostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.GetAll().Count());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void GetIdsIsOk(GroupedHostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.GetIds().Count());
        }

        [Theory, AutoHostData]
        public void AddAndGetIsOk(GroupedHostService hostService, Host host)
        {
            hostService.Add(host);

            hostService.TryGet(host.FullName, out var jb);
            Assert.Equal(host.Id, jb.Id);
        }

        [Theory, AutoHostData(RepeatCount)]
        public void CountIsOk(GroupedHostService hostService)
        {
            Assert.Equal(RepeatCount, hostService.Count());
        }

        [Theory, AutoHostData(RepeatCount)]
        public void ExistsIsOk(GroupedHostService hostService)
        {
            var host = hostService.GetAll().ToArray()[0];
            Assert.True(hostService.Exists(host.FullName));
        }

        [Theory, AutoHostData(RepeatCount)]
        public void DoesNotExistIsOk(GroupedHostService hostService)
        {
            Assert.False(hostService.Exists("Group/NonExistingHost"));
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnAdd(GroupedHostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            hostService.Added += (s, e) => { raisedEvents.Add("Added"); };

            hostService.Add(host);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoHostData]
        public void RemoveIsOk(GroupedHostService hostService, Host host)
        {
            hostService.Add(host);
            hostService.Remove(host.FullName);

            Assert.False(hostService.Exists(host.FullName));
            Assert.Equal(0, hostService.Count());
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnRemove(GroupedHostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            hostService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            hostService.Add(host);

            hostService.Remove(host.FullName);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoHostData]
        public void UpdateIsOk(GroupedHostService hostService, Host host)
        {
            hostService.Add(host);
            var updatedHost = new Host(host.Id, host.Name, host.Group) { Priority = 9 };
            hostService.Update(updatedHost);

            hostService.TryGet(host.FullName, out var jb);
            Assert.Equal(updatedHost.Priority, jb.Priority);
        }

        [Theory, AutoHostData]
        public void AddOrUpdateIsOk(GroupedHostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Added += (s, e) => { raisedEvents.Add("Added"); };
            hostService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            hostService.AddOrUpdate(host);
            var updated = new Host(host.Id, host.Name, host.Group) { Priority = 9 };
            hostService.AddOrUpdate(updated);

            hostService.TryGet(host.FullName, out var jb);
            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Priority, jb.Priority);
        }

        [Theory, AutoHostData]
        public void TryAddIsOk(GroupedHostService hostService, Host host)
        {
            Assert.True(hostService.TryAdd(host));
            hostService.TryGet(host.FullName, out var jb);
            Assert.Equal(host.Id, jb.Id);
        }

        [Theory, AutoHostData]
        public void EventsAreRaisedOnUpdate(GroupedHostService hostService, Host host)
        {
            var raisedEvents = new List<string>();
            hostService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            hostService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            hostService.Add(host);

            var updatedHost = new Host(host.Id, host.Name, host.Group) { Priority = 9 };
            hostService.Update(updatedHost);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }
    }
}
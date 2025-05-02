namespace DHI.Services.Test.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.Authorization;
    using Xunit;

    public class UserGroupServiceTest
    {
        private const int RepeatCount = 10;

        [Theory]
        [AutoUserGroupData]
        public void UpdateNonExistingThrows(UserGroupService userGroupService, UserGroup userGroup)
        {
            Assert.Throws<KeyNotFoundException>(() => userGroupService.Update(userGroup));
        }

        [Theory]
        [AutoUserGroupData]
        public void RemoveNonExistingThrows(UserGroupService userGroupService, UserGroup userGroup)
        {
            Assert.Throws<KeyNotFoundException>(() => userGroupService.Remove(userGroup.Id));
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UserGroupService(null));
        }

        [Theory]
        [AutoUserGroupData]
        public void AddUserToNonExistingGroupThrows(UserGroupService userGroupService, string userId)
        {
            Assert.Throws<KeyNotFoundException>(() => userGroupService.AddUser("nonExistingGroup", userId));
        }

        [Theory]
        [AutoUserGroupData(RepeatCount)]
        public void AddNullOrEmptyUserThrows(UserGroupService userGroupService)
        {
            Assert.Throws<ArgumentNullException>(() => userGroupService.AddUser(userGroupService.GetAll().First().Id, null));
            Assert.Throws<ArgumentException>(() => userGroupService.AddUser(userGroupService.GetAll().First().Id, ""));
        }

        [Theory]
        [AutoUserGroupData]
        public void RemoveUserFromNonExistingGroupThrows(UserGroupService userGroupService, string userId)
        {
            Assert.Throws<KeyNotFoundException>(() => userGroupService.RemoveUser("nonExistingGroup", userId));
        }

        [Theory]
        [AutoUserGroupData]
        public void ContainsUserForNonExistingGroupThrows(UserGroupService userGroupService, string userId)
        {
            Assert.False( userGroupService.ContainsUser("nonExistingGroup", userId));
        }

        [Theory]
        [AutoUserGroupData]
        public void AnyContainsUserForNonExistingGroupsThrows(UserGroupService userGroupService, string userId)
        {
            Assert.False(userGroupService.AnyContainsUser(new List<string> { "nonExistingGroup" }, userId));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void GetAllIsOk(UserGroupService userGroupService)
        {
            Assert.Equal(RepeatCount, userGroupService.GetAll().Count());
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void GetIdsIsOk(UserGroupService userGroupService)
        {
            Assert.Equal(RepeatCount, userGroupService.GetIds().Count());
        }

        [Theory, AutoUserGroupData]
        public void AddAndGetIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            userGroupService.Add(userGroup);
            userGroupService.TryGet(userGroup.Id, out var myEntity);
            Assert.Equal(userGroup.Id, myEntity.Id);
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void CountIsOk(UserGroupService userGroupService)
        {
            Assert.Equal(RepeatCount, userGroupService.Count());
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void ExistsIsOk(UserGroupService userGroupService)
        {
            var userGroup = userGroupService.GetAll().ToArray()[0];
            Assert.True(userGroupService.Exists(userGroup.Id));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void DoesNotExistsIsOk(UserGroupService userGroupService)
        {
            Assert.False(userGroupService.Exists("NonExistingConnection"));
        }

        [Theory, AutoUserGroupData]
        public void EventsAreRaisedOnAdd(UserGroupService userGroupService, UserGroup userGroup)
        {
            var raisedEvents = new List<string>();
            userGroupService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            userGroupService.Added += (s, e) => { raisedEvents.Add("Added"); };

            userGroupService.Add(userGroup);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoUserGroupData]
        public void RemoveIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            userGroupService.Add(userGroup);
            userGroupService.Remove(userGroup.Id);

            Assert.False(userGroupService.Exists(userGroup.Id));
            Assert.Equal(0, userGroupService.Count());
        }

        [Theory, AutoUserGroupData]
        public void EventsAreRaisedOnRemove(UserGroupService userGroupService, UserGroup userGroup)
        {
            var raisedEvents = new List<string>();
            userGroupService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            userGroupService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            userGroupService.Add(userGroup);

            userGroupService.Remove(userGroup.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoUserGroupData]
        public void UpdateIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            userGroupService.Add(userGroup);

            const string newName = "New name";
            var updated = new UserGroup(userGroup.Id, newName);
            userGroupService.Update(updated);

            userGroupService.TryGet(userGroup.Id, out var myEntity);
            Assert.Equal(newName, myEntity.Name);
        }

        [Theory, AutoUserGroupData]
        public void AddOrUpdateIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            var raisedEvents = new List<string>();
            userGroupService.Added += (s, e) => { raisedEvents.Add("Added"); };
            userGroupService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            userGroupService.AddOrUpdate(userGroup);
            const string newName = "New name";
            var updated = new UserGroup(userGroup.Id, newName);
            userGroupService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            userGroupService.TryGet(userGroup.Id, out var myEntity);
            Assert.Equal(newName, myEntity.Name);
        }

        [Theory, AutoUserGroupData]
        public void TryAddIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            Assert.True(userGroupService.TryAdd(userGroup));
            userGroupService.TryGet(userGroup.Id, out var myEntity);
            Assert.Equal(userGroup.Id, myEntity.Id);
        }

        [Theory, AutoUserGroupData]
        public void TryAddExistingReturnsFalse(UserGroupService userGroupService, UserGroup userGroup)
        {
            userGroupService.Add(userGroup);
            Assert.False(userGroupService.TryAdd(userGroup));
        }

        [Theory, AutoUserGroupData]
        public void TryUpdateIsOk(UserGroupService userGroupService, UserGroup userGroup)
        {
            userGroupService.Add(userGroup);

            const string newName = "New name";
            var updated = new UserGroup(userGroup.Id, newName);

            Assert.True(userGroupService.TryUpdate(updated));
            userGroupService.TryGet(userGroup.Id, out var myEntity);
            Assert.Equal(newName, myEntity.Name);
        }

        [Theory, AutoUserGroupData]
        public void TryUpdateNonExistingReturnsFalse(UserGroupService userGroupService, UserGroup userGroup)
        {
            Assert.False(userGroupService.TryUpdate(userGroup));
        }

        [Theory, AutoUserGroupData]
        public void EventsAreRaisedOnUpdate(UserGroupService userGroupService, UserGroup userGroup)
        {
            var raisedEvents = new List<string>();
            userGroupService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            userGroupService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            userGroupService.Add(userGroup);

            const string newName = "New name";
            var updated = new UserGroup(userGroup.Id, newName);
            userGroupService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void AddAndRemoveUserIsOk(UserGroupService userGroupService, string userId)
        {
            var firstGroup = userGroupService.GetAll().First().Id;
            var lastGroup = userGroupService.GetAll().Last().Id;

            Assert.True(userGroupService.AddUser(firstGroup, userId));
            Assert.True(userGroupService.ContainsUser(firstGroup, userId));
            Assert.False(userGroupService.ContainsUser(lastGroup, userId));
            Assert.True(userGroupService.RemoveUser(firstGroup, userId));
            Assert.False(userGroupService.ContainsUser(firstGroup, userId));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void RemoveUserFromAllGroupsIsOk(UserGroupService userGroupService, string userId)
        {
            var firstGroup = userGroupService.GetAll().First().Id;
            var lastGroup = userGroupService.GetAll().Last().Id;

            Assert.True(userGroupService.AddUser(firstGroup, userId));
            Assert.True(userGroupService.AddUser(lastGroup, userId));
            Assert.True(userGroupService.ContainsUser(firstGroup, userId));
            Assert.True(userGroupService.ContainsUser(lastGroup, userId));
            userGroupService.RemoveUser(userId);

            Assert.False(userGroupService.ContainsUser(firstGroup, userId));
            Assert.False(userGroupService.ContainsUser(lastGroup, userId));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void AddExistingUserReturnsFalse(UserGroupService userGroupService, string userId)
        {
            var group = userGroupService.GetAll().First().Id;
            Assert.True(userGroupService.AddUser(group, userId));
            Assert.False(userGroupService.AddUser(group, userId));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void RemoveNonExistingUserReturnsFalse(UserGroupService userGroupService)
        {
            var group = userGroupService.GetAll().First().Id;
            Assert.False(userGroupService.RemoveUser(group, "NonExistingUser"));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void AnyContainsUserIsOk(UserGroupService userGroupService, string userId)
        {
            var firstGroup = userGroupService.GetAll().First().Id;
            var lastGroup = userGroupService.GetAll().Last().Id;

            Assert.False(userGroupService.AnyContainsUser(new List<string> { firstGroup, lastGroup }, userId));
            Assert.True(userGroupService.AddUser(firstGroup, userId));
            Assert.True(userGroupService.AnyContainsUser(new List<string> { firstGroup, lastGroup }, userId));
        }

        [Theory, AutoUserGroupData(RepeatCount)]
        public void GetIdsForUserIsOk(UserGroupService userGroupService, string userId)
        {
            var firstGroup = userGroupService.GetAll().First().Id;
            var lastGroup = userGroupService.GetAll().Last().Id;

            Assert.True(userGroupService.AddUser(firstGroup, userId));
            Assert.True(userGroupService.AddUser(lastGroup, userId));

            var groupIds = userGroupService.GetIds(userId).ToArray();
            Assert.Equal(2, groupIds.Length);
            Assert.Contains(firstGroup, groupIds);
            Assert.Contains(lastGroup, groupIds);
        }
    }
}
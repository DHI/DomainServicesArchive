namespace DHI.Services.Scalars.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Xunit;

    public class GroupedScalarServiceTest
    {
        private const int RepeatCount = 3;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedScalarService<Guid, int>(null));
        }

        [Theory, AutoScalarData]
        public void GetNonExistingThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.False(scalarService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoScalarData]
        public void UpdateNonExistingThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.Update(scalar));
        }

        [Theory, AutoScalarData]
        public void SetDataForNonExistingThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.SetData(scalar.Id, scalar.GetData().Value));
        }

        [Theory, AutoScalarData]
        public void RemoveNonExistingThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.Remove(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void GetByGroupForNonExistingThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoScalarData]
        public void GetByGroupForNullGroupThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Throws<ArgumentNullException>(() => scalarService.GetByGroup(null));
        }

        [Theory, AutoScalarData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoScalarData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Throws<ArgumentNullException>(() => scalarService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => scalarService.GetFullNames(""));
        }

        [Theory, AutoScalarData]
        public void AddExistingThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            Assert.Throws<ArgumentException>(() => scalarService.Add(scalar));
        }

        [Theory, AutoScalarData]
        public void AddWithExistingIdThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var newScalar = new Scalar<Guid, int>(scalar.Id, "NewName", scalar.Group);
            Assert.Throws<ArgumentException>(() => scalarService.Add(newScalar));
        }

        [Theory, AutoScalarData]
        public void UpdateLockedThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = scalar.Clone<Scalar<Guid, int>>();
            Assert.Throws<Exception>(() => scalarService.Update(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void AddOrUpdateLockedThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = scalar.Clone<Scalar<Guid, int>>();
            Assert.Throws<Exception>(() => scalarService.AddOrUpdate(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void SetDataForLockedThrows(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            Assert.Throws<Exception>(() => scalarService.SetData(scalar.Id, scalarData));
        }

        [Theory, AutoScalarData]
        public void SetLockedForNonExistingThrows(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.SetLocked(Guid.NewGuid(), false));
        }

        [Theory, AutoScalarData]
        public void GetByGroupIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            var group = scalarService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(scalarService.GetByGroup(group).Any());
        }

        [Theory, AutoScalarData]
        public void GetFullNamesByGroupIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            var group = scalarService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = scalarService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoScalarData]
        public void GetFullNamesIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.GetFullNames().Count());
        }

        [Theory, AutoScalarData]
        public void GetAllIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.GetAll().Count());
        }

        [Theory, AutoScalarData]
        public void GetIdsIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.GetIds().Count());
        }

        [Theory, AutoScalarData]
        public void AddAndGetIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(scalar.Id, sc.Id);
        }

        [Theory, AutoScalarData]
        public void CountIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.Count());
        }

        [Theory, AutoScalarData]
        public void ExistsIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            var scalar = scalarService.GetAll().ToArray()[0];
            Assert.True(scalarService.Exists(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void DoesNotExistIsOk(GroupedScalarService<Guid, int> scalarService)
        {
            Assert.False(scalarService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnAdd(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            scalarService.Added += (s, e) => { raisedEvents.Add("Added"); };

            scalarService.Add(scalar);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoScalarData]
        public void RemoveIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            scalarService.Remove(scalar.Id);

            Assert.False(scalarService.Exists(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnRemove(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            scalarService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            scalarService.Add(scalar);

            scalarService.Remove(scalar.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoScalarData]
        public void UpdateIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };
            scalarService.Update(updatedScalar);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(updatedScalar.Description, sc.Description);
        }

        [Theory, AutoScalarData]
        public void SetLockedIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            scalarService.SetLocked(scalar.Id, false);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.False(sc.Locked);
        }

        [Theory, AutoScalarData]
        public void SetDataIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedData = new ScalarData<int>(999, DateTime.Now);
            scalarService.SetData(scalar.Id, updatedData);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(999, sc.GetData().Value.Value);
        }

        [Theory, AutoScalarData]
        public void AddOrUpdateIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Added += (s, e) => { raisedEvents.Add("Added"); };
            scalarService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scalarService.AddOrUpdate(scalar);
            var updated = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };
            scalarService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(updated.Description, sc.Description);
        }

        [Theory, AutoScalarData]
        public void TrySetDataOrAddIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Added += (s, e) => { raisedEvents.Add("Added"); };
            scalarService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scalarService.TrySetDataOrAdd(scalar);
            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(new ScalarData<int>(999, DateTime.Now));

            Assert.True(scalarService.TrySetDataOrAdd(updated));
            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(999, sc.GetData().Value.Value);
        }

        [Theory, AutoScalarData]
        public void TrySetDataOrAddLockedReturnsFalse(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(new ScalarData<int>(999, DateTime.Now));

            Assert.False(scalarService.TrySetDataOrAdd(updated));
        }

        [Theory, AutoScalarData]
        public void TryAddIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.True(scalarService.TryAdd(scalar));
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(scalar.Id, sc.Id);
        }

        [Theory, AutoScalarData]
        public void TryUpdateIsOk(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };

            Assert.True(scalarService.TryUpdate(updatedScalar));
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(updatedScalar.Description, sc.Description);
        }

        [Theory, AutoScalarData]
        public void TryUpdateNonExistingReturnsFalse(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.False(scalarService.TryUpdate(scalar));
        }

        [Theory, AutoScalarData]
        public void TryUpdateLockedReturnsFalse(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };

            Assert.False(scalarService.TryUpdate(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnUpdate(GroupedScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            scalarService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scalarService.Add(scalar);

            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };
            scalarService.Update(updatedScalar);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnUpdate(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.Update(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnAddOrUpdate(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.AddOrUpdate(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnSetData(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);
            scalarService.SetData(scalar.Id, scalarData);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreNotLoggedOnSetData(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);
            scalarService.SetData(scalar.Id, scalarData, false);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnTrySetDataOrAdd(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.TrySetDataOrAdd(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreNotLoggedOnTrySetDataOrAdd(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.TrySetDataOrAdd(updated, false);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnTryUpdate(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.TryUpdate(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreNotLoggedOnUpdateIfDataIsUnchanged(Scalar<Guid, int> scalar)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            scalarService.Update(updated);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreNotLoggedOnAddOrUpdateIfDataIsUnchanged(Scalar<Guid, int> scalar)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            scalarService.AddOrUpdate(updated);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnUpdateIfDataBeforeIsEmpty(ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            var scalar = new Scalar<Guid, int>(Guid.NewGuid(), "Pending", "System.Int32");
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(scalarData);
            scalarService.Update(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains("Old data: 'Empty'", logEntry);
            Assert.Contains($"New data: '{scalarData}'", logEntry);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnUpdateIfDataAfterIsEmpty(Scalar<Guid, int> scalar)
        {
            var logger = new FakeLogger();
            var scalarService = new GroupedScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.ValueTypeName);
            scalarService.Update(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains("New data: 'Empty'", logEntry);
        }
    }
}
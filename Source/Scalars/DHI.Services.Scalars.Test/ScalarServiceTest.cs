namespace DHI.Services.Scalars.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ScalarServiceTest
    {
        private const int RepeatCount = 3;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ScalarService<Guid, int>(null));
        }

        [Theory]
        [AutoScalarData]
        public void GetNonExistingThrows(ScalarService<Guid, int> scalarService)
        {
            Assert.False(scalarService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoScalarData]
        public void UpdateNonExistingThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.Update(scalar));
        }

        [Theory, AutoScalarData]
        public void SetDataForNonExistingThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.SetData(scalar.Id, scalar.GetData().Value));
        }

        [Theory, AutoScalarData]
        public void RemoveNonExistingThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.Remove(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void AddExistingThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            Assert.Throws<ArgumentException>(() => scalarService.Add(scalar));
        }

        [Theory, AutoScalarData]
        public void UpdateLockedThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = scalar.Clone<Scalar<Guid, int>>();
            Assert.Throws<Exception>(() => scalarService.Update(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void AddOrUpdateLockedThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = scalar.Clone<Scalar<Guid, int>>();
            Assert.Throws<Exception>(() => scalarService.AddOrUpdate(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void SetDataForLockedThrows(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            Assert.Throws<Exception>(() => scalarService.SetData(scalar.Id, scalarData));
        }

        [Theory, AutoScalarData]
        public void SetLockedForNonExistingThrows(ScalarService<Guid, int> scalarService)
        {
            Assert.Throws<KeyNotFoundException>(() => scalarService.SetLocked(Guid.NewGuid(), false));
        }

        [Theory, AutoScalarData]
        public void GetAllIsOk(ScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.GetAll().Count());
        }

        [Theory, AutoScalarData]
        public void GetIdsIsOk(ScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.GetIds().Count());
        }

        [Theory, AutoScalarData]
        public void AddAndGetIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            Assert.Equal(scalar.Id, scalarService.TryGet(scalar.Id, out var s) ? s.Id : Guid.Empty);
        }

        [Theory, AutoScalarData]
        public void CountIsOk(ScalarService<Guid, int> scalarService)
        {
            Assert.Equal(RepeatCount, scalarService.Count());
        }

        [Theory, AutoScalarData]
        public void ExistsIsOk(ScalarService<Guid, int> scalarService)
        {
            var scalar = scalarService.GetAll().ToArray()[0];
            Assert.True(scalarService.Exists(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void DoesNotExistsIsOk(ScalarService<Guid, int> scalarService)
        {
            Assert.False(scalarService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnAdd(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            scalarService.Added += (s, e) => { raisedEvents.Add("Added"); };

            scalarService.Add(scalar);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoScalarData]
        public void RemoveIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            scalarService.Remove(scalar.Id);

            Assert.False(scalarService.Exists(scalar.Id));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnRemove(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
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
        public void UpdateIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, "Updated name", "System.String");
            scalarService.Update(updatedScalar);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal("Updated name", sc.Name);
        }

        [Theory, AutoScalarData]
        public void SetDataIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedData = new ScalarData<int>(999, DateTime.Now);
            scalarService.SetData(scalar.Id, updatedData);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(999, sc.GetData().Value.Value);
        }

        [Theory, AutoScalarData]
        public void SetLockedIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            scalarService.SetLocked(scalar.Id, false);

            scalarService.TryGet(scalar.Id, out var sc);
            Assert.False(sc.Locked);
        }

        [Theory, AutoScalarData]
        public void AddOrUpdateIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Added += (s, e) => { raisedEvents.Add("Added"); };
            scalarService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scalarService.AddOrUpdate(scalar);
            var updated = new Scalar<Guid, int>(scalar.Id, "Updated name", "System.Boolean");
            scalarService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(updated.Name, sc.Name);
        }

        [Theory, AutoScalarData]
        public void TrySetDataOrAddIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
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
        public void TrySetDataOrAddLockedReturnsFalse(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updated = scalar.Clone<Scalar<Guid, int>>();
            updated.SetData(new ScalarData<int>(999, DateTime.Now));

            Assert.False(scalarService.TrySetDataOrAdd(updated));
        }

        [Theory, AutoScalarData]
        public void TryAddIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.True(scalarService.TryAdd(scalar));
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(scalar.Id, sc.Id);
        }

        [Theory, AutoScalarData]
        public void TryUpdateIsOk(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };

            Assert.True(scalarService.TryUpdate(updatedScalar));
            scalarService.TryGet(scalar.Id, out var sc);
            Assert.Equal(updatedScalar.Description, sc.Description);
        }

        [Theory, AutoScalarData]
        public void TryUpdateNonExistingReturnsFalse(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            Assert.False(scalarService.TryUpdate(scalar));
        }

        [Theory, AutoScalarData]
        public void TryUpdateLockedReturnsFalse(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            scalar.Locked = true;
            scalarService.Add(scalar);
            var updatedScalar = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.Group) { Description = "New description" };

            Assert.False(scalarService.TryUpdate(updatedScalar));
        }

        [Theory, AutoScalarData]
        public void EventsAreRaisedOnUpdate(ScalarService<Guid, int> scalarService, Scalar<Guid, int> scalar)
        {
            var raisedEvents = new List<string>();
            scalarService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            scalarService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scalarService.Add(scalar);

            var updatedScalar = new Scalar<Guid, int>(scalar.Id, "Updated name", "System.Int32");
            scalarService.Update(updatedScalar);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnUpdate(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);
            scalarService.SetData(scalar.Id, scalarData, false);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnTrySetDataOrAdd(Scalar<Guid, int> scalar, ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            scalarService.Update(updated);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreNotLoggedOnAddOrUpdateIfDataIsUnchanged(Scalar<Guid, int> scalar)
        {
            var logger = new FakeLogger();
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = scalar.Clone<Scalar<Guid, int>>();
            scalarService.AddOrUpdate(updated);

            Assert.Empty(logger.LogEntries);
        }

        [Theory, AutoScalarData]
        public void DataChangesAreLoggedOnUpdateIfDataBeforeIsEmpty(ScalarData<int> scalarData)
        {
            var logger = new FakeLogger();
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
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
            var scalarService = new ScalarService<Guid, int>(new FakeScalarRepository(), logger);
            scalarService.Add(scalar);

            var updated = new Scalar<Guid, int>(scalar.Id, scalar.Name, scalar.ValueTypeName);
            scalarService.Update(updated);

            var logEntry = logger.LogEntries.Single();
            Assert.Contains($"Old data: '{scalar.GetData().Value}'", logEntry);
            Assert.Contains("New data: 'Empty'", logEntry);
        }
    }
}
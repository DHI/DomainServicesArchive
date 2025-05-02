namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ConnectionServiceTest
    {
        private const int RepeatCount = 10;
        private List<FakeConnection> fakeConnections = new List<FakeConnection>() { new FakeConnection("fake1", "fake1"), new FakeConnection("fake2", "fake2"), new FakeConnection("fake3", "fake3"), };

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ConnectionService(null));
        }

        [Theory, ConnectionData]
        public void GetNonExistingThrows(ConnectionService connectionService)
        {
            Assert.False(connectionService.TryGet("UnknownConnection", out _));
        }

        [Theory, ConnectionData]
        public void UpdateNonExistingThrows(ConnectionService connectionService, FakeConnection connection)
        {
            Assert.Throws<KeyNotFoundException>(() => connectionService.Update(connection));
        }

        [Theory, ConnectionData]
        public void RemoveNonExistingThrows(ConnectionService connectionService, FakeConnection connection)
        {
            Assert.Throws<KeyNotFoundException>(() => connectionService.Remove(connection.Id));
        }

        [Theory, ConnectionData]
        public void GetAllIsOk(ConnectionService connectionService)
        {
            _ResetConnections(connectionService);
            _LoadConnections(connectionService);
            Assert.Equal(fakeConnections.Count(), connectionService.GetAll().Count());
            _RemoveConnections(connectionService);
        }

        [Theory, AutoConnectionData(RepeatCount)]
        public void GetIdsIsOk(ConnectionService connectionService)
        {
            Assert.Equal(RepeatCount, connectionService.GetIds().Count());
        }

        [Theory, ConnectionData]
        public void AddAndGetIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            connectionService.Add(connection);
            connectionService.TryGet(connection.Id, out var myEntity);
            Assert.Equal(connection.Id, myEntity.Id);
            connectionService.Remove(connection.Id);
        }

        [Theory, ConnectionData]
        public void CountIsOk(ConnectionService connectionService)
        {
            _ResetConnections(connectionService);
            _LoadConnections(connectionService);
            Assert.Equal(fakeConnections.Count(), connectionService.Count());
            _RemoveConnections(connectionService);
        }

        [Theory, ConnectionData]
        public void ExistsIsOk(ConnectionService connectionService)
        {
            var connection = connectionService.GetAll().ToArray()[0];
            Assert.True(connectionService.Exists(connection.Id));
        }

        [Theory, ConnectionData]
        public void DoesNotExistsIsOk(ConnectionService connectionService)
        {
            Assert.False(connectionService.Exists("NonExistingConnection"));
        }

        [Theory, ConnectionData]
        public void EventsAreRaisedOnAdd(ConnectionService connectionService, FakeConnection connection)
        {
            var raisedEvents = new List<string>();
            connectionService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            connectionService.Added += (s, e) => { raisedEvents.Add("Added"); };

            connectionService.Add(connection);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
            connectionService.Remove(connection.Id);
        }

        [Theory, ConnectionData]
        public void RemoveIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            _ResetConnections(connectionService);
            connectionService.Add(connection);
            connectionService.Remove(connection.Id);

            Assert.False(connectionService.Exists(connection.Id));
            Assert.Equal(0, connectionService.Count());
        }

        [Theory, ConnectionData]
        public void EventsAreRaisedOnRemove(ConnectionService connectionService, FakeConnection connection)
        {
            var raisedEvents = new List<string>();
            connectionService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            connectionService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            connectionService.Add(connection);

            connectionService.Remove(connection.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, ConnectionData]
        public void UpdateIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            connectionService.Add(connection);

            var connectionUpdated = new FakeConnection(connection.Id, "Updated name");
            connectionService.Update(connectionUpdated);

            connectionService.TryGet(connection.Id, out var myEntity);
            Assert.Equal("Updated name", myEntity.Name);
            connectionService.Remove(connection.Id);
        }

        [Theory, ConnectionData]
        public void AddOrUpdateIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            var raisedEvents = new List<string>();
            connectionService.Added += (s, e) => { raisedEvents.Add("Added"); };
            connectionService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            connectionService.AddOrUpdate(connection);
            var updated = new FakeConnection(connection.Id, "Updated name");
            connectionService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            connectionService.TryGet(connection.Id, out var myEntity);
            Assert.Equal(updated.Name, myEntity.Name);
            connectionService.Remove(updated.Id);
        }

        [Theory, ConnectionData]
        public void TryAddIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            Assert.True(connectionService.TryAdd(connection));
            connectionService.TryGet(connection.Id, out var myEntity);
            Assert.Equal(connection.Id, myEntity.Id);
            connectionService.Remove(connection.Id);
        }

        [Theory, ConnectionData]
        public void TryAddExistingReturnsFalse(ConnectionService connectionService, FakeConnection connection)
        {
            connectionService.Add(connection);
            Assert.False(connectionService.TryAdd(connection));
        }

        [Theory, ConnectionData]
        public void TryUpdateIsOk(ConnectionService connectionService, FakeConnection connection)
        {
            connectionService.Add(connection);

            var connectionUpdated = new FakeConnection(connection.Id, "Updated name");

            Assert.True(connectionService.TryUpdate(connectionUpdated));
            connectionService.TryGet(connection.Id, out var myEntity);
            Assert.Equal("Updated name", myEntity.Name);
            connectionService.Remove(connection.Id);
        }

        [Theory, ConnectionData]
        public void TryUpdateNonExistingReturnsFalse(ConnectionService connectionService, FakeConnection connection)
        {
            Assert.False(connectionService.TryUpdate(connection));
        }

        [Theory, ConnectionData]
        public void EventsAreRaisedOnUpdate(ConnectionService connectionService, FakeConnection connection)
        {
            var raisedEvents = new List<string>();
            connectionService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            connectionService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            connectionService.Add(connection);

            var connectionUpdated = new FakeConnection(connection.Id, "Updated name");
            connectionService.Update(connectionUpdated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            connectionService.Remove(connection.Id);
        }

        private void _LoadConnections(ConnectionService connectionService)
        {
            foreach (var fake in fakeConnections)
            {
                connectionService.Add(fake);
            }
        }

        private void _RemoveConnections(ConnectionService connectionService)
        {
            foreach (var fake in fakeConnections)
            {
                connectionService.Remove(fake.Id);
            }
        }

        private void _ResetConnections(ConnectionService connectionService)
        {
            var all = connectionService.GetAll();
            foreach (var fake in all)
            {
                connectionService.Remove(fake.Id);
            }
        }
    }
}
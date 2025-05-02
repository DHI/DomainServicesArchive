namespace DHI.Services.Test.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using DHI.Services.Filters;
    using DHI.Services.Notifications;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class FilterServiceTest
    {
        private readonly FilterService _service;

        public FilterServiceTest()
        {
            _service = new FilterService(new FakeFilterRepository(), NullLogger.Instance);
            _service.Add(new Filter("foo", new[] { new QueryCondition("bar", "baz") }));
            _service.Add(new Filter("foo", "myConnectionId", new[] { new QueryCondition("bar", "baz") }));
        }

        [Fact]
        public async Task GetListWithNullOrEmptyDataTypeThrows()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetListAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetListAsync(""));
        }

        [Fact]
        public async Task GetIdsWithNullOrEmptyConnectionIdThrows()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetIdsAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetIdsAsync(""));
        }

        [Fact]
        public async Task GetListIsOk()
        {
            Assert.Equal(2, (await _service.GetListAsync("foo")).Count());
            Assert.Single(await _service.GetListAsync("foo", "myConnectionId"));
            Assert.Empty(await _service.GetListAsync("foo", "NonExistingConnectionId"));
            Assert.Empty(await _service.GetListAsync("bar"));
        }

        [Theory, AutoData]
        public async Task GetIdsByTransportConnectionIsOk(string transportConnectionId)
        {
            var filterId = _service.GetIds().First();
            await _service.TryAddTransportConnectionAsync(transportConnectionId, filterId);
            var ids = (await _service.GetIdsAsync(transportConnectionId)).ToArray();
            Assert.Single(ids);
            Assert.Equal(filterId, ids.Single());
        }

        [Theory, AutoData]
        public async Task TryAddTransportConnectionIsOk(string connectionId)
        {
            var raisedEvents = new List<string>();
            _service.AddingTransportConnection += (s, e) => { raisedEvents.Add("AddingTransportConnection"); };
            _service.TransportConnectionAdded += (s, e) => { raisedEvents.Add("TransportConnectionAdded"); };
            var filterId = _service.GetIds().First();
            var result = await _service.TryAddTransportConnectionAsync(connectionId, filterId);

            Assert.True(result);
            _service.TryGet(filterId, out var myEntity);
            Assert.Single(myEntity.TransportConnections);
            Assert.Equal("AddingTransportConnection", raisedEvents[0]);
            Assert.Equal("TransportConnectionAdded", raisedEvents[1]);
        }

        [Theory, AutoData]
        public async Task TryAddTransportConnectionToNonExistingFilterLogsError(string connectionId, string filterId)
        {
            var result = await _service.TryAddTransportConnectionAsync(connectionId, filterId);
            Assert.False(result);
        }

        [Theory, AutoData]
        public async Task TryDeleteTransportConnectionIsOk(string connectionId)
        {
            var raisedEvents = new List<string>();
            _service.DeletingTransportConnection += (s, e) => { raisedEvents.Add("DeletingTransportConnection"); };
            _service.TransportConnectionDeleted += (s, e) => { raisedEvents.Add("TransportConnectionDeleted"); };
            var filterId = _service.GetIds().First();
            await _service.TryAddTransportConnectionAsync(connectionId, filterId);
            var result = await _service.TryDeleteTransportConnectionAsync(connectionId, filterId);

            Assert.True(result);
            _service.TryGet(filterId, out var myEntity);
            Assert.Empty(myEntity.TransportConnections);
            Assert.Equal("DeletingTransportConnection", raisedEvents[0]);
            Assert.Equal("TransportConnectionDeleted", raisedEvents[1]);
        }

        [Theory, AutoData]
        public async Task TryDeleteTransportConnectionFromNonExistingFilterLogsError(string connectionId, string filterId)
        {
            var result = await _service.TryDeleteTransportConnectionAsync(connectionId, filterId);
            Assert.False(result);
        }
    }
}
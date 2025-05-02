namespace DHI.Services.Test.Filters
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DHI.Services.Filters;
    using Xunit;

    public class FilterRepositoryTest : IDisposable
    {
        public FilterRepositoryTest()
        {
            _repository = new FilterRepository(_filePath);
        }

        private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__filters.json");
        private readonly FilterRepository _repository;

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Theory, AutoFilterData]
        public void AddExistingThrows(Filter filter)
        {
            _repository.Add(filter);
            Assert.Throws<ArgumentException>(() => _repository.Add(filter));
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new FilterRepository(null));
        }

        [Theory, AutoFilterData]
        public void AddAndGetIsOk(Filter filter)
        {
            _repository.Add(filter);
            var actual = _repository.Get(filter.Id).Value;
            Assert.Equal(filter.Id, actual.Id);
            Assert.Equal(filter.DataType, actual.DataType);
            Assert.Equal(filter.TransportConnections, actual.TransportConnections);
            Assert.Equal(filter.QueryConditions.First().Item, actual.QueryConditions.First().Item);
        }

        [Theory, AutoFilterData]
        public async Task GetByDataTypeIsOk(Filter filter1, Filter filter2)
        {
            _repository.Add(filter1);
            _repository.Add(filter2);
            var filters = (await _repository.GetListAsync(filter1.DataType)).ToArray();
            Assert.Single((IEnumerable)filters);
            Assert.Equal(filter1.Id, filters.First().Id);
        }

        [Theory, AutoFilterData]
        public async Task AddAndRemoveTransportConnectionIsOk(Filter filter, string connectionId)
        {
            _repository.Add(filter);
            await _repository.AddTransportConnectionAsync(connectionId, filter.Id);
            Assert.Contains(connectionId, _repository.Get(filter.Id).Value.TransportConnections);
            Assert.Equal(4, await _repository.TransportConnectionsCountAsync(filter.Id));
            await _repository.DeleteTransportConnectionAsync(connectionId, filter.Id);
            Assert.DoesNotContain(connectionId, _repository.Get(filter.Id).Value.TransportConnections);
            Assert.Equal(3, await _repository.TransportConnectionsCountAsync(filter.Id));
        }

        [Theory, AutoFilterData]
        public async Task AddTransportConnectionIsIdempotent(Filter filter, string connectionId)
        {
            _repository.Add(filter);
            await _repository.AddTransportConnectionAsync(connectionId, filter.Id);
            await _repository.AddTransportConnectionAsync(connectionId, filter.Id);
            Assert.Equal(4, await _repository.TransportConnectionsCountAsync(filter.Id));
        }
    }
}
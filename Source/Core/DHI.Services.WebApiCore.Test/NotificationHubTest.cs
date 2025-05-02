namespace DHI.Services.WebApiCore.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class NotificationHubTest
    {
        public NotificationHubTest()
        {
            _logger = new FakeLogger();
            _notificationHub = new NotificationHub(new FakeFilterRepository(), _logger);
        }

        private readonly NotificationHub _notificationHub;
        private readonly FakeLogger _logger;

        [Fact]
        public async Task AddFilterWithNullOrEmptyDataTypeThrows()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationHub.AddFilter(null, new QueryDTO(), "mySource"));
            await Assert.ThrowsAsync<ArgumentException>(() => _notificationHub.AddFilter("", new QueryDTO(), "mySource"));
        }

        [Fact]
        public async Task AddJobFilterWithEmptyDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _notificationHub.AddJobFilter("", new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Required input `dataConnectionId` is empty. (Parameter 'dataConnectionId')", logEntry.Text);
        }

        [Fact]
        public async Task AddJobFilterWithNullDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationHub.AddJobFilter(null, new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Value cannot be null. (Parameter 'dataConnectionId')", logEntry.Text);
        }

        [Fact]
        public async Task AddJsonDocumentFilterWithEmptyDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _notificationHub.AddJsonDocumentFilter("", new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Required input `dataConnectionId` is empty. (Parameter 'dataConnectionId')", logEntry.Text);
        }

        [Fact]
        public async Task AddJsonDocumentFilterWithNullDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationHub.AddJsonDocumentFilter(null, new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Value cannot be null. (Parameter 'dataConnectionId')", logEntry.Text);
        }

        [Fact]
        public async Task AddTimeSeriesFilterWithEmptyDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _notificationHub.AddTimeSeriesFilter("", new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Required input `dataConnectionId` is empty. (Parameter 'dataConnectionId')", logEntry.Text);
        }

        [Fact]
        public async Task AddTimeSeriesFilterWithNullDataConnectionIdThrowsAndLogs()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationHub.AddTimeSeriesFilter(null, new QueryDTO()));
            Assert.Single(_logger.LogEntries);
            var logEntry = _logger.LogEntries.First();
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Contains("Value cannot be null. (Parameter 'dataConnectionId')", logEntry.Text);
        }
    }
}
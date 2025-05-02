namespace DHI.Services.Test.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoFixture.Xunit2;
    using DHI.Services.Logging;
    using DHI.Services.Notifications;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class SimpleLoggerTest
    {
        [Theory]
        [AutoData]
        public void LogToFileIsOk(Guid guid, LogLevel level, string notificationEntry)
        {
            var filePath = Path.Combine(Path.GetTempPath(), "__test.log");
            var logger = new SimpleLogger(filePath);

            logger.Log(level, new EventId(guid.GetHashCode()), notificationEntry, null, (s, _) => s);

            Assert.True(File.Exists(filePath));
            var lines = File.ReadLines(filePath);
            Assert.Single(lines);

            File.Delete(filePath);
        }

        [Theory]
        [AutoData]
        public void LogToFileInNonExistingFolderIsOk(Guid guid, LogLevel level, string notificationEntry)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var filePath = Path.Combine(path, "__test.log");
            var logger = new SimpleLogger(filePath);

            logger.Log(level, new EventId(guid.GetHashCode()), notificationEntry, null, (s, _) => s);

            Assert.True(File.Exists(filePath));
            var lines = File.ReadLines(filePath);
            Assert.Single(lines);

            Directory.Delete(path, true);
        }

        [Fact]
        public void CreateWithNullOrEmptyFilePathThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new SimpleLogger(null!));
            Assert.Contains("Value cannot be null. (Parameter 'filePath')", e.Message);

            var e2 = Assert.Throws<ArgumentException>(() => new SimpleLogger(""));
            Assert.Contains("Required input `filePath` is empty. (Parameter 'filePath')", e2.Message);
        }
    }
}
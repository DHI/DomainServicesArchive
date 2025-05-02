namespace DHI.Services.Test.Notifications
{
    using System;
    using DHI.Services.Notifications;
    using Xunit;

    public class NotificationEntryTest
    {
        [Fact]
        public void CreateWithNullOrEmptySourceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationEntry(NotificationLevel.Error, "SomeError", null));
            Assert.Throws<ArgumentException>(() => new NotificationEntry(NotificationLevel.Error, "SomeError", ""));
        }

        [Fact]
        public void CreateWithNullOrEmptyTextThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationEntry(NotificationLevel.Error, null, null));
            Assert.Throws<ArgumentException>(() => new NotificationEntry(NotificationLevel.Error, "", null));
        }

        [Fact]
        public void CreateGeneratesUniqueId()
        {
            var entry = new NotificationEntry(NotificationLevel.Information, "my-text", "my-source");
            Assert.IsType<Guid>(entry.Id);
        }

        [Fact]
        public void CreateSetsDateTimeNow()
        {
            var entry = new NotificationEntry(NotificationLevel.Information, "my-text", "my-source");
            Assert.True(DateTime.Now - entry.DateTime < TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void CreateSetsMachineName()
        {
            var entry = new NotificationEntry(NotificationLevel.Information, "my-text", "my-source");
            Assert.Equal(Environment.GetEnvironmentVariable("COMPUTERNAME"), entry.MachineName);
        }

        [Fact]
        public void IsImmutable()
        {
            var entry = new NotificationEntry(NotificationLevel.Information, "my-text", "my-source");
            Assert.Throws<NotSupportedException>(() => entry.Metadata.Add("Description", "Entry description"));
        }
    }
}
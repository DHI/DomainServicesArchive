namespace DHI.Services.Test.Notifications
{
    using DHI.Services.Logging;
    using DHI.Services.Notifications;
    using Xunit;

    public class NotificationServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionTypeIsOk()
        {
            var connectionType = NotificationServiceConnection.CreateConnectionType<NotificationServiceConnection>();

            Assert.Equal("NotificationServiceConnection", connectionType.Id);
            Assert.Equal(typeof(NotificationServiceConnection), connectionType.Type);
            Assert.Contains(typeof(JsonNotificationRepository), connectionType.ProviderTypes[0].Options);
            Assert.Contains(typeof(FakeNotificationRepository), connectionType.ProviderTypes[0].Options);
        }
    }
}
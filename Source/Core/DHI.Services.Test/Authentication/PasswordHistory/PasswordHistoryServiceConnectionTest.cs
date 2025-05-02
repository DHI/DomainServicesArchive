namespace DHI.Services.Test.Authentication.PasswordHistory
{
    using DHI.Services.Authentication.PasswordHistory;
    using Xunit;

    public class PasswordHistoryServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionPasswordHistoryTypeIsOk()
        {
            var connectionType = PasswordHistoryServiceConnection.CreateConnectionType<PasswordHistoryServiceConnection>();

            Assert.Equal("PasswordHistoryServiceConnection", connectionType.Id);
            Assert.Equal(typeof(PasswordHistoryServiceConnection), connectionType.Type);
            Assert.Contains(typeof(PasswordHistoryRepository), connectionType.ProviderTypes[0].Options);
        }
    }
}

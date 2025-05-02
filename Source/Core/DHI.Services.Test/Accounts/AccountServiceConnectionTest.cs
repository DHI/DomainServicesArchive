namespace DHI.Services.Test
{
    using Accounts;
    using Xunit;

    public class AccountServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionTypeIsOk()
        {
            var connectionType = AccountServiceConnection.CreateConnectionType<AccountServiceConnection>();

            Assert.Equal("AccountServiceConnection", connectionType.Id);
            Assert.Equal(typeof(AccountServiceConnection), connectionType.Type);
            Assert.Contains(typeof(AccountRepository), connectionType.ProviderTypes[0].Options);
        }
    }
}
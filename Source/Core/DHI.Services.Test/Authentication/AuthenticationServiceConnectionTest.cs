namespace DHI.Services.Test.Authentication
{
    using Accounts;
    using DHI.Services.Authentication;
    using Xunit;

    public class AuthenticationServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionTypeIsOk()
        {
            var connectionType = AuthenticationServiceConnection.CreateConnectionType<AuthenticationServiceConnection>();

            Assert.Equal("AuthenticationServiceConnection", connectionType.Id);
            Assert.Equal(typeof(AuthenticationServiceConnection), connectionType.Type);
            Assert.Contains(typeof(AccountRepository), connectionType.ProviderTypes[0].Options);
            Assert.True(connectionType.ProviderTypes[0].Mandatory);
        }
    }
}
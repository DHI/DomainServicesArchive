namespace DHI.Services.Test.Authentication
{
    using DHI.Services.Authentication;
    using Xunit;

    public class RefreshTokenServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionTypeIsOk()
        {
            var connectionType = RefreshTokenServiceConnection.CreateConnectionType<RefreshTokenServiceConnection>();

            Assert.Equal("RefreshTokenServiceConnection", connectionType.Id);
            Assert.Equal(typeof(RefreshTokenServiceConnection), connectionType.Type);
            Assert.Contains(typeof(RefreshTokenRepository), connectionType.ProviderTypes[0].Options);
        }
    }
}
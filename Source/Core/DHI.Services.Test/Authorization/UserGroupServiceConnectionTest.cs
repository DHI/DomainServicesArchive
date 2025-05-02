namespace DHI.Services.Test.Authorization
{
    using DHI.Services.Authorization;
    using Xunit;

    public class UserGroupServiceConnectionTest
    {
        [Fact]
        public void CreateConnectionTypeIsOk()
        {
            var connectionType = UserGroupServiceConnection.CreateConnectionType<UserGroupServiceConnection>();

            Assert.Equal("UserGroupServiceConnection", connectionType.Id);
            Assert.Equal(typeof(UserGroupServiceConnection), connectionType.Type);
            Assert.Contains(typeof(UserGroupRepository), connectionType.ProviderTypes[0].Options);
        }
    }
}
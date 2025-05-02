namespace DHI.Services.Test.Authentication
{
    using System;
    using AutoFixture.Xunit2;
    using DHI.Services.Authentication;
    using Xunit;

    public class RefreshTokenTest
    {
        [Fact]
        public void CreateWithNullAccountIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshToken("someToken", null, DateTime.Now.AddDays(5)));
        }

        [Fact]
        public void CreateWithEmptyAccountIdThrows()
        {
            Assert.Throws<ArgumentException>(() => new RefreshToken("someToken", "", DateTime.Now.AddDays(5)));
        }

        [Fact]
        public void CreateWithClientIpAndNullAccountIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshToken("someToken", null, DateTime.Now.AddDays(5), "127.0.0.1"));
        }

        [Fact]
        public void CreateWithNullTokenThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshToken(null, "john.doe", DateTime.Now.AddDays(5)));
        }

        [Theory, AutoData]
        public void CreateIsOk(string token, string accountId)
        {
            var refreshToken = new RefreshToken(token, accountId, DateTime.Now.AddDays(5));
            Assert.Equal(accountId, refreshToken.Id);
        }

        [Theory, AutoData]
        public void CreateWithClientIpIsOk(string token, string accountId, string clientIp)
        {
            var refreshToken = new RefreshToken(token, accountId, DateTime.Now.AddDays(5), clientIp);
            Assert.Equal($"{accountId}-{clientIp}", refreshToken.Id);
        }

        [Theory, AutoData]
        public void CreateWithEmptyClientIpIsIgnored(string token, string accountId)
        {
            var refreshToken = new RefreshToken(token, accountId, DateTime.Now.AddDays(5), "");
            Assert.Equal(accountId, refreshToken.Id);
        }

        [Fact]
        public void IsExpiredIsOk()
        {
            Assert.False(new RefreshToken("someToken", "john.doe", DateTime.Now.AddDays(5)).IsExpired);
            Assert.True(new RefreshToken("someToken", "john.doe", DateTime.Now.AddDays(-5)).IsExpired);
        }
    }
}
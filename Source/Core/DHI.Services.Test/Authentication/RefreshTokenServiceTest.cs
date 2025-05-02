namespace DHI.Services.Test.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.Authentication;
    using Xunit;

    public class RefreshTokenServiceTest
    {
        private const int RepeatCount = 10;

        [Theory]
        [AutoRefreshTokenData]
        public void GetByNonExistingTokenThrows(RefreshTokenService refreshTokenService, string token)
        {
            Assert.Throws<KeyNotFoundException>(() => refreshTokenService.GetByToken(token));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void UpdateNonExistingThrows(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            Assert.Throws<KeyNotFoundException>(() => refreshTokenService.Update(token));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void RemoveNonExistingThrows(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            Assert.Throws<KeyNotFoundException>(() => refreshTokenService.Remove(token.Id));
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshTokenService(null));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeIllegalRefreshTokenThrows(RefreshTokenService refreshTokenService, RefreshToken token, TimeSpan expirationTimeSpan)
        {
            refreshTokenService.Add(token);
            Assert.Throws<ArgumentException>(() => refreshTokenService.ExchangeRefreshToken("illegalToken", token.AccountId, expirationTimeSpan));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeRefreshTokenWithWrongAccountThrows(RefreshTokenService refreshTokenService, RefreshToken token, TimeSpan expirationTimeSpan)
        {
            refreshTokenService.Add(token);
            Assert.Throws<ArgumentException>(() => refreshTokenService.ExchangeRefreshToken(token.Id, "wrongAccount", expirationTimeSpan));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeRefreshTokenWithNullAccountIdThrows(RefreshTokenService refreshTokenService, RefreshToken token, TimeSpan expirationTimeSpan)
        {
            refreshTokenService.Add(token);
            Assert.Throws<ArgumentNullException>(() => refreshTokenService.ExchangeRefreshToken(token.Id, null, expirationTimeSpan));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeRefreshTokenWithEmptyAccountIdThrows(RefreshTokenService refreshTokenService, RefreshToken token, TimeSpan expirationTimeSpan)
        {
            refreshTokenService.Add(token);
            Assert.Throws<ArgumentException>(() => refreshTokenService.ExchangeRefreshToken(token.Id, "", expirationTimeSpan));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeExpiredRefreshTokenThrows(RefreshTokenService refreshTokenService, TimeSpan expirationTimeSpan)
        {
            var token = new RefreshToken("someToken", "someAccount", DateTime.UtcNow.AddDays(-1));
            refreshTokenService.Add(token);
            Assert.Throws<ArgumentException>(() => refreshTokenService.ExchangeRefreshToken(token.Id, token.AccountId, expirationTimeSpan));
        }

        [Theory, AutoRefreshTokenData(RepeatCount)]
        public void GetAllIsOk(RefreshTokenService refreshTokenService)
        {
            Assert.Equal(RepeatCount, refreshTokenService.GetAll().Count());
        }

        [Theory, AutoRefreshTokenData(RepeatCount)]
        public void GetIdsIsOk(RefreshTokenService refreshTokenService)
        {
            Assert.Equal(RepeatCount, refreshTokenService.GetIds().Count());
        }

        [Theory, AutoRefreshTokenData]
        public void AddAndGetIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            refreshTokenService.TryGet(token.Id, out var myEntity);
            Assert.Equal(token.Id, myEntity.Id);
        }

        [Theory, AutoRefreshTokenData]
        public void AddAndGetByAccountIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            Assert.Single(refreshTokenService.GetByAccount(token.AccountId));
        }

        [Theory, AutoRefreshTokenData]
        public void AddAndGetByTokenIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            Assert.Equal(token.AccountId, refreshTokenService.GetByToken(token.Token).AccountId);
        }

        [Theory, AutoRefreshTokenData]
        public void ContainsAccountIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            Assert.False(refreshTokenService.ContainsAccount(token.AccountId));
            refreshTokenService.Add(token);
            Assert.True(refreshTokenService.ContainsAccount(token.AccountId));
        }

        [Theory, AutoRefreshTokenData(RepeatCount)]
        public void CountIsOk(RefreshTokenService refreshTokenService)
        {
            Assert.Equal(RepeatCount, refreshTokenService.Count());
        }

        [Theory, AutoRefreshTokenData(RepeatCount)]
        public void ExistsIsOk(RefreshTokenService refreshTokenService)
        {
            var token = refreshTokenService.GetAll().ToArray()[0];
            Assert.True(refreshTokenService.Exists(token.Id));
        }

        [Theory, AutoRefreshTokenData(RepeatCount)]
        public void DoesNotExistsIsOk(RefreshTokenService refreshTokenService)
        {
            Assert.False(refreshTokenService.Exists("NonExistingConnection"));
        }

        [Theory, AutoRefreshTokenData]
        public void EventsAreRaisedOnAdd(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            var raisedEvents = new List<string>();
            refreshTokenService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            refreshTokenService.Added += (s, e) => { raisedEvents.Add("Added"); };

            refreshTokenService.Add(token);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoRefreshTokenData]
        public void RemoveIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            refreshTokenService.Remove(token.Id);

            Assert.False(refreshTokenService.Exists(token.Id));
            Assert.Equal(0, refreshTokenService.Count());
        }

        [Theory, AutoRefreshTokenData]
        public void RemoveByAccountIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            var anotherToken = new RefreshToken("someToken", token.AccountId, DateTime.Now + TimeSpan.FromDays(1));
            refreshTokenService.Add(anotherToken);
            Assert.Equal(2, refreshTokenService.Count());
            var removedCount = refreshTokenService.RemoveByAccount(token.AccountId);

            Assert.False(refreshTokenService.Exists(token.Id));
            Assert.False(refreshTokenService.Exists(anotherToken.Id));
            Assert.Equal(2, removedCount);
            Assert.Equal(0, refreshTokenService.Count());
        }

        [Theory, AutoRefreshTokenData]
        public void EventsAreRaisedOnRemove(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            var raisedEvents = new List<string>();
            refreshTokenService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            refreshTokenService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            refreshTokenService.Add(token);

            refreshTokenService.Remove(token.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoRefreshTokenData]
        public void UpdateIsOk(RefreshTokenService refreshTokenService, RefreshToken refreshToken)
        {
            refreshTokenService.Add(refreshToken);

            var expiration = DateTime.Now;
            var tokenUpdated = new RefreshToken(refreshToken.Token, refreshToken.AccountId, expiration, refreshToken.ClientIp);
            refreshTokenService.Update(tokenUpdated);

            refreshTokenService.TryGet(refreshToken.Id, out var myEntity);
            Assert.Equal(expiration, myEntity.Expiration);
        }

        [Theory, AutoRefreshTokenData]
        public void AddOrUpdateIsOk(RefreshTokenService refreshTokenService, RefreshToken refreshToken)
        {
            var raisedEvents = new List<string>();
            refreshTokenService.Added += (s, e) => { raisedEvents.Add("Added"); };
            refreshTokenService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            refreshTokenService.AddOrUpdate(refreshToken);
            var expiration = DateTime.Now;
            var updated = new RefreshToken(refreshToken.Token, refreshToken.AccountId, expiration, refreshToken.ClientIp);
            refreshTokenService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            refreshTokenService.TryGet(refreshToken.Id, out var myEntity);
            Assert.Equal(expiration, myEntity.Expiration);
        }

        [Theory, AutoRefreshTokenData]
        public void TryAddIsOk(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            Assert.True(refreshTokenService.TryAdd(token));
            refreshTokenService.TryGet(token.Id, out var myEntity);
            Assert.Equal(token.Id, myEntity.Id);
        }

        [Theory, AutoRefreshTokenData]
        public void TryAddExistingReturnsFalse(RefreshTokenService refreshTokenService, RefreshToken token)
        {
            refreshTokenService.Add(token);
            Assert.False(refreshTokenService.TryAdd(token));
        }

        [Theory, AutoRefreshTokenData]
        public void TryUpdateIsOk(RefreshTokenService refreshTokenService, RefreshToken refreshToken)
        {
            refreshTokenService.Add(refreshToken);

            var expiration = DateTime.Now;
            var tokenUpdated = new RefreshToken(refreshToken.Token, refreshToken.AccountId, expiration, refreshToken.ClientIp);

            Assert.True(refreshTokenService.TryUpdate(tokenUpdated));
            refreshTokenService.TryGet(refreshToken.Id, out var myEntity);
            Assert.Equal(expiration, myEntity.Expiration);
        }

        [Theory, AutoRefreshTokenData]
        public void TryUpdateNonExistingReturnsFalse(RefreshTokenService refreshTokenService, RefreshToken refreshToken)
        {
            Assert.False(refreshTokenService.TryUpdate(refreshToken));
        }

        [Theory, AutoRefreshTokenData]
        public void EventsAreRaisedOnUpdate(RefreshTokenService refreshTokenService, RefreshToken refreshToken)
        {
            var raisedEvents = new List<string>();
            refreshTokenService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            refreshTokenService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            refreshTokenService.Add(refreshToken);

            var expiration = DateTime.Now;
            var tokenUpdated = new RefreshToken(refreshToken.Token, refreshToken.AccountId, expiration, refreshToken.ClientIp);
            refreshTokenService.Update(tokenUpdated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory]
        [AutoRefreshTokenData]
        public void CreateRefreshTokenIsOk(RefreshTokenService refreshTokenService)
        {
            var token = refreshTokenService.CreateRefreshToken("myAccount", TimeSpan.FromHours(1));

            Assert.False(token.IsExpired);
            Assert.True(refreshTokenService.Exists(token.Id));
        }

        [Theory]
        [AutoRefreshTokenData]
        public void ExchangeRefreshTokenIsOk(RefreshTokenService refreshTokenService)
        {
            var refreshToken = new RefreshToken("someToken", "someAccount", DateTime.UtcNow.AddHours(1));
            refreshTokenService.Add(refreshToken);
            var newRefreshToken = refreshTokenService.ExchangeRefreshToken(refreshToken.Token, refreshToken.AccountId, TimeSpan.FromHours(1));

            Assert.False(newRefreshToken.IsExpired);
            refreshTokenService.TryGet(newRefreshToken.Id, out var myEntity);
            Assert.Equal(newRefreshToken.Token, myEntity.Token);
        }
    }
}
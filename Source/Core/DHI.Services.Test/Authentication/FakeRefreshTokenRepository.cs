namespace DHI.Services.Test.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.Authentication;

    internal class FakeRefreshTokenRepository : FakeRepository<RefreshToken, string>, IRefreshTokenRepository
    {
        public FakeRefreshTokenRepository()
        {
        }

        public FakeRefreshTokenRepository(IEnumerable<RefreshToken> tokens)
            : base(tokens)
        {
        }

        public IEnumerable<RefreshToken> GetByAccount(string accountId)
        {
            return Get(token => token.AccountId == accountId).ToList();
        }

        public Maybe<RefreshToken> GetByToken(string token)
        {
            var tokens = Get(t => t.Token == token).ToArray();
            return tokens.Length == 1 ? tokens.Single().ToMaybe() : Maybe.Empty<RefreshToken>();
        }
    }
}
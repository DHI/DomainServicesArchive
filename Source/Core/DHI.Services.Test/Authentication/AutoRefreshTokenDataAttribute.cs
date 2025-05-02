namespace DHI.Services.Test.Authentication
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;
    using DHI.Services.Authentication;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoRefreshTokenDataAttribute : AutoDataAttribute
    {
        public AutoRefreshTokenDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var connectionList = fixture.CreateMany<RefreshToken>().ToList();
                    fixture.Register<IRefreshTokenRepository>(() => new FakeRefreshTokenRepository(connectionList));
                }
                else
                {
                    fixture.Register<IRefreshTokenRepository>(() => new FakeRefreshTokenRepository());
                }

                return fixture;
            })
        {
        }
    }
}
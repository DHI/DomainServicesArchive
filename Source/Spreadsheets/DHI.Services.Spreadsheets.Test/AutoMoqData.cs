namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
                fixture.Inject(Maybe.Empty<object>());
                fixture.Inject(Maybe.Empty<object[,]>());
                return fixture;
            })
        {
        }
    }
}
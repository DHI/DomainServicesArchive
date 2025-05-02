namespace DHI.Services.TimeSteps.Test
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }))
        {
        }
    }
}
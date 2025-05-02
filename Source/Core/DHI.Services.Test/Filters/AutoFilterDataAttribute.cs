namespace DHI.Services.Test.Filters
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using DHI.Services.Filters;

    [AttributeUsage(AttributeTargets.Method)]
    public class AutoFilterDataAttribute : AutoDataAttribute
    {
        public AutoFilterDataAttribute() : base(() =>
        {
            var fixture = new Fixture();
            fixture.Customize<Filter>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
            fixture.Register(() => new QueryCondition("foo", "bar"));
            return fixture;
        })
        {
        }
    }
}
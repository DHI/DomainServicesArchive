namespace DHI.Services.Physics.Test
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;
    using DHI.Physics;
    using Unit = Unit;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoUnitDataAttribute : AutoDataAttribute
    {
        public AutoUnitDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var unitList = new List<Unit>();
                    for (var i = 0; i < repeatCount; i++)
                    {
                        unitList.Add(new Unit(fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), 1, Dimension.Length));
                    }

                    fixture.Register<IUnitRepository>(() => new FakeUnitRepository(unitList));
                }
                else
                {
                    fixture.Register<IUnitRepository>(() => new FakeUnitRepository());
                }

                return fixture;
            })
        {
        }
    }
}
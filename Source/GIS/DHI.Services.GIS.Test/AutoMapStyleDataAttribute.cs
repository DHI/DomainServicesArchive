namespace DHI.Services.GIS.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using Maps;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMapStyleDataAttribute : AutoDataAttribute
    {
        public AutoMapStyleDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var mapStyleList = fixture.CreateMany<MapStyle>().ToList();
                    fixture.Register<IMapStyleRepository>(() => new FakeMapStyleRepository(mapStyleList));
                }
                else
                {
                    fixture.Register<IMapStyleRepository>(() => new FakeMapStyleRepository());
                }

                return fixture;
            })
        { }
    }
}
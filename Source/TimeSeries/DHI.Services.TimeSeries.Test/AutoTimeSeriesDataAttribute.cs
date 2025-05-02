namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using DHI.Services.TimeSeries;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoTimeSeriesDataAttribute : AutoDataAttribute
    {
        public AutoTimeSeriesDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customizations.Add(new TypeRelay(typeof(TimeSeriesDataType), TimeSeriesDataType.Instantaneous.GetType()));
                fixture.Customize<TimeSeries<Guid, float>>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                var dateTimes = fixture.CreateMany<DateTime>().OrderBy(d => d).ToList();
                var values = fixture.CreateMany<float?>().ToList();
                fixture.Register<ITimeSeriesData<float>>(() => new TimeSeriesData<float>(dateTimes, values));
                var timeSeriesList = fixture.CreateMany<TimeSeries<Guid, float>>().ToList();
                fixture.Register<ICoreTimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register<ITimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register<IDiscreteTimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register<IGroupedDiscreteTimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register<IUpdatableTimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register<IGroupedUpdatableTimeSeriesRepository<Guid, float>>(() => new FakeTimeSeriesRepository(timeSeriesList));
                fixture.Register(() => new InMemoryTimeSeriesRepository<Guid, float>(timeSeriesList));
                return fixture;
            })
        {
        }
    }
}
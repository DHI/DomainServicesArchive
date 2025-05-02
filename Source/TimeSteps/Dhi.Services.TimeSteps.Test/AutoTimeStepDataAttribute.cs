namespace DHI.Services.TimeSteps.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using TimeSteps;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoTimeStepDataAttribute : AutoDataAttribute
    {
        public AutoTimeStepDataAttribute()
            : base(() =>
            {
                var data = new Dictionary<TimeStep<string>, double[]>();
                var fixture = new Fixture();
                var itemIds = fixture.CreateMany<string>().ToArray();
                var dateTimes = fixture.CreateMany<DateTime>().ToArray();
                foreach (var dateTime in dateTimes)
                {
                    foreach (var itemId in itemIds)
                    {
                        data.Add(new TimeStep<string>(itemId, dateTime), fixture.CreateMany<double>().ToArray());
                    }
                }

                fixture.Register<ITimeStepServer<string, double[]>>(() => new FakeTimeStepServer<string, double[]>(data));
                return fixture;
            })
        {
        }
    }
}
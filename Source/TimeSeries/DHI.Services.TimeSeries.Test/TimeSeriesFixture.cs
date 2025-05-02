namespace DHI.Services.TimeSeries.Test
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using DHI.Services.TimeSeries;

    public class TimeSeriesFixture : IDisposable
    {
        public TimeSeriesFixture()
        {
            Fixture = new Fixture();
            Fixture.Customizations.Add(new TypeRelay(typeof (TimeSeriesDataType), TimeSeriesDataType.Instantaneous.GetType()));
        }

        public Fixture Fixture { get; }

        public void Dispose()
        {
        }
    }
}
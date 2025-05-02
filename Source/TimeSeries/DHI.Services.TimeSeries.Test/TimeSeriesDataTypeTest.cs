namespace DHI.Services.TimeSeries.Test
{
    using System;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class TimeSeriesDataTypeTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void InterpolationWithIllegalIntervalThrows()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(-1), _fixture.Create<double>());

            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Instantaneous.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Accumulated.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.StepAccumulated.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepBackward.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepForward.Interpolate(p0, p1, p0.DateTime));
        }

        [Fact]
        public void InterpolationWithIllegalDateTimeThrows()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(1), _fixture.Create<double>());

            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Instantaneous.Interpolate(p0, p1, DateTime.MaxValue));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Accumulated.Interpolate(p0, p1, DateTime.MaxValue));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.StepAccumulated.Interpolate(p0, p1, DateTime.MaxValue));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepBackward.Interpolate(p0, p1, DateTime.MaxValue));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepForward.Interpolate(p0, p1, DateTime.MaxValue));   
        }

        [Fact]
        public void InterpolationWithNullValuesThrows()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(1), null);

            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Instantaneous.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.Accumulated.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.StepAccumulated.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepBackward.Interpolate(p0, p1, p0.DateTime));
            Assert.Throws<ArgumentException>(() => TimeSeriesDataType.MeanStepForward.Interpolate(p0, p1, p0.DateTime));
        }

        [Fact]
        public void InstantaneousInterpolationIsOk()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(2), _fixture.Create<double>());
            var dateTime = p0.DateTime.AddDays(1);
            var expected = (p0.Value + p1.Value)/2;

            Assert.Equal(expected, TimeSeriesDataType.Instantaneous.Interpolate(p0, p1, dateTime).Value);
        }

        [Fact]
        public void MeanStepBackwardInterpolationIsOk()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(2), _fixture.Create<double>());
            var dateTime = p0.DateTime.AddDays(1);

            Assert.Equal(p1.Value, TimeSeriesDataType.MeanStepBackward.Interpolate(p0, p1, dateTime).Value);
        }

        [Fact]
        public void MeanStepForwardInterpolationIsOk()
        {
            var p0 = _fixture.Create<DataPoint>();
            var p1 = new DataPoint(p0.DateTime.AddDays(2), _fixture.Create<double>());
            var dateTime = p0.DateTime.AddDays(1);

            Assert.Equal(p0.Value, TimeSeriesDataType.MeanStepForward.Interpolate(p0, p1, dateTime).Value);
        }
    }
}
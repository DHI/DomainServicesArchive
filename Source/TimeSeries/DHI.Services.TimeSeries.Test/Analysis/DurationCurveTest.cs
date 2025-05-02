namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class DurationCurveTest
    {
        private readonly Fixture _fixture = new Fixture();

        private static TimeSeriesData TimeSeriesDataDouble => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1),
                new DateTime(2000, 1, 2),
                new DateTime(2000, 1, 3),
                new DateTime(2000, 1, 4),
                new DateTime(2000, 1, 5),
                new DateTime(2000, 1, 6),
                new DateTime(2000, 1, 7),
                new DateTime(2000, 1, 8),
                new DateTime(2000, 1, 9),
                new DateTime(2000, 1, 10),
                new DateTime(2000, 1, 11)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            });

        [Fact]
        public void DurationCurveOfEmptyValuesThrows()
        {
            Assert.Throws<Exception>(() => new TimeSeriesData().DurationCurve(1));
        }

        [Fact]
        public void DurationCurveOfConstantValuesThrows()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.Values.Add(1.1);
            timeSeriesData.Values.Add(1.1);
            Assert.Throws<Exception>(() => timeSeriesData.DurationCurve(1));
        }

        [Fact]
        public void DurationCurveOfTooManyNullValuesThrows()
        {
            var dateTimes = _fixture.CreateMany<DateTime>().OrderBy(d => d).ToList();
            var values = new List<double?> { null, 3, 4 };
            var timeSeriesData = new TimeSeriesData(dateTimes, values);
            Assert.Throws<Exception>(() => timeSeriesData.DurationCurve(1, 10, 3));
        }

        [Fact]
        public void DurationCurveOfTooFewValuesThrows()
        {
            var dateTimes = _fixture.CreateMany<DateTime>().OrderBy(d => d).ToList();
            var values = _fixture.CreateMany<double?>().ToList();
            var timeSeriesData = new TimeSeriesData(dateTimes, values);
            Assert.Throws<Exception>(() => timeSeriesData.DurationCurve(1));
        }

        [Fact]
        public void DurationCurveIsOk()
        {
            var durationCurve = TimeSeriesDataDouble.DurationCurve(60, 90, 5);
            var criticalValue = durationCurve.Where(kvp => Math.Abs(kvp.Value) < 1E-10).Select(kvp => kvp.Key).Min();

            Assert.Equal(89, durationCurve.Count);
            Assert.DoesNotContain(durationCurve.Values, value => value > 1d);
            Assert.DoesNotContain(durationCurve.Values, value => value < 0d);
            Assert.InRange(criticalValue, 9.06, 9.07);
        }

        [Fact]
        public void DurationCurveExtremeIsOk()
        {
            var durationCurve = TimeSeriesDataDouble.DurationCurve(); 
            Assert.Equal(TimeSeriesDataDouble.Values.Max(), durationCurve.Values.Max());
            Assert.Equal(TimeSeriesDataDouble.Values.Min(), durationCurve.Values.Min());
        }
    }
}
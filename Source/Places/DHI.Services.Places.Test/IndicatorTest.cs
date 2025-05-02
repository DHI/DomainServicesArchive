namespace DHI.Services.Places.Test
{
    using System;
    using System.Linq;
    using AutoFixture.Xunit2;
    using GIS.Maps;
    using TimeSeries;
    using Xunit;

    public class IndicatorTest
    {
        [Theory, AutoData]
        public void CreateWithNullOrEmptyStyleCodeThrows(DataSource dataSource)
        {
            Assert.Throws<ArgumentNullException>(() => new Indicator(dataSource, null));
            Assert.Throws<ArgumentException>(() => new Indicator(dataSource, ""));
        }

        [Theory, AutoData]
        public void CreateWithIllegalStyleCodeThrows(DataSource dataSource)
        {
            var exception = Assert.Throws<Exception>(() => new Indicator(dataSource, "IllegalStyleCode"));
            Assert.Contains("does not define a valid palette", exception.Message);
        }

        [Fact]
        public void CreateAsTimeSeriesWithMissingArgumentsThrows()
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "connectionId", "entityId");
            Assert.Throws<ArgumentNullException>(() => new Indicator(dataSource, "0:Red|10:Green"));
        }

        [Fact]
        public void CreationAsScalarIsOk()
        {
            var dataSource = new DataSource(DataSourceType.Scalar, "connectionId", "scalarId");
            var indicator = new Indicator(dataSource, "0:Red|10:Green");
            Assert.Equal(2, indicator.GetPalette().ThresholdValues.Count());
            Assert.Null(indicator.AggregationType);
            Assert.Null(indicator.TimeInterval);
        }

        [Fact]
        public void CreationAsTimeSeriesIsOk()
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "connectionId", "timeSeriesId");
            var indicator = new Indicator(dataSource, "0:Red|10:Green", TimeInterval.CreateRelativeToNow(-1, 0), AggregationType.Sum);
            Assert.Equal(2, indicator.GetPalette().ThresholdValues.Count());
            Assert.Equal(AggregationType.Sum, indicator.AggregationType);
            Assert.Equal(TimeIntervalType.RelativeToNow, indicator.TimeInterval.Type);
        }

        [Fact]
        public void CreationWithPaletteAsUpperThresholdValuesIsOk()
        {
            var dataSource = new DataSource(DataSourceType.TimeSeries, "connectionId", "timeSeriesId");
            var indicator = new Indicator(dataSource, "0:Red|10:Green", TimeInterval.CreateRelativeToNow(-1, 0), AggregationType.Sum, null, PaletteType.UpperThresholdValues);
            Assert.Equal(PaletteType.UpperThresholdValues.ToString(), indicator.PaletteType.ToString());
        }
    }
}
namespace DHI.Services.TimeSeries.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using DHI.Services.TimeSeries;
    using Logging;
    using Xunit;

    public class CoreTimeSeriesServiceTest : IClassFixture<TimeSeriesFixture>
    {
        private readonly Fixture _fixture;

        public CoreTimeSeriesServiceTest(TimeSeriesFixture timeSeriesFixture)
        {
            _fixture = timeSeriesFixture.Fixture;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new CoreTimeSeriesService<Guid, float>(null));
        }

        [Theory, AutoTimeSeriesData]
        public void GetNonExistingThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.False(timeSeriesService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForNonExistingReturnsEmpty(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Empty(timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue).Values);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(Guid.NewGuid(), DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueToWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValue(Guid.NewGuid(), AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetEnsembleAggregatedValuesWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(Guid.NewGuid(), AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetEnsembleAggregatedValuesFromWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(Guid.NewGuid(), AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetEnsembleAggregatedValuesToWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(Guid.NewGuid(), AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetEnsembleAggregatedValuesThrowsIfNotOverridden(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var e = Assert.Throws<NotSupportedException>(() => timeSeriesService.GetEnsembleAggregatedValues(timeSeries.Id, AggregationType.Minimum));
            Assert.Contains("This repository does not support ensemble time series.", e.Message);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesWithIllegalIntervalForMultipleThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesToWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetMultipleEnsembleAggregatedValuesWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetMultipleEnsembleAggregatedValuesFromWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, DateTime.MaxValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetMultipleEnsembleAggregatedValuesToWithIllegalIntervalThrows(CoreTimeSeriesService<Guid, float> timeSeriesService)
        {
            Assert.Throws<ArgumentException>(() => timeSeriesService.GetEnsembleAggregatedValues(new[] { Guid.NewGuid(), Guid.NewGuid() }, AggregationType.Minimum, to: DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetMultipleEnsembleAggregatedValuesThrowsIfNotOverridden(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var e = Assert.Throws<NotSupportedException>(() => timeSeriesService.GetEnsembleAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum));
            Assert.Contains("This repository does not support ensemble time series.", e.Message);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueWithNonExistingDateReturnsNull(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            Assert.Null(timeSeriesService.GetValue(timeSeries.Id, DateTime.MinValue));
        }

        [Theory, AutoTimeSeriesData]
        public void GetIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            timeSeriesService.TryGet(timeSeries.Id, out var ts);
            Assert.Equal(timeSeries.Id, ts.Id);
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray();

            timeSeriesService.TryGet(timeSeries.Select(t => t.Id), out var ts);
            var myTimeSeries = ts.ToArray();

            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Count());
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetManyWithNonExistingIdIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray();
            var ids = timeSeries.Select(e => e.Id).ToList();
            var nonExistingId = Guid.NewGuid();
            ids.Add(nonExistingId);
            timeSeriesService.TryGet(ids, out var ts);
            var myTimeSeries = ts.Where(t => t != null).ToArray();
            Assert.Equal(_fixture.RepeatCount, myTimeSeries.Length);
            Assert.Contains(timeSeries[0].Id, myTimeSeries.Select(t => t.Id));
            Assert.DoesNotContain(nonExistingId, myTimeSeries.Select(t => t.Id));
        }

        [Theory, AutoTimeSeriesData]
        public void GetValueIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var lastDateTime = (DateTime)discreteTimeSeriesService.GetLastDateTime(timeSeries.Id);
            var value = timeSeriesService.GetValue(timeSeries.Id, lastDateTime);
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var firstDateTime = (DateTime)discreteTimeSeriesService.GetFirstDateTime(timeSeries.Id);
            var lastDateTime = (DateTime)discreteTimeSeriesService.GetLastDateTime(timeSeries.Id);
            var data = timeSeriesService.GetValues(timeSeries.Id, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, data.Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetValuesForMultipleIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var firstDateTime = (DateTime)discreteTimeSeriesService.GetFirstDateTime(timeSeries1.Id);
            var lastDateTime = (DateTime)discreteTimeSeriesService.GetLastDateTime(timeSeries1.Id);
            var values = timeSeriesService.GetValues(new[] { timeSeries1.Id, timeSeries2.Id }, firstDateTime.AddMilliseconds(1), lastDateTime.AddMilliseconds(-1));
            Assert.Equal(_fixture.RepeatCount - 2, values[timeSeries1.Id].Values.Count);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, DateTime.MinValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueToIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, to: DateTime.MaxValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValueFromToIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries = discreteTimeSeriesService.GetAll().ToArray()[0];
            var maybe = timeSeriesService.GetAggregatedValue(timeSeries.Id, AggregationType.Minimum, DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(timeSeries.Data.Values.Min(), maybe.Value);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, DateTime.MinValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesToIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, to: DateTime.MaxValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Theory, AutoTimeSeriesData]
        public void GetAggregatedValuesFromToIsOk(CoreTimeSeriesService<Guid, float> timeSeriesService, DiscreteTimeSeriesService<Guid, float> discreteTimeSeriesService)
        {
            var timeSeries1 = discreteTimeSeriesService.GetAll().ToArray()[0];
            var timeSeries2 = discreteTimeSeriesService.GetAll().ToArray()[1];
            var minValues = timeSeriesService.GetAggregatedValues(new[] { timeSeries1.Id, timeSeries2.Id }, AggregationType.Minimum, DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(timeSeries1.Data.Values.Min(), minValues[timeSeries1.Id]);
            Assert.Equal(timeSeries2.Data.Values.Min(), minValues[timeSeries2.Id]);
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = CoreTimeSeriesService<string, double>.GetRepositoryTypes();

            Assert.Contains(typeof(DHI.Services.TimeSeries.Daylight.TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = CoreTimeSeriesService<string, double>.GetRepositoryTypes(path);

            Assert.Contains(typeof(DHI.Services.TimeSeries.Daylight.TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = CoreTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(DHI.Services.TimeSeries.Daylight.TimeSeriesRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = CoreTimeSeriesService<string, double>.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}
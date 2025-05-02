namespace DHI.Services.Models.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ModelDataReaderTest
    {
        private readonly ModelDataReader<FakeModelDataReader> _modelDataReader;

        public ModelDataReaderTest()
        {
            _modelDataReader = new ModelDataReader<FakeModelDataReader>("fakeModelDataReader", "Fake model data reader");
        }

        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<IModelDataReader>(_modelDataReader);
        }

        [Fact]
        public void TypeNameIsOk()
        {
            Assert.Equal(typeof(FakeModelDataReader).FullName, _modelDataReader.TypeName);
        }

        [Fact]
        public void GetNullOrEmptyParameterThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() => _modelDataReader.GetParameterValue<string>(null));
            Assert.Contains("cannot be null", e.Message);
            var e2 = Assert.Throws<ArgumentException>(() => _modelDataReader.GetParameterValue<string>(""));
            Assert.Contains("is empty", e2.Message);
        }

        [Fact]
        public void GetNonExistingParameterThrows()
        {
            var e = Assert.Throws<KeyNotFoundException>(() => _modelDataReader.GetParameterValue<string>("NonExistingParameter"));
            Assert.Contains("was not found", e.Message);
        }

        [Fact]
        public async Task GetNullOrEmptyInputTimeSeriesThrows()
        {
            var e = await Assert.ThrowsAsync<ArgumentNullException>(() => _modelDataReader.GetInputTimeSeriesValues(null!));
            Assert.Contains("cannot be null", e.Message);
            var e2 = await Assert.ThrowsAsync<ArgumentException>(() => _modelDataReader.GetInputTimeSeriesValues(""));
            Assert.Contains("is empty", e2.Message);
        }

        [Fact]
        public async Task GetNonExistingInputTimeSeriesThrows()
        {
            var e = await Assert.ThrowsAsync<KeyNotFoundException>(() => _modelDataReader.GetInputTimeSeriesValues("NonExistingTimeSeries"));
            Assert.Contains("was not found", e.Message);
        }

        [Fact]
        public async Task GetNullOrEmptyOutputTimeSeriesThrows()
        {
            var e = await Assert.ThrowsAsync<ArgumentNullException>(() => _modelDataReader.GetOutputTimeSeriesValues(Guid.NewGuid(), null));
            Assert.Contains("cannot be null", e.Message);
            var e2 = await Assert.ThrowsAsync<ArgumentException>(() => _modelDataReader.GetOutputTimeSeriesValues(Guid.NewGuid(), ""));
            Assert.Contains("is empty", e2.Message);
        }

        [Fact]
        public async Task GetNonExistingOutputTimeSeriesThrows()
        {
            var e = await Assert.ThrowsAsync<KeyNotFoundException>(() => _modelDataReader.GetOutputTimeSeriesValues(Guid.NewGuid(), "NonExistingTimeSeries"));
            Assert.Contains("was not found", e.Message);
        }

        [Fact]
        public void GetParameterListIsOk()
        {
            var parameters = _modelDataReader.GetParameterList();

            Assert.True(parameters.Any());
            Assert.Contains("foo", parameters);
            Assert.Equal(typeof(int), parameters["foo"]);
            Assert.Contains("bar", parameters);
            Assert.Equal(typeof(bool), parameters["bar"]);
        }

        [Fact]
        public void GetParameterValueIsOk()
        {
            Assert.Equal(1, _modelDataReader.GetParameterValue<int>("foo"));
            Assert.False(_modelDataReader.GetParameterValue<bool>("bar"));
        }

        [Fact]
        public void GetInputTimeSeriesListIsOk()
        {
            var inputTimeSeries = _modelDataReader.GetInputTimeSeriesList();

            Assert.Contains("ts1-in", inputTimeSeries);
        }

        [Fact]
        public async Task SetAndGetInputTimeSeriesValuesIsOk()
        {
            var data = await _modelDataReader.GetInputTimeSeriesValues("ts1-in");

            Assert.True(data.DateTimes.Any());
            Assert.True(data.Values.Any());
        }

        [Fact]
        public void GetOutputTimeSeriesListIsOk()
        {
            var outputTimeSeries = _modelDataReader.GetOutputTimeSeriesList();

            Assert.Contains("ts1-out", outputTimeSeries);
        }

        [Fact]
        public async Task GetOutputTimeSeriesValuesIsOk()
        {
            var data = await _modelDataReader.GetOutputTimeSeriesValues(Guid.NewGuid(), "ts1-out");

            Assert.True(data.Values.Any());
            Assert.Equal(123, data.Values.Max());
        }

        [Fact(Skip = "To be run manually.")]
        public void Test1()
        {
            var service = new ModelDataReaderService(new ModelDataReaderRepository("C:\\Temp\\models.json"));
            var reader = new ModelDataReader<FakeModelDataReader>("fakeReader", "Fake model data reader");
            service.TryAdd(reader);
            var modelDataReader = service.Get(reader.Id) as ModelDataReader<FakeModelDataReader>;
            Assert.NotNull(modelDataReader);
            var parameters = modelDataReader.GetParameterList();
            var foo = modelDataReader.GetParameterValue<int>("foo");
            var bar = modelDataReader.GetParameterValue<bool>("bar");
            var timeSeriesList = modelDataReader.GetInputTimeSeriesList();
            var timeSeries = modelDataReader.GetInputTimeSeriesValues("ts1-in");
        }
    }
}
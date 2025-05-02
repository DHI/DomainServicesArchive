namespace DHI.Services.TimeSeries.Test.Repositories.Json
{
    using System;
    using System.Linq;
    using DHI.Services.TimeSeries.Json;
    using Xunit;

    public class TimeSeriesRepositoryTest
    {
        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<ITimeSeriesRepository<string, double>>(new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json"));
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesRepository(null));
        }

        [Fact]
        public void CreateWithIllegalFilePathThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository("NonExistingJson"));
        }

        [Fact]
        public void GetValuesWithInvalidPathThrows()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            Assert.Throws<ArgumentException>(() => repository.GetValues("InvalidPath;whatever=something"));
        }

        [Fact]
        public void GetIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            var timeSeries = repository.Get(@"..\..\..\Data\Json\Melbourne Gauge Data.json").Value;
            Assert.Equal(@"..\..\..\Data\Json\Melbourne Gauge Data.json", timeSeries.Id);
        }

        [Fact]
        public void GetAllValuesIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            var timeSeriesData = repository.GetValues(@"..\..\..\Data\Json\Melbourne Gauge Data.json").Value;
            
            Assert.Equal(2649, timeSeriesData.Values.Sum());
            Assert.Equal(22, timeSeriesData.Values.Count);
        }

        [Fact]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
            Assert.True(repository.GetLastDateTime(id).Value > repository.GetFirstDateTime(id).Value);
        }

        [Fact]
        public void GetFirstValueAfterIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
            var firstDateTime = repository.GetFirstDateTime(id).Value;
            var value = repository.GetFirstValueAfter(id, firstDateTime).Value;
            Assert.True(value.DateTime > firstDateTime);
        }

        [Fact]
        public void GetLastValueBeforeIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
            var lastDateTime = repository.GetLastDateTime(id).Value;
            var value = repository.GetLastValueBefore(id, lastDateTime).Value;
            Assert.True(value.DateTime < lastDateTime);
        }

        [Fact]
        public void GetValueIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
            var lastDateTime = repository.GetLastDateTime(id).Value;
            var value = repository.GetValue(id, lastDateTime).Value;
            Assert.Equal(value.DateTime, lastDateTime);
        }

        [Fact]
        public void GetDateTimesIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
            Assert.Equal(22, repository.GetDateTimes(id).Value.Count);
        }

        [Fact]
        public void GetValues1IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Melbourne Gauge Data Recipe - without comments.json");
            const string id = @"..\..\..\Data\Json\Melbourne Gauge Data.json";
          
            var from = new DateTime(2019, 02, 05, 6, 0, 0);
            var to = new DateTime(2019, 02, 28, 11, 0, 0);
            Assert.Equal(11, repository.GetValues(id, from, to).Value.DateTimes.Count);
            Assert.Equal(11, repository.GetValues(id, from, to).Value.Values.Count);
        }

        [Fact]
        public void GetValues2IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\89 Recipe.json");
            //var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\89 Recipe - without comment.json");


            const string id = @"..\..\..\Data\Json\89.json";
            Assert.Equal(176, repository.GetValues(id).Value.Values.First());
        }

        [Fact]
        public void GetValues3IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Lyrup Pump Station Recipe.json");
            const string id = @"..\..\..\Data\Json\Lyrup Pump Station.json";
            Assert.Equal(13.288, repository.GetValues(id).Value.Values.First());
            Assert.Equal(new DateTime(2019, 5, 27, 14, 0, 0), repository.GetValues(id).Value.DateTimes.First());
        }

        [Fact]
        public void GetValues4IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\WillyWeather Recipe.json");
            const string id = @"..\..\..\Data\Json\WillyWeather.json";
            Assert.Equal(1.8, repository.GetValues(id).Value.Values.First());
            Assert.Equal(new DateTime(2019, 7, 9, 0, 30, 0), repository.GetValues(id).Value.DateTimes.First());
        }

        [Fact]
        public void GetValues5IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Json\Hindmarsh Island WW Recipe.json");
            const string id = @"Hindmarsh Island WW.json;AAATypeAAA=wind;AAAVariableAAA=y";
            Assert.Equal(44.3, repository.GetValues(id).Value.Values.First());
            Assert.Equal(new DateTime(2019, 6, 29, 22, 30, 0), repository.GetValues(id).Value.DateTimes.First());
        }
    }
}

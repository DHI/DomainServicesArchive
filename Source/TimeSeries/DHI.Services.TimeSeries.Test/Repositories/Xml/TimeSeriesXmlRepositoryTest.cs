namespace DHI.Services.TimeSeries.Test.Repositories.Xml
{
    using System;
    using System.Linq;
    using DHI.Services.TimeSeries.Xml;
    using Xunit;

    public class TimeSeriesXmlRepositoryTest
    {
        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<ITimeSeriesRepository<string, double>>(new TimeSeriesRepository(@"..\..\..\Data\Xml\IDQ60920.json"));
        }

        [Fact]
        public void CreateWithNullRootThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesRepository(null));
        }

        [Fact]
        public void CreateWithIllegalRootThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository("NonExistingJson"));
        }

        [Fact]
        public void GetWithInvalidPathThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository(@"..\..\..\Data\Xml\Brisbane Station PW015.json").GetValues("InvalidPath;whatever"));
        }

        [Fact]
        public void GetWithInvalidItemThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository(@"..\..\..\Data\Xml\Brisbane Station PW015.json").GetValues("Beacon2F_data_current.csv;whatever"));
        }

        [Fact]
        public void GetValue1IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Xml\IDQ60920.json");
            var values = repository.GetValues(@"..\..\..\Data\Xml\IDQ60920.xml");
            var dateTimeRef = new DateTime(2020, 04, 27, 06, 00, 00);
            Assert.Equal(1, values.Value.Values.Count);
            Assert.Equal(59, values.Value.Values.First().Value);
            Assert.Equal(dateTimeRef, values.Value.DateTimes[0]);
        }

        [Fact]
        public void GetValue2IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Xml\WindDirBRISBANEAERO.json");
            var values = repository.GetValues(@"..\..\..\Data\Xml\WindDirBRISBANEAERO.xml");
            Assert.Equal(0, values.Value.Values.Count);
        }
        [Fact]
        public void GetTimeIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Xml\TimeSeriesConfigWindGustEntranceBeacon.json");
            var values = repository.GetValues(@"..\..\..\Data\Xml\AdelaideEntranceBeacon.xml");
            var dateTimeRef = new DateTime(2022, 02, 28, 05, 00, 00);
            Assert.Equal(168, values.Value.Values.Count);
            Assert.Equal(dateTimeRef, values.Value.DateTimes[0]);
        }
    }
}

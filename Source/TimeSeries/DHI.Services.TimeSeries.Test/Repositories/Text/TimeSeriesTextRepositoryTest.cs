namespace DHI.Services.TimeSeries.Test.Repositories.Text
{
    using System;
    using System.Linq;
    using DHI.Services.TimeSeries.Text;
    using Xunit;

    public class TimeSeriesTextRepositoryTest
    {
        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<ITimeSeriesRepository<string, double>>(new TimeSeriesRepository(@"..\..\..\Data\Text\201811134618_1172.json"));
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
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository(@"..\..\..\Data\Text\201811134618_1172.json").GetValues("InvalidPath;whatever"));
        }

        [Fact]
        public void GetWithInvalidItemThrows()
        {
            Assert.Throws<ArgumentException>(() => new TimeSeriesRepository(@"..\..\..\Data\Text\201811134618_1172.json").GetValues("Beacon2F_data_current.csv;whatever"));
        }

        [Fact]
        public void GetValues1IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\201811134618_1172.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\201811134618_1172.txt;MyTimeseries1");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(16.58, firstValue);
        }

        [Fact]
        public void GetValues2IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\Beacon2F_data_current.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\Beacon2F_data_current.csv;Water Speed (Knots)");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(0.375167, firstValue);
        }
        
        [Fact]
        public void GetValues3IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\calo_2018-09.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\calo_2018-09.his;MyTimeseries1");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(124.0, firstValue);
        }

        [Fact]
        public void GetValues4IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\oh_tide_201707261539.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\oh_tide_201707261539.csv;MyTimeseries1");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(1.725, firstValue);
            Assert.Equal(new DateTime(2017, 07, 26, 15, 4, 0), maybe.Value.DateTimes.First());
        }

        [Fact]
        public void GetValues5IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\tweedheads_4225.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\tweedheads_4225.csv;MyTimeseries1");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(-0.1797752808988764, firstValue);
        }

        [Fact]
        public void GetValues6IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\Vejstation.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\Vejstation.txt;UTMI3002.PV");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(44.9412, firstValue);
        }

        [Fact]
        public void GetValues7IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\DHI-WindSummary.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\DHI-WindSummary.txt;WindSpeed");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(8.313, firstValue);
        }

        [Fact]
        public void GetValues8IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\143001C.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\143001C.csv;value");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(4.9182, firstValue);
        }

        [Fact]
        public void GetValues9IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\44099.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\44099.spec;SwP");
            Assert.True(maybe.HasValue);
            var firstValue = maybe.Value.Values.First();
            Assert.NotNull(firstValue);
            Assert.Equal(14.3, firstValue);
            Assert.Equal(new DateTime(2019, 5, 19, 0, 0, 0), maybe.Value.DateTimes.First());
        }

        [Fact]
        public void GetValues10IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\oh_tide_201707261539SkipIfCannotParse.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\oh_tide_201707261539SkipIfCannotParse.csv;MyTimeseries1");
            Assert.True(maybe.HasValue);
            Assert.Equal(29, maybe.Value.Values.Count);
        }

        [Fact]
        public void GetValues11IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\oh_tide_201707261539NullIfCannotParse.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\oh_tide_201707261539NullIfCannotParse.csv;MyTimeseries1");
            Assert.True(maybe.HasValue);
            Assert.Equal(31, maybe.Value.Values.Count);
            Assert.Equal(2, maybe.Value.Values.Count(r => !r.HasValue));
        }

        [Fact]
        public void GetValues12IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\cb0402-currents.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\cb0402-currents.csv;Speed");
            Assert.True(maybe.HasValue);
            Assert.Equal(0, maybe.Value.Values.Count);
        }

        [Fact]
        public void GetValues13IsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\HvideSande.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\HvideSande.txt;Hs");
            Assert.True(maybe.HasValue);
            Assert.Equal(1, maybe.Value.Values.Count);
        }

        [Fact]
        public void BrokenFileReturnsEmpty()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\OuterTide.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\OuterTide.txt;ReallyBrokenFile");
            Assert.True(maybe.HasValue);
            Assert.Empty(maybe.Value.Values);
        }

        [Fact]
        public void ResampleIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\oh_tide_201707261539Resample.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\oh_tide_201707261539Resample.csv;MyTimeseries1");
            Assert.True(maybe.HasValue);
            Assert.Equal(3, maybe.Value.Values.Count);
        }

        [Fact]
        public void ResampleWithTooLargeTimeSpanReturnsEmpty()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\ttTest_Resample-Config.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\ttTest.csv;WSPD");
            Assert.Empty(maybe.Value.Values);
        }

        [Fact]
        public void ValueRegExFilterIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\WAVE.RAW.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\WAVE.RAW.txt;depth");
            Assert.Equal(21608, maybe.Value.Values.Count);
        }

        [Fact]
        public void SkipIfCannotParseDateTime()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\WAVE.RAW-Skip.json");
            var maybe = repository.GetValues(@"..\..\..\Data\Text\WAVE.RAW-Skip.txt;depth");
            Assert.Equal(21600, maybe.Value.Values.Count);
        }

        [Fact]
        public void FillEmptyValueIsOk()
        {
            var repository = new TimeSeriesRepository(@"..\..\..\Data\Text\143001C_with_empty_value.json");
            
            var valueMaybe = repository.GetValues(@"..\..\..\Data\Text\143001C_with_empty_value.csv;value");
            Assert.True(valueMaybe.HasValue);
            Assert.Equal(4, valueMaybe.Value.Values.Count);
            Assert.Contains(Double.NaN, valueMaybe.Value.Values);

            var qualityMaybe = repository.GetValues(@"..\..\..\Data\Text\143001C_with_empty_value.csv;quality");
            Assert.True(qualityMaybe.HasValue);
            Assert.Equal(4, qualityMaybe.Value.Values.Count);
            Assert.Contains(Double.NaN, qualityMaybe.Value.Values);

            // Thre is '-' in the column, as no handling to skip or null, still throw exception directly
            Assert.Throws<Exception>(() => repository.GetValues(@"..\..\..\Data\Text\143001C_with_empty_value.csv;trend"));

            var varMaybe = repository.GetValues(@"..\..\..\Data\Text\143001C_with_empty_value.csv;var");
            Assert.True(varMaybe.HasValue);
            Assert.Equal(4, varMaybe.Value.Values.Count);
            Assert.Contains(Double.NaN, varMaybe.Value.Values);
        }
    }
}

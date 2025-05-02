namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class StandardDeviationTest
    {
        [Fact]
        public void StandardDeviationIsOk()
        {
            var data = new TimeSeriesData(new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 4),
                    new DateTime(2015, 1, 6),
                    new DateTime(2015, 1, 9),
                    new DateTime(2015, 1, 11),
                },
                new List<double?>
                {
                    10.0, 11.0, 10.0, 9.0,13.0
                });

            Assert.Equal(1.5165750888103104, data.StandardDeviation());
        }

        [Fact]
        public void StandardDeviationPeriodicallyIsOk()
        {
            var yearly = TestData.TimeSeriesData.StandardDeviation(Period.Yearly);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(2.183269719175042, yearly.Values[0]);

            var monthly = TestData.TimeSeriesData.StandardDeviation(Period.Monthly);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(2.183269719175042, monthly.Values[0]);

            var daily = TestData.TimeSeriesData.StandardDeviation(Period.Daily);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(double.NaN, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = TestData.TimeSeriesDataDense.StandardDeviation(Period.Hourly);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(0.7071067811865476, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(2.1213203435596424, hourly.Values[2]);
        }
    }
}
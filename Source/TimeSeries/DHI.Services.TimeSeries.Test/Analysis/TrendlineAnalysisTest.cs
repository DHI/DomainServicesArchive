namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class TrendlineAnalysisTest
    {
        [Fact]
        public void LinearTrendlineIsOk()
        {
            var data = new TimeSeriesData(new List<DateTime>
                {
                    new DateTime(2020, 2, 1),
                    new DateTime(2020, 2, 1),
                    new DateTime(2020, 2, 3),
                    new DateTime(2020, 2, 4),
                    new DateTime(2020, 2, 5),
                    new DateTime(2020, 2, 6)
                },
                new List<double?>
                {
                    10, 7, 9, 12, 11, 17
                });

            var (slope, offset, trendline) = data.LinearTrendline();
            Assert.Equal(1.359375, slope);
            Assert.Equal(7.828125, offset);
            Assert.Equal(new DateTime(2020, 2, 1), trendline.GetFirstDateTime().Value);
            Assert.Equal(new DateTime(2020, 2, 6), trendline.GetLastDateTime().Value);
            Assert.Equal(7.828125, trendline.GetFirst().Value.Value);
            Assert.Equal(slope * (new DateTime(2020, 2, 6) - new DateTime(2020, 2, 1)).TotalDays + offset, trendline.GetLast().Value.Value);
        }
    }
}
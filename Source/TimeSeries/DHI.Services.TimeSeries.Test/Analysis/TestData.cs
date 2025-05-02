namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;

    public static class TestData
    {
        public static readonly TimeSeriesData TimeSeriesDataDense = new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2000, 1, 1, 0, 0, 0),
                new DateTime(2000, 1, 1, 0, 30, 0),
                new DateTime(2000, 1, 1, 1, 0, 0),
                new DateTime(2000, 1, 1, 1, 30, 0),
                new DateTime(2000, 1, 1, 2, 0, 0),
                new DateTime(2000, 1, 1, 2, 30, 0),
                new DateTime(2000, 1, 1, 3, 0, 0),
                new DateTime(2000, 1, 1, 3, 30, 0),
                new DateTime(2000, 1, 1, 4, 0, 0),
                new DateTime(2000, 1, 1, 4, 30, 0),
                new DateTime(2000, 1, 1, 5, 0, 0)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            });

        public static TimeSeriesDataWFlag<int?> TimeSeriesData => new TimeSeriesDataWFlag<int?>(
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
            },
            new List<int?>
            {
                1, 1, 1, 0, 1, 1, null, 0, null, 1, 0
            });

        public static TimeSeriesData TimeSeriesDataWithGaps => new TimeSeriesData(
            new List<DateTime>
            {
                new DateTime(2015, 1, 1),
                new DateTime(2015, 1, 4),
                new DateTime(2015, 1, 6),
                new DateTime(2015, 1, 9)
            },
            new List<double?>
            {
                11.1, 11.1, 11.1, 11.1
            });
    }
}
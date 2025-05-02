namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DisaggregateTest
    {
        [Fact]
        public void CreateABackwardsAggregatedTimeseries()
        {
            // Arrange
            var inputTimeSeries = new TimeSeriesData<double>(DateTimesStub(), DoublesStub());

            // Act
            var result = inputTimeSeries.DisaggregateBackward();

            // Assert
            var resultList = result.ToSortedSet().ToList();
            var expectedData = ExpectedDisaggregateBackwardResults();

            Assert.Equal(expectedData.Count, resultList.Count);
            Assert.True(expectedData.SequenceEqual(resultList));
        }


        [Fact]
        public void CreateAForwardAggregatedTimeseries()
        {
            // Arrange
            var inputTimeSeries = new TimeSeriesData<double>(DateTimesStub(), DoublesStub());

            // Act
            var result = inputTimeSeries.DisaggregateForward();

            // Assert
            var resultList = result.ToSortedSet().ToList();
            var expectedData = ExpectedDisaggregateForwardResults();

            Assert.Equal(expectedData.Count, resultList.Count);
            Assert.True(expectedData.SequenceEqual(resultList));
        }


        #region setup and expected

        private List<DataPoint<double>> ExpectedDisaggregateForwardResults()
        {
            var expectedData = new List<DataPoint<double>>
            {
                new DataPoint<double>(new DateTime(2023, 9, 5, 14, 30, 0), 9.5),
                new DataPoint<double>(new DateTime(2023, 9, 6, 9, 15, 0), 15.75),
                new DataPoint<double>(new DateTime(2023, 9, 7, 16, 45, 0), 8.3),
            };

            return expectedData;
        }

        private List<DataPoint<double>> ExpectedDisaggregateBackwardResults()
        {
            var expectedData = new List<DataPoint<double>>
            {
                new DataPoint<double>(new DateTime(2023, 9, 5, 10, 0, 0), 9.5),
                new DataPoint<double>(new DateTime(2023, 9, 5, 14, 30, 0), 15.75),
                new DataPoint<double>(new DateTime(2023, 9, 6, 9, 15, 0), 8.3),
            };

            return expectedData;
        }

        private IList<DateTime> DateTimesStub()
        {
            var dateTimeList = new List<DateTime>
            {
                new DateTime(2023, 9, 5, 10, 0, 0),
                new DateTime(2023, 9, 5, 14, 30, 0),
                new DateTime(2023, 9, 6, 9, 15, 0),
                new DateTime(2023, 9, 7, 16, 45, 0)
            };

            return dateTimeList;
        }

        private IList<double?> DoublesStub()
        {
            var doubleList = new List<double?>
            {
                10.5,
                20.0,
                15.75,
                8.3
            };

            return doubleList;
        }

        #endregion

    }

}

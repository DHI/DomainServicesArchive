namespace DHI.Services.TimeSteps.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using TimeSteps;
    using Xunit;

    public class TimeStepServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeStepService<int, float[]>(null));
        }

        [Theory, AutoTimeStepData]
        public void GetNonExistingDateThrows(TimeStepService<string, double[]> timeStepService)
        {
            var id = timeStepService.GetItemIds()[0];
            Assert.Throws<KeyNotFoundException>(() => timeStepService.Get(id, DateTime.MinValue));
        }

        [Theory, AutoTimeStepData]
        public void GetNonExistingIdThrows(TimeStepService<string, double[]> timeStepService)
        {
            var dateTime = timeStepService.GetDateTimes()[0];
            Assert.Throws<KeyNotFoundException>(() => timeStepService.Get("NonExistingId", dateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetFirstAfterWithIllegalDateThrows(TimeStepService<string, double[]> timeStepService)
        {
            var id = timeStepService.GetItemIds()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeStepService.GetFirstAfter(id, DateTime.MaxValue));
        }

        [Theory, AutoTimeStepData]
        public void GetFirstAfterWithNonExistingIdReturnsNull(TimeStepService<string, double[]> timeStepService)
        {
            var dateTime = timeStepService.GetDateTimes()[0];
            Assert.Null(timeStepService.GetFirstAfter("NonExistingId", dateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetLastBeforeWithIllegalDateThrows(TimeStepService<string, double[]> timeStepService)
        {
            var id = timeStepService.GetItemIds()[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => timeStepService.GetLastBefore(id, DateTime.MinValue));
        }

        [Theory, AutoTimeStepData]
        public void GetLastBeforeWithNonExistingIdReturnsNull(TimeStepService<string, double[]> timeStepService)
        {
            var dateTime = timeStepService.GetDateTimes().Last();
            Assert.Null(timeStepService.GetLastBefore("NonExistingId", dateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetLastWithNonExistingIdReturnsNull(TimeStepService<string, double[]> timeStepService)
        {
            Assert.Null(timeStepService.GetLast("NonExistingId"));
        }

        [Theory, AutoMoqData]
        public void GetLastReturnsNullIfNoTimeSteps(Mock<BaseTimeStepServer<Guid, int[]>> timeStepServerMock, Guid itemId)
        {
            timeStepServerMock.Setup(s => s.GetLastDateTime(null)).Returns(default(DateTime?));
            var timeStepServer = timeStepServerMock.Object;
            var timeStepService = new TimeStepService<Guid, int[]>(timeStepServer);
            Assert.Null(timeStepService.GetLast(itemId));
        }

        [Theory, AutoMoqData]
        public void GetLastDateTimeReturnsNullIfNoTimeSteps(Mock<BaseTimeStepServer<Guid, int[]>> timeStepServerMock)
        {
            timeStepServerMock.Setup(s => s.GetLastDateTime(null)).Returns(default(DateTime?));
            var timeStepServer = timeStepServerMock.Object;
            var timeStepService = new TimeStepService<Guid, int[]>(timeStepServer);
            Assert.Null(timeStepService.GetLastDateTime());
        }

        [Theory, AutoMoqData]
        public void GetFirstAfterReturnsNullIfNoTimeSteps(Mock<BaseTimeStepServer<Guid, int[]>> timeStepServerMock, Guid itemId)
        {
            timeStepServerMock.Setup(s => s.GetLastDateTime(null)).Returns(default(DateTime?));
            var timeStepServer = timeStepServerMock.Object;
            var timeStepService = new TimeStepService<Guid, int[]>(timeStepServer);
            Assert.Null(timeStepService.GetFirstAfter(itemId, DateTime.MinValue));
        }

        [Theory, AutoMoqData]
        public void GetLastBeforeReturnsNullIfNoTimeSteps(Mock<BaseTimeStepServer<Guid, int[]>> timeStepServerMock, Guid itemId)
        {
            timeStepServerMock.Setup(s => s.GetFirstDateTime(null)).Returns(default(DateTime?));
            var timeStepServer = timeStepServerMock.Object;
            var timeStepService = new TimeStepService<Guid, int[]>(timeStepServer);
            Assert.Null(timeStepService.GetLastBefore(itemId, DateTime.MaxValue));
        }

        [Theory, AutoTimeStepData]
        public void GetDateTimesIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var dateTimes = timeStepService.GetDateTimes();
            Assert.True(dateTimes.Any());
        }

        [Theory, AutoTimeStepData]
        public void GetItemIdsIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var itemIds = timeStepService.GetItemIds();
            Assert.IsType<string[]>(itemIds);
            Assert.True(itemIds.Any());
        }

        [Theory, AutoTimeStepData]
        public void GetItemsIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var items = timeStepService.GetItems();
            Assert.IsType<Item<string>[]>(items);
        }

        [Theory, AutoTimeStepData]
        public void GetIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var dateTime = timeStepService.GetDateTimes()[0];
            var itemId = timeStepService.GetItemIds()[0];
            Assert.IsType<double[]>(timeStepService.Get(itemId, dateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetManyIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var dictionary = new Dictionary<string, IEnumerable<DateTime>>();
            for (int i = 0; i < timeStepService.GetItemIds().Length; i++)
            {
                dictionary.Add(timeStepService.GetItemIds()[i], timeStepService.GetDateTimes());
            }

            var result = timeStepService.Get(dictionary);
            foreach (var item in timeStepService.GetItemIds())
            {
                Assert.Equal(result[item].Keys.ToList(), timeStepService.GetDateTimes());
            }
        }

        [Theory, AutoTimeStepData]
        public void GetLastDateTimesIsOk(TimeStepService<string, double[]> timeStepService)
        {
            Assert.IsType<DateTime>(timeStepService.GetLastDateTime());
        }

        [Theory, AutoTimeStepData]
        public void GetLastIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var lastDateTime = (DateTime)timeStepService.GetLastDateTime();
            var itemId = timeStepService.GetItemIds()[0];
            Assert.Equal(timeStepService.GetLast(itemId), timeStepService.Get(itemId, lastDateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetFirstAfterIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var itemId = timeStepService.GetItemIds()[0];
            Assert.IsType<double[]>(timeStepService.GetFirstAfter(itemId, DateTime.MinValue));
        }

        [Theory, AutoTimeStepData]
        public void GetLastBeforeIsOk(TimeStepService<string, double[]> timeStepService)
        {
            var itemId = timeStepService.GetItemIds()[0];
            Assert.IsType<double[]>(timeStepService.GetLastBefore(itemId, DateTime.MaxValue));
        }
    }
}
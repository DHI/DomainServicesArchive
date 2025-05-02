namespace DHI.Services.TimeSteps.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TimeSteps;
    using Xunit;

    public class BaseTimeStepServerTest
    {
        [Theory, AutoTimeStepData]
        public void FirstDateTimeIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            Assert.Equal(timeStepServer.GetDateTimes().Min(), timeStepServer.GetFirstDateTime());
        }

        [Fact]
        public void FirstDateTimeReturnsNullIfNoTimeSteps()
        {
            var timeStepServer = new FakeTimeStepServer<string, double[]>(new Dictionary<TimeStep<string>, double[]>());
            Assert.Null(timeStepServer.GetFirstDateTime());
        }

        [Theory, AutoTimeStepData]
        public void LastDateTimeIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            Assert.Equal(timeStepServer.GetDateTimes().Max(), timeStepServer.GetLastDateTime());
        }

        [Fact]
        public void LastDateTimeReturnsNullIfNoTimeSteps()
        {
            var timeStepServer = new FakeTimeStepServer<string, double[]>(new Dictionary<TimeStep<string>, double[]>());
            Assert.Null(timeStepServer.GetLastDateTime());
        }

        [Theory, AutoTimeStepData]
        public void ContainsDateTimeIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            var dateTime = timeStepServer.GetDateTimes().First();
            Assert.True(timeStepServer.ContainsDateTime(dateTime));
        }

        [Theory, AutoTimeStepData]
        public void DoesNotContainDateTimeIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            Assert.False(timeStepServer.ContainsDateTime(default(DateTime)));
        }

        [Theory, AutoTimeStepData]
        public void ContainsItemIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            var itemId = timeStepServer.GetItemIds().First();
            Assert.True(timeStepServer.ContainsItem(itemId));
        }

        [Theory, AutoTimeStepData]
        public void ItemIdsIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            Assert.IsType<string[]>(timeStepServer.GetItemIds());
        }

        [Theory, AutoTimeStepData]
        public void DoesNotContainItemIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            Assert.False(timeStepServer.ContainsItem("NonExistingItemId"));
        }

        [Theory, AutoTimeStepData]
        public void GetFirstAfterIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            var itemId = timeStepServer.GetItemIds().First();
            var dateTime = timeStepServer.GetDateTimes().Min();
            Assert.Equal(timeStepServer.GetFirstAfter(itemId, DateTime.MinValue), timeStepServer.Get(itemId, dateTime));
        }

        [Theory, AutoTimeStepData]
        public void GetLastBeforeIsOk(ITimeStepServer<string, double[]> timeStepServer)
        {
            var itemId = timeStepServer.GetItemIds().First();
            var dateTime = timeStepServer.GetDateTimes().Max();
            Assert.Equal(timeStepServer.GetLastBefore(itemId, DateTime.MaxValue), timeStepServer.Get(itemId, dateTime));
        }
    }
}
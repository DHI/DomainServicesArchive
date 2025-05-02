namespace DHI.Services.Places.Test
{
    using System;
    using Xunit;

    public class TimeIntervalTest
    {
        [Fact]
        public void CreateWithStartLargerThenEndThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeInterval.CreateRelativeToNow(-10, -20));
            Assert.Throws<ArgumentException>(() => TimeInterval.CreateRelativeToDateTime(-10, -20));
            Assert.Throws<ArgumentException>(() => TimeInterval.CreateFixed(-10, -20));
        }

        [Fact]
        public void MissingStartOrEndInConstructorThrowsIfTypeIsNotAll()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeInterval(TimeIntervalType.RelativeToNow, null, -20));
            Assert.Throws<ArgumentNullException>(() => new TimeInterval(TimeIntervalType.RelativeToNow, 10, null));
            Assert.Throws<ArgumentNullException>(() => new TimeInterval(TimeIntervalType.RelativeToNow, null, null));
            Assert.Throws<ArgumentNullException>(() => new TimeInterval(TimeIntervalType.Fixed, null, null));
            Assert.Throws<ArgumentNullException>(() => new TimeInterval(TimeIntervalType.RelativeToDateTime, null, null));
        }

        [Fact]
        public void ToPeriodForAllThrows()
        {
            var timeInterval = new TimeInterval(TimeIntervalType.All, 0, 0);
            Assert.Throws<NotSupportedException>(() => timeInterval.ToPeriod());
        }

        [Fact]
        public void ToPeriodWithMissingStartOrEndThrows()
        {
            var timeInterval = new TimeInterval(TimeIntervalType.All, null, 0);
            Assert.Throws<Exception>(() => timeInterval.ToPeriod());
        }

        [Fact]
        public void ToPeriodWithMissingDateTimeThrows()
        {
            var timeInterval = TimeInterval.CreateRelativeToDateTime(0, 1);
            Assert.Throws<ArgumentException>(() => timeInterval.ToPeriod());
        }

        [Fact]
        public void ToPeriodFixedIsOk()
        {
            var end = DateTime.Now;
            var start = end.AddDays(-1);
            var timeInterval = TimeInterval.CreateFixed(start.ToOADate(), end.ToOADate());
            var (from, to) = timeInterval.ToPeriod();

            Assert.Equal(start, from, TimeSpan.FromMilliseconds(1));
            Assert.Equal(end, to, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void ToPeriodRelativeToDateTimeIsOk()
        {
            var dateTime = new DateTime(1963, 8, 18);
            const int start = -3;
            const double end = 2.5;
            var timeInterval = TimeInterval.CreateRelativeToDateTime(start, end);
            var (from, to) = timeInterval.ToPeriod(dateTime);

            Assert.Equal(dateTime.AddDays(start), from, TimeSpan.FromMilliseconds(1));
            Assert.Equal(dateTime.AddDays(end), to, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void ToPeriodRelativeToNowIsOk()
        {
            const int start = -3;
            const double end = 2.5;
            var timeInterval = TimeInterval.CreateRelativeToNow(start, end);
            var (from, to) = timeInterval.ToPeriod();

            Assert.Equal(DateTime.Now.AddDays(start), from, TimeSpan.FromMinutes(5));
            Assert.Equal(DateTime.Now.AddDays(end), to, TimeSpan.FromMinutes(5));
        }
    }
}
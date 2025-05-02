namespace DHI.Services.Rasters.Test
{
    using AutoFixture;
    using Rasters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Xunit;

    public class RasterServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryWillThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new RasterService<FakeRaster>(null));
        }

        [Theory, AutoData]
        public void GetNonExistingThrows(RasterService<FakeRadarImage> images)
        {
            Assert.False(images.TryGet(DateTime.MinValue, out var rs));
        }

        [Theory, AutoData]
        public void GetWithIllegalIntervalThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.Get(DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetFirstAfterWithIllegalDateThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetFirstAfter(DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetLastBeforeWithIllegalDateThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetLastBefore(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetLastBeforeListThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetLastBefore(new List<DateTime> { DateTime.MinValue }));
        }

        [Theory, AutoData]
        public void GetFirstAfterListThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetFirstAfter(new List<DateTime> { DateTime.MaxValue }));
        }

        [Theory, AutoData]
        public void GetWithTooLargeTimeSpanThrows(RasterService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.Get(DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.Last();
            images.TryGet(image.Id, out var rs);
            Assert.Equal(image.Id, rs.Id);
        }

        [Theory, AutoData]
        public void ExistsIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.Last();
            Assert.True(images.Exists(image.Id));
        }

        [Theory, AutoData]
        public void DoesNotExistIsOk(RasterService<FakeRadarImage> images)
        {
            Assert.False(images.Exists(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(RasterService<FakeRadarImage> images)
        {
            Assert.True(images.LastDateTime() > images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetLastIsOk(RasterService<FakeRadarImage> images)
        {
            var last = images.Last();
            Assert.Equal(last.DateTime, images.LastDateTime());
        }

        [Theory, AutoData]
        public void GetFirstAfterIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.GetFirstAfter(images.FirstDateTime());
            Assert.True(image.DateTime > images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetLastBeforeIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.GetLastBefore(images.LastDateTime());
            Assert.True(image.DateTime < images.LastDateTime());
        }

        [Theory, AutoData]
        public void GetFirstAfterListIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.GetFirstAfter(new List<DateTime> { images.FirstDateTime().AddSeconds(-1) });
            Assert.True(image.First().DateTime == images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetLastBeforeListIsOk(RasterService<FakeRadarImage> images)
        {
            var image = images.GetLastBefore(new List<DateTime> { images.LastDateTime().AddSeconds(1) });
            Assert.True(image.First().DateTime == images.LastDateTime());
        }

        [Theory, AutoData]
        public void GetDateTimes(RasterService<FakeRadarImage> images)
        {
            var dateTimes = images.GetDateTimes(images.FirstDateTime(), images.LastDateTime());
            Assert.True(dateTimes.Any());
        }

        [Theory, AutoData]
        public void GetDateTimesFirstAfterListIsOk(RasterService<FakeRadarImage> images)
        {
            var dateTimes = images.GetDateTimesFirstAfter(new List<DateTime> { images.FirstDateTime().AddSeconds(-1) });
            Assert.True(dateTimes.First() == images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetDateTimesLastBeforeListIsOk(RasterService<FakeRadarImage> images)
        {
            var dateTimes = images.GetDateTimesLastBefore(new List<DateTime> { images.LastDateTime().AddSeconds(1) });
            Assert.True(dateTimes.First() == images.LastDateTime());
        }

        [Fact]
        public void GetTimeSeriesIsOk()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            var repository = new FakeRasterRepository<FakeRaster>(fixture.CreateMany<FakeRaster>().ToList());
            var images = new RasterService<FakeRaster>(repository, TimeSpan.MaxValue);
            var timeSeries = images.Get(images.FirstDateTime().AddMilliseconds(1), images.LastDateTime().AddMilliseconds(-1));

            Assert.Equal(fixture.RepeatCount - 2, timeSeries.Count);
        }

        [Fact]
        public void GetMetadataIsOk()
        {
            var raster = new FakeRaster(DateTime.Now, "foo");
            raster.Metadata.Add("foo", "bar");
            var json = JsonSerializer.Serialize(raster, new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            });

            var deserializedRaster = JsonSerializer.Deserialize<FakeRaster>(json, new JsonSerializerOptions()
            {
                Converters = { new MetadataConverter() }
            });

            Assert.Equal("bar", deserializedRaster.Metadata["foo"]);
        }
    }
}
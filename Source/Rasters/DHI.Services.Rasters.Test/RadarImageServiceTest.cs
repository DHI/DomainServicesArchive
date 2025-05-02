namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Radar;
    using Radar.DELIMITEDASCII;
    using Rasters;
    using Xunit;
    using Zones;

    public class RadarImageServiceTest
    {
        private readonly IFixture _fixture;

        public RadarImageServiceTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        }

        [Fact]
        public void CreateWithNullRepositoryWillThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new RadarImageService<FakeRadarImage>(null));
        }

        [Theory, AutoData]
        public void GetNonExistingThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.False(images.TryGet(DateTime.MinValue, out _));
        }

        [Theory, AutoData]
        public void GetIntensityNonExistingThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<KeyNotFoundException>(() => images.GetIntensity(zone, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetWithIllegalIntervalThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.Get(DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetIntensitiesWithIllegalIntervalThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetIntensities(zone, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetIntensitiesWithTooLongTimeSpanThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetIntensities(zone, DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetAverageIntensityWithIllegalIntervalThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetAverageIntensity(zone, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetAverageIntensityWithTooLongTimeSpanThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetAverageIntensity(zone, DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetDepthWithIllegalIntervalThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetDepth(zone, DateTime.MinValue, DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetDepthWithTooLongTimeSpanThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<ArgumentException>(() => images.GetDepth(zone, DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetDepthWithTooShortIntervalThrows(RadarImageService<FakeRadarImage> images, Zone zone)
        {
            Assert.Throws<Exception>(() => images.GetDepth(zone, images.FirstDateTime(), images.FirstDateTime().AddMilliseconds(1)));
        }

        [Theory, AutoData]
        public void GetFirstAfterWithIllegalDateThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetFirstAfter(DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetLastBeforeWithIllegalDateThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetLastBefore(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetWithTooLargeTimeSpanThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.Get(DateTime.MinValue, DateTime.MaxValue));
        }

        [Theory, AutoData]
        public void GetLastBeforeListThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetLastBefore(new List<DateTime> { DateTime.MinValue }));
        }

        [Theory, AutoData]
        public void GetFirstAfterListThrows(RadarImageService<FakeRadarImage> images)
        {
            Assert.Throws<ArgumentException>(() => images.GetFirstAfter(new List<DateTime> { DateTime.MaxValue }));
        }

        [Theory, AutoData]
        public void GetIsOk(RadarImageService<FakeRadarImage> images)
        {
            var image = images.Last();
            images.TryGet(image.Id, out var rs);
            Assert.Equal(image.Id, rs.Id);
        }

        [Theory, AutoData]
        public void GetCorrectedReturnsOriginalImageIfNoCorrectionMatrixFound(RadarImageService<FakeRadarImage> images, BiasCorrectionService biasCorrectionService)
        {
            var correctedImage = images.GetCorrected(images.LastDateTime(), biasCorrectionService);

            var pixel = new Pixel(1, 1);
            Assert.Equal(images.Last().GetValue(pixel), correctedImage.GetValue(pixel));

        }

        [Theory, AutoData]
        public void GetCorrectedIsOk(RadarImageService<FakeRadarImage> images, BiasCorrectionService biasCorrectionService)
        {
            //var images = _fixture.Create<RadarImageService<FakeRadarImage>>();
            var correctionMatrix = new Matrix(images.FirstDateTime()) { Size = new Size(_fixture.RepeatCount, _fixture.RepeatCount) };
            _fixture.AddManyTo(correctionMatrix.Values, correctionMatrix.Size.Width * correctionMatrix.Size.Height);
            biasCorrectionService.Add(correctionMatrix);

            var pixel = new Pixel(1, 1);
            var expected = images.Last().GetValue(pixel) * correctionMatrix.GetValue(pixel);
            var correctedImage = images.GetCorrected(images.LastDateTime(), biasCorrectionService);

            Assert.Equal(expected, correctedImage.GetValue(pixel));
        }

        [Theory, AutoData]
        public void ExistsIsOk(RadarImageService<FakeRadarImage> images)
        {
            var image = images.Last();
            Assert.True(images.Exists(image.Id));
        }

        [Theory, AutoData]
        public void DoesNotExistIsOk(RadarImageService<FakeRadarImage> images)
        {
            Assert.False(images.Exists(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void GetLastDateTimeAndGetFirstDateTimeIsOk(RadarImageService<FakeRadarImage> images)
        {
            Assert.True(images.LastDateTime() > images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetLastIsOk(RadarImageService<FakeRadarImage> images)
        {
            var last = images.Last();
            Assert.Equal(last.DateTime, images.LastDateTime());
        }

        [Theory, AutoData]
        public void GetFirstAfterIsOk(RadarImageService<FakeRadarImage> images)
        {
            var image = images.GetFirstAfter(images.FirstDateTime());
            Assert.True(image.DateTime > images.FirstDateTime());
        }

        [Theory, AutoData]
        public void GetLastBeforeIsOk(RadarImageService<FakeRadarImage> images)
        {
            var image = images.GetLastBefore(images.LastDateTime());
            Assert.True(image.DateTime < images.LastDateTime());
        }

        [Fact]
        public void GetTimeSeriesIsOk()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var timeSeries = images.Get(images.FirstDateTime().AddMilliseconds(1), images.LastDateTime().AddMilliseconds(-1));

            Assert.Equal(_fixture.RepeatCount - 2, timeSeries.Count);
        }

        [Fact]
        public void GetFirstAfterListIsOk()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var image = images.GetFirstAfter(new List<DateTime> { images.FirstDateTime().AddSeconds(-1) });
            Assert.True(image.First().DateTime == images.FirstDateTime());
        }

        [Fact]
        public void GetLastBeforeListIsOk()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var image = images.GetLastBefore(new List<DateTime> { images.LastDateTime().AddSeconds(1) });
            Assert.True(image.First().DateTime == images.LastDateTime());
        }

        [Fact]
        public void GetDateTimes()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var dateTimes = images.GetDateTimes(images.FirstDateTime(), images.LastDateTime());
            Assert.True(dateTimes.Any());
        }

        [Fact]
        public void GetDateTimesFirstAfterListIsOk()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var dateTimes = images.GetDateTimesFirstAfter(new List<DateTime> { images.FirstDateTime().AddSeconds(-1) });
            Assert.True(dateTimes.First() == images.FirstDateTime());
        }

        [Fact]
        public void GetDateTimesLastBeforeListIsOk()
        {
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var dateTimes = images.GetDateTimesLastBefore(new List<DateTime> { images.LastDateTime().AddSeconds(1) });
            Assert.True(dateTimes.First() == images.LastDateTime());
        }

        [Theory, AutoData]
        public void GetIntensityIsOk(RadarImageService<FakeRadarImage> images)
        {
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            var intensity = images.GetIntensity(zone, images.LastDateTime());

            Assert.Equal(1, intensity);
        }

        [Fact]
        public void GetIntensitiesIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            // Exercise system
            var intensities = images.GetIntensities(zone, images.FirstDateTime(), images.LastDateTime());

            // verify outcome
            Assert.Equal(_fixture.RepeatCount, intensities.Count);
        }

        [Fact]
        public void GetAverageIntensityIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            // Exercise system
            var averageIntensity = images.GetAverageIntensity(zone, images.FirstDateTime(), images.LastDateTime());

            // verify outcome
            Assert.Equal(1, averageIntensity);
        }

        [Fact]
        public void GetAverageIntensityFromTimeSpanIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));
            var timespan = images.LastDateTime() - images.FirstDateTime();

            // Exercise system
            var averageIntensity = images.GetAverageIntensity(zone, timespan);

            // verify outcome
            Assert.Equal(1, averageIntensity);
        }

        [Fact]
        public void GetMinIntensityIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            // Exercise system
            var minIntensity = images.GetMinIntensity(zone, images.FirstDateTime(), images.LastDateTime());

            // verify outcome
            Assert.Equal(1, minIntensity);
        }

        [Fact]
        public void GetMaxIntensityIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            // Exercise system
            var maxIntensity = images.GetMaxIntensity(zone, images.FirstDateTime(), images.LastDateTime());

            // verify outcome
            Assert.Equal(1, maxIntensity);
        }

        [Fact]
        public void GetDepthIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));

            // Exercise system
            var depth = images.GetDepth(zone, images.FirstDateTime(), images.LastDateTime());

            // verify outcome
            var timespan = images.LastDateTime() - images.FirstDateTime();
            Assert.Equal(Math.Round(timespan.TotalHours, 0), Math.Round(depth, 0));
        }

        [Fact]
        public void GetDepthFromTimeSpanIsOk()
        {
            // Setup fixture
            var repository = new FakeRasterRepository<FakeRadarImage>(_fixture.CreateMany<FakeRadarImage>().ToList());
            var images = new RadarImageService<FakeRadarImage>(repository, TimeSpan.MaxValue, TimeSpan.MaxValue);
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.4)));
            zone.PixelWeights.Add(new PixelWeight(_fixture.Create<Pixel>(), new Weight(0.2)));
            var timespan = images.LastDateTime() - images.FirstDateTime();

            // Exercise system
            var depth = images.GetDepth(zone, timespan);

            // verify outcome
            Assert.Equal(Math.Round(timespan.TotalHours, 0), Math.Round(depth, 0));
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = RadarImageService<AsciiImage>.GetRepositoryTypes(path);

            Assert.Contains(typeof(DelimitedAsciiRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = RadarImageService<AsciiImage>.GetRepositoryTypes(path);

            Assert.Contains(typeof(DelimitedAsciiRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = RadarImageService<AsciiImage>.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(DelimitedAsciiRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = RadarImageService<AsciiImage>.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}
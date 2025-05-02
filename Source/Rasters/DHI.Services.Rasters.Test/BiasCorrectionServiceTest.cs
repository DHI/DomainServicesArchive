namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Radar;
    using Rasters;
    using Xunit;

    public class BiasCorrectionServiceTest
    {
        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new BiasCorrectionService(null));
        }

        [Theory, AutoData]
        public void GetNonExistingThrows(BiasCorrectionService biasCorrectionService)
        {
            Assert.False(biasCorrectionService.TryGet(DateTime.MinValue, out _));
        }

        [Theory, AutoData]
        public void RemoveNonExistingThrows(BiasCorrectionService biasCorrectionService)
        {
            Assert.Throws<KeyNotFoundException>(() => biasCorrectionService.Remove(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void UpdateNonExistingThrows(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            Assert.Throws<KeyNotFoundException>(() => biasCorrectionService.Update(matrix));
        }

        [Theory, AutoData]
        public void AddExistingThrows(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            biasCorrectionService.Add(matrix);

            Assert.Throws<ArgumentException>(() => biasCorrectionService.Add(matrix));
        }

        [Theory, AutoData]
        public void CreateCorrectionMatrixWithEmptyGaugesThrows(Size size, DateTime dateTime)
        {
            Assert.Throws<ArgumentException>(() => BiasCorrectionService.CreateCorrectionMatrix(size, dateTime, new List<Gauge>()));
        }

        [Theory, AutoData]
        public void CreateCorrectionMatrixWithEmptyGaugeRainDepthsThrows(Size size, DateTime dateTime)
        {
            Assert.Throws<ArgumentException>(() => BiasCorrectionService.CreateCorrectionMatrix(size, dateTime, new List<GaugeRadarDepth>()));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            biasCorrectionService.Add(matrix);
            biasCorrectionService.TryGet(matrix.Id, out var mat);
            Assert.Equal(matrix.Id, mat.Id);
        }

        [Theory, AutoData]
        public void GetLastBeforeIsOk(BiasCorrectionService biasCorrectionService)
        {
            biasCorrectionService.Add(new Matrix(new DateTime(2016, 5, 1)));
            biasCorrectionService.Add(new Matrix(new DateTime(2016, 5, 2)));
            biasCorrectionService.Add(new Matrix(new DateTime(2016, 5, 4)));

            Assert.Equal(new DateTime(2016, 5, 2), biasCorrectionService.GetLastBefore(new DateTime(2016, 5, 3)).Value.Id);
        }

        [Theory, AutoData]
        public void GetLastBeforeReturnsEmptyMaybeIfNoneFound(BiasCorrectionService biasCorrectionService)
        {
            biasCorrectionService.Add(new Matrix(DateTime.MaxValue));
            Assert.False(biasCorrectionService.GetLastBefore(DateTime.Now).HasValue);
        }

        [Theory, AutoData]
        public void EventsAreRaisedOnAdd(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            var raisedEvents = new List<string>();
            biasCorrectionService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            biasCorrectionService.Added += (s, e) => { raisedEvents.Add("Added"); };
            biasCorrectionService.Add(matrix);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoData]
        public void GetAllIsOk(BiasCorrectionService biasCorrectionService)
        {
            biasCorrectionService.Add(new Matrix(DateTime.MinValue));
            biasCorrectionService.Add(new Matrix(DateTime.MaxValue));

            Assert.Equal(2, biasCorrectionService.GetAll().Count());
        }

        [Theory, AutoData]
        public void CountIsOk(BiasCorrectionService biasCorrectionService)
        {
            biasCorrectionService.Add(new Matrix(DateTime.MinValue));
            biasCorrectionService.Add(new Matrix(DateTime.MaxValue));

            Assert.Equal(2, biasCorrectionService.Count());
        }

        [Theory, AutoData]
        public void ExistsIsOk(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            biasCorrectionService.Add(matrix);
            Assert.True(biasCorrectionService.Exists(matrix.Id));
        }

        [Theory, AutoData]
        public void DoesNotExistIsOk(BiasCorrectionService biasCorrectionService)
        {
            Assert.False(biasCorrectionService.Exists(DateTime.MinValue));
        }

        [Theory, AutoData]
        public void RemoveIsOk(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            biasCorrectionService.Add(matrix);
            biasCorrectionService.Remove(matrix.Id);

            Assert.False(biasCorrectionService.Exists(matrix.Id));
        }

        [Theory, AutoData]
        public void EventsAreRaisedOnRemove(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            var raisedEvents = new List<string>();
            biasCorrectionService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            biasCorrectionService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            biasCorrectionService.Add(matrix);

            biasCorrectionService.Remove(matrix.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoData]
        public void UpdateIsOk(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            biasCorrectionService.Add(matrix);

            var updated = new Matrix(matrix.Id) { Size = new Size(matrix.Size.Width*2, matrix.Size.Height*2) };
            biasCorrectionService.Update(updated);

            biasCorrectionService.TryGet(matrix.Id, out var mat);
            Assert.Equal(updated.Size.Height, mat.Size.Height);
        }

        [Theory, AutoData]
        public void AddOrUpdateIsOk(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            var raisedEvents = new List<string>();
            biasCorrectionService.Added += (s, e) => { raisedEvents.Add("Added"); };
            biasCorrectionService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            biasCorrectionService.Add(matrix);
            var updated = new Matrix(matrix.Id) { Size = new Size(matrix.Size.Width*2, matrix.Size.Height*2) };
            biasCorrectionService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            biasCorrectionService.TryGet(matrix.Id, out var mat);
            Assert.Equal(updated.Size.Height, mat.Size.Height);
        }

        [Theory, AutoData]
        public void EventsAreRaisedOnUpdate(BiasCorrectionService biasCorrectionService, Matrix matrix)
        {
            var raisedEvents = new List<string>();
            biasCorrectionService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            biasCorrectionService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            biasCorrectionService.Add(matrix);

            var updated = new Matrix(matrix.Id) { Size = new Size(matrix.Size.Width*2, matrix.Size.Height*2) };
            biasCorrectionService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Fact]
        public void CreateCorrectionMatrixIsOk()
        {
            var gauges = new List<Gauge> { new Gauge(new Pixel(34, 240), 100, .04),
                                           new Gauge(new Pixel(57, 36), 100, 1.48),
                                           new Gauge(new Pixel(151, 50), 100, 2.84),
                                           new Gauge(new Pixel(15, 87), 100, .2),
                                           new Gauge(new Pixel(24, 175), 100, 1.64),
                                           new Gauge(new Pixel(167, 236), 100, .16),
                                           new Gauge(new Pixel(25, 73), 100, 1.36),
                                           new Gauge(new Pixel(55, 141), 100, 3.6),
                                           new Gauge(new Pixel(204, 236), 100, 2.08),
                                           new Gauge(new Pixel(36, 39), 100, 2.2)
            };
            var correctionMatrix = BiasCorrectionService.CreateCorrectionMatrix(new Size(240, 240), DateTime.Now, gauges);
            var pointColors = new SortedDictionary<double, Color> { { 0.92, Color.Blue }, { 1.04, Color.Cyan }, { 1.16, Color.Yellow }, { 1.28, Color.Red } };
            var colorGradient = new ColorGradient(pointColors);
            var bitmap = correctionMatrix.ToBitmap(colorGradient);

            Assert.True(correctionMatrix.HasValues);
            Assert.Equal(1.274, correctionMatrix.Values.Max(), 3);
            Assert.Equal(0.913, correctionMatrix.Values.Min(), 3);
        }

        [Fact]
        public void CreateCorrectionMatrixMeanFieldBiasIsOk()
        {
            var gaugeRadarDepths = new List<GaugeRadarDepth> { new GaugeRadarDepth(21.6,  7.61),
                                                               new GaugeRadarDepth( 4.0,  4.88),
                                                               new GaugeRadarDepth(10.0,  2.45),
                                                               new GaugeRadarDepth( 4.6,  4.28),
                                                               new GaugeRadarDepth(11.2, 11.19),
                                                               new GaugeRadarDepth(19.4,  8.87),
                                                               new GaugeRadarDepth(21.0,  1.49),
                                                               new GaugeRadarDepth( 0.2,  0.21),
                                                               new GaugeRadarDepth( 6.2, 25.46),
                                                               new GaugeRadarDepth(20.2, 20.33)
            };
            var correctionMatrix = BiasCorrectionService.CreateCorrectionMatrix(new Size(240, 240), DateTime.Now, gaugeRadarDepths);
            var pointColors = new SortedDictionary<double, Color> { { 0.92, Color.Blue }, { 1.04, Color.Cyan }, { 1.16, Color.Yellow }, { 1.28, Color.Red } };
            var colorGradient = new ColorGradient(pointColors);
            var bitmap = correctionMatrix.ToBitmap(colorGradient);

            Assert.True(correctionMatrix.HasValues);
            Assert.Equal(correctionMatrix.Values.Max(), correctionMatrix.Values.Min(), 3);
            Assert.Equal(1.365, correctionMatrix.Values.Max(), 3);
        }
    }
}
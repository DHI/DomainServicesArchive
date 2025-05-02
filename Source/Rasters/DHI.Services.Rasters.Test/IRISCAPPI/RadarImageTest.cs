namespace DHI.Services.Rasters.Test.IRISCAPPI
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Radar;
    using Radar.IRISCAPPI;
    using Rasters;
    using Xunit;

    public class RadarImageTest
    {
        private readonly IFixture _fixture = new Fixture();

        public RadarImageTest()
        {
            _fixture.Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new RandomNumericSequenceGenerator(0, 1000));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        }

        [Fact]
        public void GetValueFromNonExistingPixelThrows()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().OmitAutoProperties().Create();
            var pixel = _fixture.Create<Pixel>();

            // Exercise system and verify outcome
            Assert.Throws<ArgumentOutOfRangeException>(() => image.GetValue(pixel));
        }

        [Fact]
        public void ToIntensityOfIntensityThrows()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().With(i => i.PixelValueType, PixelValueType.Intensity).Without(i => i.Size).Without(i => i.PixelSize).Create();

            // Exercise system and verify outcome
            Assert.Throws<Exception>(() => image.ToIntensity());
        }

        [Fact]
        public void CreateFromNonExistingFileThrows()
        {
            // Exercise system and verify outcome
            Assert.Throws<FileNotFoundException>(() => RadarImage.CreateNew("NonExistingFile"));
        }

        [Fact]
        public void CorrectWithEmptyCorrectionMatrixThrows()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().Create();
            var correctionMatrix = _fixture.Create<Matrix>();

            // Exercise system and verify outcome
            Assert.Throws<ArgumentException>(() => image.Correct(correctionMatrix));
        }

        [Fact]
        public void CorrectWithInvalidCorrectionMatrixSizeThrows()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().Create();
            var correctionMatrix = _fixture.Create<Matrix>();
            correctionMatrix.Size = new Size(image.Size.Width + 1, image.Size.Height + 1);
            _fixture.AddManyTo(correctionMatrix.Values, correctionMatrix.Size.Width * correctionMatrix.Size.Height);

            // Exercise system and verify outcome
            Assert.Throws<ArgumentException>(() => image.Correct(correctionMatrix));
        }

        [Fact]
        public void CorrectEmptyImageThrows()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().Create();
            var correctionMatrix = _fixture.Create<Matrix>();
            correctionMatrix.Size = new Size(image.Size.Width, image.Size.Height);
            _fixture.AddManyTo(correctionMatrix.Values, correctionMatrix.Size.Width * correctionMatrix.Size.Height);

            // Exercise system and verify outcome
            Assert.Throws<Exception>(() => image.Correct(correctionMatrix));
        }

        [Fact]
        public void CreateFromFileIsOk()
        {
            // Setup fixture and exercise system
            const string fname = @"..\..\..\Data\MR1160304030003.CAPHHC6";
            var image = RadarImage.CreateNew(fname);
            
            var bmp=image.ToBitmap();
            var outPath = Path.Combine(Path.GetDirectoryName(fname), $"{Path.GetFileNameWithoutExtension(fname)}{Path.GetExtension(fname)}.png");

            bmp.Save(outPath);

            // Verify outcome
            Assert.Equal(PixelValueType.Reflectivity, image.PixelValueType);
            Assert.Equal(RadarImageType.Observation, image.Type);
            Assert.Equal(1251, image.PixelSize.Width);
            Assert.Equal(1251, image.PixelSize.Height);
            Assert.Equal(new DateTime(2016, 3, 4, 3, 0, 3), image.DateTime);
            Assert.Equal(Path.GetFileName(fname), image.Name);
            Assert.Equal(480, image.Size.Width);
            Assert.Equal(480, image.Size.Height);
            Assert.Equal(0.0, image.TimeOfForecastOffset);
            Assert.Equal("dBZ", image.PixelValueUnit);
        }


        [Fact]
        public void HasValuesIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome
            Assert.True(image.HasValues);
        }

        [Fact]
        public void DoesNotHaveValuesIsOk()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().OmitAutoProperties().Create();

            // Exercise system and verify outcome
            Assert.False(image.HasValues);
        }

        [Fact]
        public void MaxValueIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome
            Assert.True(image.MaxValue >= image.MinValue);
        }

        [Fact]
        public void MinValueIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome
            Assert.True(image.MinValue <= image.MaxValue);
        }

        [Fact]
        public void MinPositiveValueIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome
            Assert.True(image.MinPositiveValue <= image.MaxValue && image.MinPositiveValue > 0);
        }

        [Fact]
        public void GetValueIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome (samples)
            image.GetGeoCoordinates(new Pixel(200, 100));

            Assert.Equal(10.5,image.GetValue(new Pixel(238, 240)));
            Assert.Equal(-8.0,image.GetValue(new Pixel(238, 245)));
            Assert.Equal(-12.00,image.GetValue(new Pixel(234, 245)) );
            Assert.Equal(-32.0,image.GetValue(new Pixel(239, 245)));
            //Assert.True(image.GetValue(new Pixel(34, 130)) > 0);

        }

        [Fact]
        public void ToBitmapIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome
            var bitmap = image.ToBitmap();
            Assert.IsType<Bitmap>(bitmap);
        }

        [Fact]
        public void GetIntensityIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);

            // Exercise system and verify outcome (samples)
            // TODO : Implement
            //Assert.Equal(0.2875271, Math.Round(image.GetIntensity(new Pixel(37, 61)) / 3.6, 7));
            //Assert.Equal(0.2773663, Math.Round(image.GetIntensity(new Pixel(179, 34)) / 3.6, 7));
            //Assert.Equal(70.681, Math.Round(image.GetIntensity(new Pixel(156, 114)) / 3.6, 3));
        }

        [Fact]
        public void GetIntensityOverloadIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);
            var coefficients = ConversionCoefficients.Default;
            coefficients.RainIntensityUnit = RainIntensityUnit.MicroMetersPerSecond;

            // Exercise system and verify outcome (samples)
            // TODO : Implement
            //Assert.Equal(0.2875271, Math.Round(image.GetIntensity(new Pixel(37, 61), coefficients), 7));
            //Assert.Equal(0.2773663, Math.Round(image.GetIntensity(new Pixel(179, 34), coefficients), 7));
            //Assert.Equal(70.681, Math.Round(image.GetIntensity(new Pixel(156, 114), coefficients), 3));
        }

        [Fact]
        public void GetGeoCoordinatesIsOk()
        {
            // Setup fixture
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.MR1160304030003.CAPHHC6");
            var image = RadarImage.CreateNew(stream);
            image.GeoCenter = new PointF(1f, 0f);

            // Exercise and verify image upper left
            var imageUpperLeft = new Pixel(1, 1);
            var pixelLowerLeft = image.GetGeoCoordinates(imageUpperLeft)[0];
            var pixelUpperLeft = image.GetGeoCoordinates(imageUpperLeft)[1];
            var pixelUpperRight = image.GetGeoCoordinates(imageUpperLeft)[2];
            var pixelLowerRight = image.GetGeoCoordinates(imageUpperLeft)[3];
            Assert.Equal(new PointF(-300240f + 1, 300240f - image.PixelSize.Height), pixelLowerLeft);
            Assert.Equal(new PointF(-300240f + 1, 300240f), pixelUpperLeft);
            Assert.Equal(new PointF(-300240f + image.PixelSize.Width + 1, 300240f), pixelUpperRight);
            Assert.Equal(new PointF(-300240f + image.PixelSize.Width + 1, 300240f - image.PixelSize.Height), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageUpperLeft)[4]);

            // Exercise and verify image upper right
            var imageUpperRight = new Pixel(image.Size.Width, 1);
            pixelLowerLeft = image.GetGeoCoordinates(imageUpperRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageUpperRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageUpperRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageUpperRight)[3];
            Assert.Equal(new PointF(300240f - image.PixelSize.Width + 1, 300240f - image.PixelSize.Height), pixelLowerLeft);
            Assert.Equal(new PointF(300240f - image.PixelSize.Width + 1, 300240f), pixelUpperLeft);
            Assert.Equal(new PointF(300240f + 1, 300240f), pixelUpperRight);
            Assert.Equal(new PointF(300240f + 1, 300240f - image.PixelSize.Height), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageUpperRight)[4]);

            // Exercise and verify image lower right
            var imageLowerRight = new Pixel(image.Size.Width, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerRight)[3];
            Assert.Equal(new PointF(300240f - image.PixelSize.Width + 1, -300240f), pixelLowerLeft);
            Assert.Equal(new PointF(300240f - image.PixelSize.Width + 1, -300240f + image.PixelSize.Height), pixelUpperLeft);
            Assert.Equal(new PointF(300240f + 1, -300240f + image.PixelSize.Height), pixelUpperRight);
            Assert.Equal(new PointF(300240f + 1, -300240f), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageLowerRight)[4]);

            // Exercise and verify image lower right
            var imageLowerLeft = new Pixel(1, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerLeft)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerLeft)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerLeft)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerLeft)[3];
            Assert.Equal(new PointF(-300240f + 1, -300240f), pixelLowerLeft);
            Assert.Equal(new PointF(-300240f + 1, -300240f + image.PixelSize.Height), pixelUpperLeft);
            Assert.Equal(new PointF(-300240f + image.PixelSize.Width + 1, -300240f + image.PixelSize.Height), pixelUpperRight);
            Assert.Equal(new PointF(-300240f + image.PixelSize.Width + 1, -300240f), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageLowerLeft)[4]);
        }

        [Fact]
        public void CorrectIsOk()
        {
            // Setup fixture
            var image = _fixture.Build<RadarImage>().Create();
            _fixture.AddManyTo(image.Values, image.Size.Width * image.Size.Height);
            var correctionMatrix = _fixture.Create<Matrix>();
            correctionMatrix.Size = new Size(image.Size.Width, image.Size.Height);
            _fixture.AddManyTo(correctionMatrix.Values, correctionMatrix.Size.Width * correctionMatrix.Size.Height);

            // Exercise system
            var pixel = new Pixel(1, 1);
            var before = image.GetValue(pixel);
            image.Correct(correctionMatrix);

            // Verify outcome
            Assert.Equal(before * correctionMatrix.GetValue(pixel), image.GetValue(pixel), 5);
        }
    }
}
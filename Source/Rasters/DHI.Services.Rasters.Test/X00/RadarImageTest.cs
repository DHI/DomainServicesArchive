namespace DHI.Services.Rasters.Test.X00
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Radar;
    using Radar.X00;
    using Rasters;
    using Xunit;

    public class RadarImageTest
    {
        private readonly IFixture _fixture;

        public RadarImageTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Customizations.Add(new RandomNumericSequenceGenerator(0, 255));
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
        public void GetGeoCoordinatesWithGeoCenterEmptyThrows()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

            // Exercise system and verify outcome
            Assert.Throws<Exception>(() => image.GetGeoCoordinates(new Pixel(1, 1)));
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
            var image = RadarImage.CreateNew(@"..\..\..\Data\AROS1245.p00");

            // Verify outcome
            Assert.Equal(PixelValueType.Reflectivity, image.PixelValueType);
            Assert.Equal(RadarImageType.Observation, image.Type);
            Assert.Equal(500, image.PixelSize.Width);
            Assert.Equal(500, image.PixelSize.Height);
            Assert.Equal(new DateTime(2013, 7, 30, 12, 45, 0), image.DateTime);
            Assert.Equal("LAWR Radar DHI", image.Name);
            Assert.Equal(240, image.Size.Width);
            Assert.Equal(240, image.Size.Height);
            Assert.Equal(0.0, image.TimeOfForecastOffset);
            Assert.Equal("dBZ", image.PixelValueUnit);
        }

        [Fact]
        public void MetadataIsSetCorrectly()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");

            // Exercise system
            var image = RadarImage.CreateNew(x00Stream);

            // Verify outcome
            Assert.Equal(PixelValueType.Reflectivity, image.PixelValueType);
            Assert.Equal(RadarImageType.Observation, image.Type);
            Assert.Equal(500, image.PixelSize.Width);
            Assert.Equal(500, image.PixelSize.Height);
            Assert.Equal(new DateTime(2013, 7, 30, 12, 45, 0), image.DateTime);
            Assert.Equal("LAWR Radar DHI", image.Name);
            Assert.Equal(240, image.Size.Width);
            Assert.Equal(240, image.Size.Height);
            Assert.Equal(0.0, image.TimeOfForecastOffset);
            Assert.Equal("dBZ", image.PixelValueUnit);
        }

        [Fact]
        public void HasValuesIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

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
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

            // Exercise system and verify outcome
            Assert.True(image.MaxValue >= image.MinValue);
        }

        [Fact]
        public void MinValueIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

            // Exercise system and verify outcome
            Assert.True(image.MinValue <= image.MaxValue);
        }

        [Fact]
        public void MinPositiveValueIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

            // Exercise system and verify outcome
            Assert.True(image.MinPositiveValue <= image.MaxValue && image.MinPositiveValue > 0);
        }

        [Fact]
        public void GetByteValueIsOk()
        {
            // Setup fixture
            var x00ByteStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00ByteStream);

            // Exercise system and verify outcome (samples)
            // (col, row) is equivalent to (col-1, 240-row) in dfs2-coordinates
            Assert.True(image.GetValue(new Pixel(37, 61)) > 0);
            Assert.True(image.GetValue(new Pixel(179, 34)) > 0);
            Assert.True(image.GetValue(new Pixel(35, 64)) > 0);
            Assert.True(image.GetValue(new Pixel(36, 64)) > 0);
            Assert.True(image.GetValue(new Pixel(34, 130)) > 0);
            var value = image.GetValue(new Pixel(156, 114));
            Assert.True(value > 0);

            Assert.Equal(0, image.GetValue(new Pixel(37, 64)));
            Assert.Equal(0, image.GetValue(new Pixel(35, 130)));
            Assert.Equal(0, image.GetValue(new Pixel(1, 1)));
            Assert.Equal(0, image.GetValue(new Pixel(240, 240)));
        }

        [Fact]
        public void GetDoubleValueIsOk()
        {
            // Setup fixture
            var x00DoubleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.i00");
            var image = RadarImage.CreateNew(x00DoubleStream);

            // Exercise system and verify outcome (samples)
            Assert.True(image.GetValue(new Pixel(37, 61)) > 0);
            Assert.True(image.GetValue(new Pixel(179, 34)) > 0);
            Assert.True(image.GetValue(new Pixel(35, 64)) > 0);
            Assert.True(image.GetValue(new Pixel(36, 64)) > 0);
            Assert.True(image.GetValue(new Pixel(34, 130)) > 0);
            var value = image.GetValue(new Pixel(156, 114));
            Assert.True(value > 0);

            Assert.Equal(0, image.GetValue(new Pixel(37, 64)));
            Assert.Equal(0, image.GetValue(new Pixel(35, 130)));
            Assert.Equal(0, image.GetValue(new Pixel(1, 1)));
            Assert.Equal(0, image.GetValue(new Pixel(240, 240)));
        }

        [Fact]
        public void ToBitmapIsOk()
        {
            // Setup fixture
            var x00DoubleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.i00");
            var image = RadarImage.CreateNew(x00DoubleStream);

            // Exercise system and verify outcome
            var bitmap = image.ToBitmap();
            Assert.IsType<Bitmap>(bitmap);
        }

        [Fact]
        public void GetIntensityIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);

            // Exercise system and verify outcome (samples)
            Assert.Equal(0.2875271, Math.Round(image.GetIntensity(new Pixel(37, 61)) / 3.6, 7));
            Assert.Equal(0.2773663, Math.Round(image.GetIntensity(new Pixel(179, 34)) / 3.6, 7));
            Assert.Equal(70.681, Math.Round(image.GetIntensity(new Pixel(156, 114)) / 3.6, 3));
        }

        [Fact]
        public void GetIntensityOverloadIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);
            var coefficients = ConversionCoefficients.Default;
            coefficients.RainIntensityUnit = RainIntensityUnit.MicroMetersPerSecond;

            // Exercise system and verify outcome (samples)
            Assert.Equal(0.2875271, Math.Round(image.GetIntensity(new Pixel(37, 61), coefficients), 7));
            Assert.Equal(0.2773663, Math.Round(image.GetIntensity(new Pixel(179, 34), coefficients), 7));
            Assert.Equal(70.681, Math.Round(image.GetIntensity(new Pixel(156, 114), coefficients), 3));
        }

        [Fact]
        public void GetGeoCoordinatesIsOk()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00Stream);
            image.GeoCenter = new PointF(1f, 0f);

            // Exercise and verify image upper left
            var imageUpperLeft = new Pixel(1, 1);
            var pixelLowerLeft = image.GetGeoCoordinates(imageUpperLeft)[0];
            var pixelUpperLeft = image.GetGeoCoordinates(imageUpperLeft)[1];
            var pixelUpperRight = image.GetGeoCoordinates(imageUpperLeft)[2];
            var pixelLowerRight = image.GetGeoCoordinates(imageUpperLeft)[3];
            Assert.Equal(new PointF(-60000f + 1, 60000f - image.PixelSize.Height), pixelLowerLeft);
            Assert.Equal(new PointF(-60000f + 1, 60000f), pixelUpperLeft);
            Assert.Equal(new PointF(-60000f + image.PixelSize.Width + 1, 60000f), pixelUpperRight);
            Assert.Equal(new PointF(-60000f + image.PixelSize.Width + 1, 60000f - image.PixelSize.Height), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageUpperLeft)[4]);

            // Exercise and verify image upper right
            var imageUpperRight = new Pixel(image.Size.Width, 1);
            pixelLowerLeft = image.GetGeoCoordinates(imageUpperRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageUpperRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageUpperRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageUpperRight)[3];
            Assert.Equal(new PointF(60000f - image.PixelSize.Width + 1, 60000f - image.PixelSize.Height), pixelLowerLeft);
            Assert.Equal(new PointF(60000f - image.PixelSize.Width + 1, 60000f), pixelUpperLeft);
            Assert.Equal(new PointF(60000f + 1, 60000f), pixelUpperRight);
            Assert.Equal(new PointF(60000f + 1, 60000f - image.PixelSize.Height), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageUpperRight)[4]);

            // Exercise and verify image lower right
            var imageLowerRight = new Pixel(image.Size.Width, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerRight)[3];
            Assert.Equal(new PointF(60000f - image.PixelSize.Width + 1, -60000f), pixelLowerLeft);
            Assert.Equal(new PointF(60000f - image.PixelSize.Width + 1, -60000f + image.PixelSize.Height), pixelUpperLeft);
            Assert.Equal(new PointF(60000f + 1, -60000f + image.PixelSize.Height), pixelUpperRight);
            Assert.Equal(new PointF(60000f + 1, -60000f), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageLowerRight)[4]);

            // Exercise and verify image lower right
            var imageLowerLeft = new Pixel(1, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerLeft)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerLeft)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerLeft)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerLeft)[3];
            Assert.Equal(new PointF(-60000f + 1, -60000f), pixelLowerLeft);
            Assert.Equal(new PointF(-60000f + 1, -60000f + image.PixelSize.Height), pixelUpperLeft);
            Assert.Equal(new PointF(-60000f + image.PixelSize.Width + 1, -60000f + image.PixelSize.Height), pixelUpperRight);
            Assert.Equal(new PointF(-60000f + image.PixelSize.Width + 1, -60000f), pixelLowerRight);
            Assert.Equal(pixelLowerLeft, image.GetGeoCoordinates(imageLowerLeft)[4]);
        }

        [Fact]
        public void ToIntensityIsOk()
        {
            // Setup fixture
            var x00DoubleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");
            var image = RadarImage.CreateNew(x00DoubleStream);

            // Exercise system
            var intensityImage = image.ToIntensity();

            // Verify pixel values (samples)
            var expected = Funcs.ReflectivityToIntensity(image.GetValue(new Pixel(37, 61)));
            var actual = intensityImage.GetValue(new Pixel(37, 61));
            Assert.Equal(expected, actual);
            expected = Funcs.ReflectivityToIntensity(image.GetValue(new Pixel(179, 34)));
            actual = intensityImage.GetValue(new Pixel(179, 34));
            Assert.Equal(expected, actual);

            // Verify 0 (or less) reflectivity equals 0 intensity (samples)
            Assert.Equal(0, intensityImage.GetValue(new Pixel(37, 64)));
            Assert.Equal(0, intensityImage.GetValue(new Pixel(35, 130)));

            // Verify pixel value type
            Assert.Equal(PixelValueType.Intensity, intensityImage.PixelValueType);

            // Verify pixel value unit. As ToIntensity was called without ConversionCoefficient arguments,
            // the expected unit for intensity is the default, millimeter per hour "mm/h"
            Assert.Equal(intensityImage.PixelValueUnit, RainIntensityUnit.MilliMetersPerHour.ToString());

            // Verify Geo information properties
            Assert.Equal(intensityImage.GeoCenter, image.GeoCenter);
            Assert.Equal(intensityImage.GeoProjectionString, image.GeoProjectionString);
            Assert.Equal(intensityImage.GeoLowerLeft, image.GeoLowerLeft);
            Assert.Equal(intensityImage.GeoUpperLeft, image.GeoUpperLeft);
            Assert.Equal(intensityImage.GeoUpperRight, image.GeoUpperRight);
            Assert.Equal(intensityImage.GeoUpperLeft, image.GeoUpperLeft);

            // Verify other image properties
            Assert.Equal(intensityImage.DateTime, image.DateTime);
            Assert.Equal(intensityImage.Id, image.Id);
            Assert.Equal(intensityImage.Name, image.Name);
            Assert.Equal(intensityImage.PixelSize, image.PixelSize);
            Assert.Equal(intensityImage.Size, image.Size);
            Assert.Equal(intensityImage.TimeOfForecastOffset, image.TimeOfForecastOffset);
            Assert.Equal(intensityImage.Type, image.Type);
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
namespace DHI.Services.Rasters.Test.ESRIASCII
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Xunit;
    using Radar.ESRIASCII;
    using Rasters;

    public class EsriAsciiTest
    {
        private readonly IFixture _fixture = new Fixture();

        public EsriAsciiTest()
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
            var image = _fixture.Build<AsciiImage>().OmitAutoProperties().Create();
            var pixel = _fixture.Create<Pixel>();

            // Exercise system and verify outcome
            Assert.Throws<ArgumentOutOfRangeException>(() => image.GetValue(pixel));
        }

        [Fact]
        public void CreateFromNonExistingFileThrows()
        {
            // Exercise system and verify outcome
            Assert.Throws<FileNotFoundException>(() => AsciiImage.CreateNew("NonExistingFile"));
        }

        [Fact]
        public void GetGeoCoordinatesWithGeoCenterEmptyThrows()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.Throws<Exception>(() => image.GetGeoCoordinates(new Pixel(1, 1)));
        }

        [Fact]
        public void CreateFromFileIsOk()
        {
            // Setup fixture and exercise system
            const string fname = @"..\..\..\Data\NAWABS_Rainfall_20180315_1245.asc";
            var image = AsciiImage.CreateNew(fname);

            // Verify outcome
            Assert.Equal(DateTime.MinValue, image.DateTime);
            Assert.Equal(Path.GetFileName(fname), image.Name);
            Assert.Equal(868, image.Size.Width);
            Assert.Equal(708, image.Size.Height);
        }

        [Fact]
        public void HasValuesIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.HasValues);
        }

        [Fact]
        public void DoesNotHaveValuesIsOk()
        {
            // Setup fixture
            var image = _fixture.Build<AsciiImage>().OmitAutoProperties().Create();

            // Exercise system and verify outcome
            Assert.False(image.HasValues);
        }

        [Fact]
        public void MaxValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MaxValue >= image.MinValue);
        }

        [Fact]
        public void MinValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MinValue <= image.MaxValue);
        }

        [Fact]
        public void MinPositiveValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MinPositiveValue <= image.MaxValue && image.MinPositiveValue > 0);
        }

        [Fact]
        public void GetDoubleValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome (samples)
            Assert.True(image.GetValue(new Pixel(4, 372)) > 0);
            var value = image.GetValue(new Pixel(7, 373));
            Assert.True(value > 0);

            Assert.Equal(0, image.GetValue(new Pixel(5, 373)));
        }

        [Fact]
        public void ToBitmapIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            var bitmap = image.ToBitmap();
            Assert.IsType<Bitmap>(bitmap);
        }

        [Fact]
        public void GetGeoCoordinatesIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.NAWABS_Rainfall_20180315_1245.asc");
            var image = AsciiImage.CreateNew(imageStream);

            image.GeoCenter = new PointF(152.96575f, -27.48125f);

            // Exercise and verify image upper left
            var imageUpperLeft = new Pixel(1, 1);
            var pixelLowerLeft = image.GetGeoCoordinates(imageUpperLeft)[0];
            var pixelUpperLeft = image.GetGeoCoordinates(imageUpperLeft)[1];
            var pixelUpperRight = image.GetGeoCoordinates(imageUpperLeft)[2];
            var pixelLowerRight = image.GetGeoCoordinates(imageUpperLeft)[3];
            Assert.Equal(new PointF(152.74875f, -27.30425f), pixelUpperLeft);
            Assert.Equal(new PointF(152.74875f, -27.30475f), pixelLowerLeft);
            Assert.Equal(new PointF(152.74925f, -27.30425f), pixelUpperRight);
            Assert.Equal(new PointF(152.74925f, -27.30475f), pixelLowerRight);

            // Exercise and verify image upper right
            var imageUpperRight = new Pixel(image.Size.Width, 1);
            pixelLowerLeft = image.GetGeoCoordinates(imageUpperRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageUpperRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageUpperRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageUpperRight)[3];
            Assert.Equal(new PointF(153.18225f, -27.30425f), pixelUpperLeft);
            Assert.Equal(new PointF(153.18225f, -27.30475f), pixelLowerLeft);
            Assert.Equal(new PointF(153.18275f, -27.30425f), pixelUpperRight);
            Assert.Equal(new PointF(153.18275f, -27.30475f), pixelLowerRight);

            // Exercise and verify image lower right
            var imageLowerRight = new Pixel(image.Size.Width, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerRight)[3];
            Assert.Equal(new PointF(153.182251f, -27.6577511f), pixelUpperLeft);
            Assert.Equal(new PointF(153.182251f, -27.65825f), pixelLowerLeft);
            Assert.Equal(new PointF(153.18275f, -27.6577511f), pixelUpperRight);
            Assert.Equal(new PointF(153.18275f, -27.65825f), pixelLowerRight);

            // Exercise and verify image lower left
            var imageLowerLeft = new Pixel(1, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerLeft)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerLeft)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerLeft)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerLeft)[3];
            Assert.Equal(new PointF(152.74875f, -27.6577511f), pixelUpperLeft);
            Assert.Equal(new PointF(152.74875f, -27.65825f), pixelLowerLeft);
            Assert.Equal(new PointF(152.74925f, -27.6577511f), pixelUpperRight);
            Assert.Equal(new PointF(152.74925f, -27.65825f), pixelLowerRight);
        }
    }
}
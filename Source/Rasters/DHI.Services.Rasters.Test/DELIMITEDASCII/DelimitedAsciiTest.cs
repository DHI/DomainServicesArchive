namespace DHI.Services.Rasters.Test.DELIMITEDASCII
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Xunit;
    using Radar.DELIMITEDASCII;
    using Rasters;

    public class DelimitedAsciiTest
    {
        private readonly IFixture _fixture = new Fixture();

        public DelimitedAsciiTest()
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
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.Throws<Exception>(() => image.GetGeoCoordinates(new Pixel(1, 1)));
        }

        [Fact]
        public void CreateFromFileIsOk()
        {
            // Setup fixture and exercise system
            const string fname = @"..\..\..\Data\2018032012_001.csv";
            var image = AsciiImage.CreateNew(fname);

            // Verify outcome
            Assert.Equal(DateTime.MinValue, image.DateTime);
            Assert.Equal(Path.GetFileName(fname), image.Name);
            Assert.Equal(2196, image.Size.Width);
            Assert.Equal(771, image.Size.Height);
        }

        [Fact]
        public void HasValuesIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
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
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MaxValue >= image.MinValue);
        }

        [Fact]
        public void MinValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MinValue <= image.MaxValue);
        }

        [Fact]
        public void MinPositiveValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            Assert.True(image.MinPositiveValue <= image.MaxValue && image.MinPositiveValue > 0);
        }

        [Fact]
        public void GetDoubleValueIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome (samples)
            Assert.True(image.GetValue(new Pixel(4, 372)) == 0);
            var value = image.GetValue(new Pixel(7, 373));
            Assert.True(value == 0);

            Assert.Equal(0, image.GetValue(new Pixel(5, 373)));
        }

        [Fact]
        public void ToBitmapIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // Exercise system and verify outcome
            var bitmap = image.ToBitmap();
            Assert.IsType<Bitmap>(bitmap);
        }

        [Fact]
        public void GetGeoCoordinatesIsOk()
        {
            // Setup fixture
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.2018032012_001.csv");
            var image = AsciiImage.CreateNew(imageStream);

            // geocenter and pixelsize will need to be set from outside
            image.GeoCenter = new PointF(109.4343f, 3.98805f);
            image.PixelSize = new SizeF(0.009f, 0.009f);

            // Exercise and verify image upper left
            var imageUpperLeft = new Pixel(1, 1);
            var pixelLowerLeft = image.GetGeoCoordinates(imageUpperLeft)[0];
            var pixelUpperLeft = image.GetGeoCoordinates(imageUpperLeft)[1];
            var pixelUpperRight = image.GetGeoCoordinates(imageUpperLeft)[2];
            var pixelLowerRight = image.GetGeoCoordinates(imageUpperLeft)[3];
            Assert.Equal(99.55231f, pixelUpperLeft.X, 6);
            Assert.Equal(7.45304966f, pixelUpperLeft.Y, 6);
            Assert.Equal(99.55231f, pixelLowerLeft.X, 6);
            Assert.Equal(7.44405f, pixelLowerLeft.Y, 6);
            Assert.Equal(99.56131f, pixelUpperRight.X, 6);
            Assert.Equal(7.45304966f, pixelUpperRight.Y, 6);
            Assert.Equal(99.56131f, pixelLowerRight.X, 6);
            Assert.Equal(7.44405f, pixelLowerRight.Y, 6);

            // Exercise and verify image lower right
            var imageLowerRight = new Pixel(image.Size.Width, image.Size.Height);
            pixelLowerLeft = image.GetGeoCoordinates(imageLowerRight)[0];
            pixelUpperLeft = image.GetGeoCoordinates(imageLowerRight)[1];
            pixelUpperRight = image.GetGeoCoordinates(imageLowerRight)[2];
            pixelLowerRight = image.GetGeoCoordinates(imageLowerRight)[3];
            Assert.Equal(119.307304f, pixelUpperLeft.X, 6);
            Assert.Equal(0.5230501f, pixelUpperLeft.Y, 6);
            Assert.Equal(119.307304f, pixelLowerLeft.X, 6);
            Assert.Equal(0.5140501f, pixelLowerLeft.Y, 6);
            Assert.Equal(119.316307f, pixelUpperRight.X, 6);
            Assert.Equal(0.5230501f, pixelUpperRight.Y, 6);
            Assert.Equal(119.316307f, pixelLowerRight.X, 6);
            Assert.Equal(0.5140501f, pixelLowerRight.Y, 6);
        }
    }
}
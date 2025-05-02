namespace DHI.Services.Rasters.Test.X00
{
    using System;
    using System.Reflection;
    using Radar;
    using Radar.X00;
    using Xunit;

    public class RadarImageHeaderTest
    {
        [Fact]
        public void ObservationHeaderIsParsedCorrectly()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS1245.p00");

            // Exercise system
            var x00Header = new RadarImageHeader(x00Stream.ToByteArray());

            // Verify outcome
            Assert.Equal("X", x00Header.Signal);
            Assert.Equal(63, x00Header.TotalBytes);
            Assert.Equal("O", x00Header.ImageType);
            Assert.Equal(500, x00Header.PixelSize);
            Assert.Equal(new DateTime(2013, 7, 30, 12, 45, 0), x00Header.DateTime);
            Assert.Equal("LAWR Radar DHI", x00Header.RadarName);
            Assert.Equal(240, x00Header.EastUppb);
            Assert.Equal(240, x00Header.NorthUppb);
            Assert.Equal(0.0, x00Header.MmPredict);
            Assert.Equal(0.25, x00Header.StoreSlope);
            Assert.Equal(0.0, x00Header.StoreOffset);
            Assert.Equal(0.0, x00Header.StoreOrd);
            Assert.Equal("dBZ", x00Header.StoreQuantity);
        }

        [Fact]
        public void ForecastHeaderIsParsedCorrectly()
        {
            // Setup fixture
            var x00Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DHI.Services.Rasters.Test.Data.AROS.f15");

            // Exercise system
            var x00Header = new RadarImageHeader(x00Stream.ToByteArray());

            // Verify outcome
            Assert.Equal("X", x00Header.Signal);
            Assert.Equal(63, x00Header.TotalBytes);
            Assert.Equal("P", x00Header.ImageType);
            Assert.Equal(500, x00Header.PixelSize);
            Assert.Equal(new DateTime(2013, 11, 19, 14, 10, 0), x00Header.DateTime);
            Assert.Equal("AROS", x00Header.RadarName);
            Assert.Equal(240, x00Header.EastUppb);
            Assert.Equal(240, x00Header.NorthUppb);
            Assert.Equal(15.0, x00Header.MmPredict);
            Assert.Equal(0.25, x00Header.StoreSlope);
            Assert.Equal(0.0, x00Header.StoreOffset);
            Assert.Equal(0.0, x00Header.StoreOrd);
            Assert.Equal("dBZ", x00Header.StoreQuantity);
        }
    }
}
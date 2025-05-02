namespace DHI.Services.GIS.Test
{
    using System.Linq;
    using Maps;
    using Xunit;
    using SkiaSharp;

    public class MapStyleTest
    {
        [Fact]
        public void GetPaletteIsOk()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") {StyleCode = "0:Red|5:Yellow|10:Green"};
            var palette = mapStyle.GetPalette();
            Assert.Equal(3, palette.Count);
            Assert.False(palette[0].HasLowerBand());
            Assert.True(palette[0].HasUpperBand());
            Assert.True(palette[10].HasLowerBand());
            Assert.False(palette[10].HasUpperBand());
            Assert.Equal(10, palette[5].UpperBandValue);
            Assert.Equal("10.0", palette[10].BandText);
            Assert.Equal(SKColors.Red, palette[0].BandColor);
            Assert.Equal(SKColors.Yellow, palette[5].BandColor);
        }

        [Fact]
        public void PaletteIsSorted()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") { StyleCode = "5:Yellow|0:Red|10:Green" };
            var palette = mapStyle.GetPalette();
            Assert.Equal(3, palette.Count);
            Assert.False(palette[0].HasLowerBand());
            Assert.True(palette[0].HasUpperBand());
            Assert.True(palette[10].HasLowerBand());
            Assert.False(palette[10].HasUpperBand());
            Assert.Equal(10, palette[5].UpperBandValue);
            Assert.Equal("10.0", palette[10].BandText);
            Assert.Equal(SKColors.Red, palette[0].BandColor);
            Assert.Equal(SKColors.Yellow, palette[5].BandColor);
        }

        [Fact]
        public void ToBitmapHorizontalIsOk()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") {StyleCode = "0:Red|5:Yellow|10:Green"};
            var image = mapStyle.ToBitmapHorizontal(100, 10);
            Assert.Equal(100, image.Width);
        }

        [Fact]
        public void ToBitmapVerticalIsOk()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") {StyleCode = "0:Red|5:Yellow|10:Green"};
            var image = mapStyle.ToBitmapVertical(10, 100);
            Assert.Equal(10, image.Width);
        }

        [Fact]
        public void ThresholdValuesIsOk()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") { StyleCode = "0:Red|5:Orange|10:Yellow|15:Green" };
            var thresholdValues = mapStyle.ThresholdValues.ToArray();
            Assert.Equal(4, thresholdValues.Count());
            Assert.Equal(new double[] {0, 5, 10, 15}, thresholdValues);
        }

        [Fact]
        public void GetColorIsOk()
        {
            var mapStyle = new MapStyle("myStyle", "MyStyle") { StyleCode = "0:Red|5:Orange|10:Yellow|15:Green" };
            
            Assert.Equal(SKColors.Transparent, mapStyle.GetColor(-99));
            Assert.Equal(SKColors.Red, mapStyle.GetColor(0));
            Assert.Equal(SKColors.Red, mapStyle.GetColor(1.5));
            Assert.Equal(SKColors.Orange, mapStyle.GetColor(5));
            Assert.Equal(SKColors.Orange, mapStyle.GetColor(8.4));
            Assert.Equal(SKColors.Yellow, mapStyle.GetColor(12));
            Assert.Equal(SKColors.Green, mapStyle.GetColor(15));
            Assert.Equal(SKColors.Green, mapStyle.GetColor(99));
        }
    }
}
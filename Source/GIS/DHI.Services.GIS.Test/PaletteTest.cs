namespace DHI.Services.GIS.Test
{
    using System;
    using System.Linq;
    using Maps;
    using SkiaSharp;
    using Xunit;

    public class PaletteTest
    {
        [Theory]
        [InlineData("IllegalStyleCode")]
        [InlineData("0:Red;5:Yellow;10:Green")]
        public void IllegalStyleCodeThrows(string code)
        {
            Assert.Throws<Exception>(() => new Palette(code));
        }

        [Fact]
        public void CreationIsOk()
        {
            var palette = new Palette("0:Red|5:Yellow|10:Green");
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
        public void SortingIsOk()
        {
            var palette = new Palette("5:Yellow|0:Red|10:Green");
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
        public void ThresholdValuesIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green");
            var thresholdValues = palette.ThresholdValues.ToArray();
            Assert.Equal(4, thresholdValues.Length);
            Assert.Equal(new double[] { 0, 5, 10, 15 }, thresholdValues);
        }

        [Fact]
        public void GetColorIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green");

            Assert.Equal(SKColors.Transparent, palette.GetColor(-99));
            Assert.Equal(SKColors.Red, palette.GetColor(0));
            Assert.Equal(SKColors.Red, palette.GetColor(1.5));
            Assert.Equal(SKColors.Orange, palette.GetColor(5));
            Assert.Equal(SKColors.Orange, palette.GetColor(8.4));
            Assert.Equal(SKColors.Yellow, palette.GetColor(12));
            Assert.Equal(SKColors.Green, palette.GetColor(15));
            Assert.Equal(SKColors.Green, palette.GetColor(99));
        }

        [Fact]
        public void GetColorForUpperThresholdValuesIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green", 1, PaletteType.UpperThresholdValues);

            Assert.Equal(SKColors.Red, palette.GetColor(-99));
            Assert.Equal(SKColors.Red, palette.GetColor(0));
            Assert.Equal(SKColors.Orange, palette.GetColor(1.5));
            Assert.Equal(SKColors.Orange, palette.GetColor(5));
            Assert.Equal(SKColors.Yellow, palette.GetColor(8.4));
            Assert.Equal(SKColors.Green, palette.GetColor(12));
            Assert.Equal(SKColors.Green, palette.GetColor(15));
            Assert.Equal(SKColors.Transparent, palette.GetColor(99));
        }

        [Fact]
        public void ToBitmapHorizontalIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green");
            var image = palette.ToBitmapHorizontal(500, 50);
            Assert.Equal(500, image.Width);
        }

        [Fact]
        public void ToBitmapHorizontalForUpperThresholdValuesIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green", 1, PaletteType.UpperThresholdValues);
            var image = palette.ToBitmapHorizontal(500, 50);
            Assert.Equal(500, image.Width);
        }

        [Fact]
        public void ToBitmapVerticalIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green");
            var image = palette.ToBitmapVertical(50, 500);
            Assert.Equal(50, image.Width);
        }

        [Fact]
        public void ToBitmapVerticalForUpperThresholdValuesIsOk()
        {
            var palette = new Palette("0:Red|5:Orange|10:Yellow|15:Green", 1, PaletteType.UpperThresholdValues);
            var image = palette.ToBitmapVertical(50, 500);
            Assert.Equal(50, image.Width);
        }
    }
}
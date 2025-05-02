namespace DHI.Services.GIS.Test
{
    using SkiaSharp;
    using Xunit;

    public class CropAndResizeTest
    {
        [Fact]
        public void CropAndResizeIsOk()
        {
            using var originalImage = SKBitmap.Decode(@"..\..\..\Data\TRMM_2000.png");
            var sourceRectangle = SKRectI.Create(500, -500, 1000, 1000);
            var destinationRectangle = SKRectI.Create(0, 0, 1000, 1000);
            using var croppedAndResizedImage = originalImage.CropAndResize(sourceRectangle, destinationRectangle);
            using var sample = SKBitmap.Decode(@"..\..\..\Data\TRMM_2000-cropped.png");

            Assert.True(croppedAndResizedImage.IsSimilar(sample));
        }
    }
}
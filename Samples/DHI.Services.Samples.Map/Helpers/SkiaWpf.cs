using SkiaSharp;
using System.IO;
using System.Windows.Media.Imaging;

namespace DHI.Services.Samples.Map.Helpers
{
    public static class SkiaWpf
    {
        public static BitmapImage ToBitmapImage(SKBitmap bmp)
        {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var bi = new BitmapImage();
            using var ms = new MemoryStream(data.ToArray());
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
    }
}

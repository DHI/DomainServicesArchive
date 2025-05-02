namespace DHI.Services.GIS.Test
{
    using System.IO;
    using System.Runtime.Versioning;
    using SkiaSharp;

    public static class Extensions
    {
        public static void ToPng(this SKBitmap bmp, string filePath)
        {
            using var memory = new MemoryStream();
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            using var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(memory);
            var bytes = memory.ToArray();
            fs.Write(bytes, 0, bytes.Length);
        }

        public static bool IsSimilar(this SKBitmap bmp, SKBitmap other)
        {
            for (var col = 0; col < bmp.Width; col++)
            {
                for (var row = 0; row < bmp.Height; row++)
                {
                    if (!bmp.GetPixel(col, row).Equals(other.GetPixel(col, row)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
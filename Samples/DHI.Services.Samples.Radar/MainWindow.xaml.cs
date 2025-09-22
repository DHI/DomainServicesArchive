namespace DHI.Services.Samples.Radar
{
    using System;
    using DHI.Services.Samples.Radar.ViewModels;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void RadarImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MainViewModel vm || !vm.IsPicking || vm.CurrentImage is not BitmapSource src)
                return;

            var img = RadarImage;
            var pos = e.GetPosition(img);

            double imgW = img.ActualWidth;
            double imgH = img.ActualHeight;
            double srcW = src.PixelWidth;
            double srcH = src.PixelHeight;

            double scale = Math.Min(imgW / srcW, imgH / srcH);
            double drawW = srcW * scale;
            double drawH = srcH * scale;
            double left = (imgW - drawW) / 2.0;
            double top = (imgH - drawH) / 2.0;

            if (pos.X < left || pos.X > left + drawW || pos.Y < top || pos.Y > top + drawH) return;

            double relX = (pos.X - left) / scale;
            double relY = (pos.Y - top) / scale;

            int col = (int)Math.Floor(relX) + 1; // 1-based indices
            int row = (int)Math.Floor(relY) + 1;

            vm.SetPickedPixel(col, row, pos.X, pos.Y, left, top, drawW, drawH);
        }
    }
}
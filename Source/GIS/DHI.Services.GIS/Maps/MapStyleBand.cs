namespace DHI.Services.GIS.Maps
{
    using SkiaSharp;

    public class MapStyleBand
    {
        public MapStyleBand()
        {
            UpperBandValue = double.NaN;
            LowerBandValue = double.NaN;
        }

        public double BandValue { get; set; }

        public double UpperBandValue { get; set; }

        public double LowerBandValue { get; set; }

        public SKColor BandColor { get; set; }

        public string BandText { get; set; }

        public bool HasLowerBand()
        {
            return !double.IsNaN(LowerBandValue);
        }

        public bool HasUpperBand()
        {
            return !double.IsNaN(UpperBandValue);
        }
    }
}
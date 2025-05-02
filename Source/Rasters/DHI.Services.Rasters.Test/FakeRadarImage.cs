namespace DHI.Services.Rasters.Test
{
    using System;
    using System.IO;
    using Radar;
    using Rasters;

    public class FakeRadarImage : BaseRadarImage
    {
        public FakeRadarImage(DateTime dateTime, string name)
            : base(dateTime, name)
        {
        }

        public override void FromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override float GetIntensity(Pixel pixel)
        {
            return 1f;
        }

        public override float GetIntensity(Pixel pixel, ConversionCoefficients coefficients)
        {
            return 1f;
        }
    }
}
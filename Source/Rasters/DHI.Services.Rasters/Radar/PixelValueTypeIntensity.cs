namespace DHI.Services.Rasters.Radar
{
    using System;

    internal class PixelValueTypeIntensity : PixelValueType
    {
        public PixelValueTypeIntensity()
            : base(1, "Intensity")
        {
        }

        public override float ToValue(byte[] valueBytes)
        {
            return (float)BitConverter.ToDouble(valueBytes, 0);
        }
    }
}

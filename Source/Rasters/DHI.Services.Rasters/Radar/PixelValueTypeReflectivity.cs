namespace DHI.Services.Rasters.Radar
{
    using System;

    internal class PixelValueTypeReflectivity : PixelValueType
    {
        public PixelValueTypeReflectivity()
            : base(0, "Reflectivity")
        {
        }

        public override float ToValue(byte[] valueBytes)
        {
            return Convert.ToSingle(valueBytes[0]);
        }
    }
}

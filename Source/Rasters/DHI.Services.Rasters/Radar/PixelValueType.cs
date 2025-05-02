namespace DHI.Services.Rasters.Radar
{
    /// <summary>
    /// Class PixelValueType.
    /// </summary>
    public abstract class PixelValueType : Enumeration
    {
        /// <summary>
        /// The reflectivity.
        /// </summary>
        public static readonly PixelValueType Reflectivity = new PixelValueTypeReflectivity();

        /// <summary>
        /// The intensity.
        /// </summary>
        public static readonly PixelValueType Intensity = new PixelValueTypeIntensity();

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelValueType" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        protected PixelValueType(int value, string displayName)
            : base(value, displayName)
        {
        }

        /// <summary>
        /// Converts a byte array to a value.
        /// </summary>
        /// <param name="valueBytes">The bytes array.</param>
        /// <returns>System.Double.</returns>
        public abstract float ToValue(byte[] valueBytes);

    }
}

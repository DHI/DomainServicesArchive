namespace DHI.Services.Rasters.Zones
{
    /// <summary>
    ///     Class PixelWeight.
    /// </summary>
    public class PixelWeight
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PixelWeight" /> class.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <param name="weight">The weight.</param>
        public PixelWeight(Pixel pixel, Weight weight)
        {
            Pixel = pixel;
            Weight = weight;
        }

        /// <summary>
        ///     Gets or sets the pixel.
        /// </summary>
        /// <value>The pixel.</value>
        public Pixel Pixel { get; }

        /// <summary>
        ///     Gets or sets the weight.
        /// </summary>
        /// <value>The weight.</value>
        public Weight Weight { get; }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is PixelWeight weight && Pixel.Equals(weight.Pixel);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Pixel.GetHashCode();
        }
    }
}
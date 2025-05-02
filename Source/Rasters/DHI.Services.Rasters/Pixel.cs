namespace DHI.Services.Rasters
{
    using System;

    /// <summary>
    ///     Class representing a pixel in raster image.
    /// </summary>
    public class Pixel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Pixel" /> class.
        /// </summary>
        /// <param name="col">The pixel column. 1-based from left in grid.</param>
        /// <param name="row">The pixel row. 1-based from top of grid.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if col or row is not a valid index.</exception>
        public Pixel(int col, int row)
        {
            if (col < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(col));
            }

            if (row < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            Col = col;
            Row = row;
        }

        /// <summary>
        ///     Gets or sets the pixel column.
        /// </summary>
        /// <value>The pixel column.</value>
        public int Col { get; }

        /// <summary>
        ///     Gets or sets the pixel row.
        /// </summary>
        /// <value>The pixel row.</value>
        public int Row { get; }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (Col == ((Pixel)obj).Col) && (Row == ((Pixel)obj).Row);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Col.GetHashCode();
            hash = (hash * 7) + Row.GetHashCode();
            return hash;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Col}; {Row}";
        }
    }
}
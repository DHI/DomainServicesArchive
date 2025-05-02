namespace DHI.Services.TimeSeries
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     2D Vector struct
    /// </summary>
    /// <typeparam name="TValue">The (numeric) type of the component values.</typeparam>
    public struct Vector<TValue> where TValue : struct
    {
        /// <summary>
        ///     Gets the X component.
        /// </summary>
        public double X { get; }

        /// <summary>
        ///     Gets the Y component.
        /// </summary>
        public double Y { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector{TValue}" /> struct.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public Vector(TValue x, TValue y)
        {
            X = Convert.ToDouble(x, CultureInfo.InvariantCulture);
            Y = Convert.ToDouble(y, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Gets the vector direction in degrees.
        /// </summary>
        public double Direction => Math.Atan2(Y, X) / (2 * Math.PI) * 360;

        /// <summary>
        ///     Gets the vector size (magnitude).
        /// </summary>
        /// <value>The size.</value>
        public double Size => Math.Sqrt(X * X + Y * Y);

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return FormattableString.Invariant($"({X:##.###}, {Y:##.###})");
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector<TValue> otherVector))
            {
                return false;
            }

            return Math.Abs(X - otherVector.X) < double.Epsilon && Math.Abs(Y - otherVector.Y) < double.Epsilon;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() * Y.GetHashCode();
        }

        /// <summary>
        ///     Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Vector<TValue> left, Vector<TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Vector<TValue> left, Vector<TValue> right)
        {
            return !(left == right);
        }
    }
}
namespace DHI.Services.TimeSeries
{
    using System;

    /// <summary>
    /// Class DataPointWFlag.
    /// </summary>
    /// <typeparam name="TValue">The type of the data point value.</typeparam>
    /// <typeparam name="TFlag">The type of the data point flag.</typeparam>
    public class DataPointWFlag<TValue, TFlag> : DataPoint<TValue>, IComparable<DataPointWFlag<TValue, TFlag>>, IEquatable<DataPointWFlag<TValue, TFlag>> where TValue : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointWFlag{TValue, TFlag}"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="flag">The flag.</param>
        public DataPointWFlag(DateTime dateTime, TValue? value, TFlag flag)
            : base(dateTime, value)
        {
            Flag = flag;
        }

        /// <summary>
        /// Gets the flag.
        /// </summary>
        /// <value>The flag.</value>
        public TFlag Flag { get; }

        /// <summary>
        ///     Compares the DateTime of this instance to the DateTime of the specified data point and indicates whether this
        ///     instance is earlier than, the same as, or later than the specified DateTime value.
        /// </summary>
        /// <param name="other">The other data point.</param>
        public int CompareTo(DataPointWFlag<TValue, TFlag> other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(DataPointWFlag<TValue, TFlag> other)
        {
            return base.Equals(other);
        }
    }

    /// <inheritdoc />
    public class DataPointWFlag<TFlag> : DataPointWFlag<double, TFlag>
    {
        public DataPointWFlag(DateTime dateTime, double? value, TFlag flag)
            : base(dateTime, value, flag)
        {
        }
    }
}
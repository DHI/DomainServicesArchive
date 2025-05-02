namespace DHI.Services.TimeSeries
{
    using System;

    /// <summary>
    ///     Class DataPoint.
    /// </summary>
    /// <typeparam name="TValue">The type of the DataPoint value.</typeparam>
    public class DataPoint<TValue> : IComparable<DataPoint<TValue>>, IEquatable<DataPoint<TValue>> where TValue : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataPoint{TValue}" /> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        public DataPoint(DateTime dateTime, TValue? value)
        {
            DateTime = dateTime;
            Value = value;
        }

        /// <summary>
        ///     Gets the date time.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        public TValue? Value { get; }

        /// <summary>
        ///     Compares the DateTime of this instance to the DateTime of the specified data point and indicates whether this
        ///     instance is earlier than, the same as, or later than the specified DateTime value.
        /// </summary>
        /// <param name="other">The other data point.</param>
        public int CompareTo(DataPoint<TValue> other)
        {
            return DateTime.CompareTo(other.DateTime);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="DataPoint{TValue}" /> is equal to this instance.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <returns><c>true</c> if the specified <see cref="DataPoint{TValue}" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(DataPoint<TValue> dataPoint)
        {
            return dataPoint != null && DateTime.Equals(dataPoint.DateTime);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as DataPoint<TValue>;
            return Equals(other);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{DateTime}:{Value}";
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implements the &lt; operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the &lt;= operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <=(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Implements the &gt; operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the &gt;= operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >=(DataPoint<TValue> left, DataPoint<TValue> right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }

    /// <inheritdoc />
    public class DataPoint : DataPoint<double>
    {
        public DataPoint(DateTime dateTime, double? value)
            : base(dateTime, value)
        {
        }
    }
}
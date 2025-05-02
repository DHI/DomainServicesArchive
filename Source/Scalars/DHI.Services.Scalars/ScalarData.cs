namespace DHI.Services.Scalars
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class ScalarData.
    /// </summary>
    /// <typeparam name="TFlag">The type of the quality flag.</typeparam>
    [Serializable]
    public class ScalarData<TFlag> : IEquatable<ScalarData<TFlag>> where TFlag : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScalarData{TFlag}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="flag">The quality flag.</param>
        public ScalarData(object value, DateTime dateTime, TFlag? flag = null)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            DateTime = dateTime;
            Flag = flag;
        }

        /// <summary>
        ///     Gets the data value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        ///     Gets the quality flag.
        /// </summary>
        public TFlag? Flag { get; }

        /// <summary>
        ///     Gets the date time of the data.
        /// </summary>
        /// <value>The date time.</value>
        public DateTime DateTime { get; }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise,
        ///     false.
        /// </returns>
        public bool Equals(ScalarData<TFlag> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Value, other.Value) && Flag.Equals(other.Flag);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Flag is null ? FormattableString.Invariant($"{DateTime}: {Value}") : FormattableString.Invariant($"{DateTime}: {Value} ({Flag})");
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals((ScalarData<TFlag>)obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ Flag.GetHashCode();
            }
        }

        public static bool operator ==(ScalarData<TFlag> left, ScalarData<TFlag> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScalarData<TFlag> left, ScalarData<TFlag> right)
        {
            return !Equals(left, right);
        }
    }

    /// <inheritdoc />
    [Serializable]
    public class ScalarData : ScalarData<int>
    {
        /// <inheritdoc />
        public ScalarData(object value, DateTime dateTime, int? flag = null)
            : base(value, dateTime, flag)
        {
        }
    }
}
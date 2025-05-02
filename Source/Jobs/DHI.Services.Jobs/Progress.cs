namespace DHI.Services.Jobs
{
    using System;

    /// <summary>
    ///     Struct representing the progress of a job
    /// </summary>
    public struct Progress : IComparable<Progress>, IEquatable<Progress>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Progress" /> struct.
        /// </summary>
        /// <param name="value">The progress value in percent.</param>
        /// <param name="message">A text message.</param>
        public Progress(int value, string message = null)
            : this()
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentException($"Illegal percent value: {value}", nameof(value));
            }

            Value = value;
            Message = message;
        }

        /// <summary>
        ///     Gets the text message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        /// <summary>
        ///     Gets the progress value.
        /// </summary>
        /// <value>The value.</value>
        public int Value { get; }

        /// <summary>
        ///     Implements the !=.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Progress p1, Progress p2)
        {
            return !(p1.Equals(p2));
        }

        /// <summary>
        ///     Implements the &lt;.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <(Progress p1, Progress p2)
        {
            return (p1.CompareTo(p2) < 0);
        }

        /// <summary>
        ///     Implements the ==.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Progress p1, Progress p2)
        {
            return (p1.Equals(p2));
        }

        /// <summary>
        ///     Implements the &gt;.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >(Progress p1, Progress p2)
        {
            return (p1.CompareTo(p2) > 0);
        }

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has the following
        ///     meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
        ///     object is equal to <paramref name="other" />. Greater than zero This object is greater than
        ///     <paramref name="other" />.
        /// </returns>
        public int CompareTo(Progress other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Progress progress && Equals(progress);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Progress other)
        {
            return Value.Equals(other.Value);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Value}% {Message}";
        }
    }
}
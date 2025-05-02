namespace DHI.Services.TimeSeries
{
    using System;

    /// <summary>
    /// Class DataPointForecasted.
    /// </summary>
    /// <typeparam name="TValue">The type of the DataPoint value.</typeparam>
    public class DataPointForecasted<TValue> : DataPoint<TValue>, IComparable<DataPointForecasted<TValue>>, IEquatable<DataPointForecasted<TValue>> where TValue : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointForecasted{TValue}"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="forecastTime">The forecast time.</param>
        public DataPointForecasted(DateTime dateTime, TValue? value, DateTime forecastTime)
            : base(dateTime, value)
        {
            TimeOfForecast = forecastTime;
        }

        /// <summary>
        /// Gets the time of forecast.
        /// </summary>
        /// <value>The time of forecast.</value>
        public DateTime TimeOfForecast { get; }

        /// <summary>
        ///     Compares the DateTime of this instance to the DateTime of the specified data point and indicates whether this
        ///     instance is earlier than, the same as, or later than the specified DateTime value.
        /// </summary>
        /// <param name="other">The other data point.</param>
        public int CompareTo(DataPointForecasted<TValue> other)
        {
            var result = DateTime.CompareTo(other.DateTime);
            if (result != 0)
            {
                return result;
            }

            return TimeOfForecast.CompareTo(other.TimeOfForecast);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as DataPointForecasted<TValue>;
            return Equals(other);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="DataPoint{TValue}" /> is equal to this instance.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(DataPointForecasted<TValue> dataPoint)
        {
            if (dataPoint == null)
            {
                return false;
            }

            return DateTime.Equals(dataPoint.DateTime) && TimeOfForecast.Equals(dataPoint.TimeOfForecast);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return DateTime.GetHashCode() ^ TimeOfForecast.GetHashCode();
        }
    }

    /// <inheritdoc />
    public class DataPointForecasted : DataPointForecasted<double>
    {
        public DataPointForecasted(DateTime dateTime, double? value, DateTime forecastTime)
            : base(dateTime, value, forecastTime)
        {
        }
    }
}
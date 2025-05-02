namespace DHI.Services.TimeSeries
{
    using System;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     TimeSeries entity class.
    /// </summary>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series data values. Must be a numeric type (int, long float, double etc.)</typeparam>
    [Serializable]
    public class TimeSeries<TId, TValue> : BaseGroupedEntity<TId> where TValue : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeries{TId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TimeSeries(TId id, string name)
            : this(id, name, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeries{TId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="group">The group.</param>
        /// <param name="data">The data.</param>
        public TimeSeries(TId id, string name, string group, ITimeSeriesData<TValue> data = null)
            : this(id, name, group, null, null, null, data)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeries{TId, TValue}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="group">The group.</param>
        /// <param name="dimension">The dimension.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="data">The data.</param>
        [JsonConstructor]
        public TimeSeries(TId id, string name, string group, string dimension, string quantity, string unit, ITimeSeriesData<TValue> data = null)
            : base(id, name, group)
        {
            Dimension = dimension;
            Quantity = quantity;
            Unit = unit;
            Data = data ?? new TimeSeriesData<TValue>();
        }

        /// <summary>
        ///     Gets the data.
        /// </summary>
        public ITimeSeriesData<TValue> Data { get; }

        /// <summary>
        ///     Gets or sets the data type.
        /// </summary>
        public TimeSeriesDataType DataType { get; set; } = TimeSeriesDataType.Instantaneous;

        /// <summary>
        ///     Gets or sets the dimension.
        /// </summary>
        public string Dimension { get; set; }

        /// <summary>
        ///     Gets or sets the quantity.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        ///     Gets or sets the unit.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance contains any data.
        /// </summary>
        /// <value><c>true</c> if this instance has values; otherwise, <c>false</c>.</value>
        public bool HasValues => Data?.Values.Count > 0;

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        public new TimeSeries<TId, TValue> Clone()
        {
            return this;
            //return (TimeSeries<TId, TValue>)base.Clone();
            //return  base.Clone<TimeSeries<TId, TValue>>();
        }

        public bool ShouldSerializeData()
        {
            return Data.Values.Any();
        }
    }

    /// <inheritdoc />
    public class TimeSeries : TimeSeries<string, double>
    {
        /// <inheritdoc />
        public TimeSeries(string id, string name)
            : base(id, name)
        {
        }

        /// <inheritdoc />
        public TimeSeries(string id, string name, string group, ITimeSeriesData<double> data = null)
            : base(id, name, group, data)
        {
        }

        /// <inheritdoc />
        public TimeSeries(string id, string name, string group, string dimension, string quantity, string unit, ITimeSeriesData<double> data = null)
            : base(id, name, group, dimension, quantity, unit, data)
        {
        }
    }
}
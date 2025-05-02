namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Vector time series data class.
    /// </summary>
    /// <typeparam name="TValue">The type of the time series data values. Must be a numeric type (int, long float, double etc.)</typeparam>
    public class VectorTimeSeriesData<TValue> : TimeSeriesData<Vector<TValue>> where TValue : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VectorTimeSeriesData{TValue}" /> class.
        /// </summary>
        /// <param name="xData">The x data.</param>
        /// <param name="yData">The y data.</param>
        public VectorTimeSeriesData(ITimeSeriesData<TValue> xData, ITimeSeriesData<TValue> yData)
        {
            if (!xData.DateTimes.SequenceEqual(yData.DateTimes))
            {
                throw new ArgumentException($"The DateTimes in {nameof(xData)} must be equal to the DateTimes in {nameof(yData)}");
            }

            for (var i = 0; i < xData.DateTimes.Count; i++)
            {
                DateTimes.Add(xData.DateTimes[i]);
                if (xData.Values[i] is null || yData.Values[i] is null)
                {
                    Values.Add(null);
                }
                else
                {
                    Values.Add(new Vector<TValue>((TValue)xData.Values[i], (TValue)yData.Values[i]));
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VectorTimeSeriesData{TValue}" /> class.
        /// </summary>
        /// <param name="xValues">The x values.</param>
        /// <param name="yValues">The y values.</param>
        public VectorTimeSeriesData(SortedSet<DataPoint<TValue>> xValues, SortedSet<DataPoint<TValue>> yValues)
            : this(xValues.ToTimeSeriesData(), yValues.ToTimeSeriesData())
        {
        }
    }

    /// <inheritdoc />
    public class VectorTimeSeriesData : VectorTimeSeriesData<double>
    {
        /// <inheritdoc />
        public VectorTimeSeriesData(ITimeSeriesData<double> xData, ITimeSeriesData<double> yData)
            : base(xData, yData)
        {
        }

        /// <inheritdoc />
        public VectorTimeSeriesData(SortedSet<DataPoint<double>> xValues, SortedSet<DataPoint<double>> yValues)
            : base(xValues, yValues)
        {
        }
    }
}
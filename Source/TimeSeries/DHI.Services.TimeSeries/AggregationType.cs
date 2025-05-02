namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Class AggregationType.
    /// </summary>
    [Serializable]
    public abstract class AggregationType : Enumeration
    {
        /// <summary>
        ///     Minimum
        /// </summary>
        public static readonly AggregationType Minimum = new MinimumAggregationType();

        /// <summary>
        ///     Maximum
        /// </summary>
        public static readonly AggregationType Maximum = new MaximumAggregationType();

        /// <summary>
        ///     Average
        /// </summary>
        public static readonly AggregationType Average = new AverageAggregationType();

        /// <summary>
        ///     Sum
        /// </summary>
        public static readonly AggregationType Sum = new SumAggregationType();

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregationType" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        protected AggregationType(int value, string displayName) : base(value, displayName)
        {
        }

        /// <summary>
        ///     Gets the aggregation value.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        public abstract TValue? GetValue<TValue>(ITimeSeriesData<TValue> timeSeriesData) where TValue : struct, IComparable<TValue>;

        /// <summary>
        ///     Gets the aggregation value.
        /// </summary>
        /// <param name="values">The values.</param>
        public abstract float? GetValue(IEnumerable<float?> values);

        /// <summary>
        ///     Gets the aggregation value.
        /// </summary>
        /// <param name="values">The values.</param>
        public abstract double? GetValue(IEnumerable<double?> values);

        [Serializable]
        private class MinimumAggregationType : AggregationType
        {
            public MinimumAggregationType()
                : base(0, "Minimum")
            {
            }

            public override TValue? GetValue<TValue>(ITimeSeriesData<TValue> timeSeriesData)
            {
                return timeSeriesData.Minimum();
            }

            public override float? GetValue(IEnumerable<float?> values)
            {
                return values.Min();
            }

            public override double? GetValue(IEnumerable<double?> values)
            {
                return values.Min();
            }
        }

        [Serializable]
        private class MaximumAggregationType : AggregationType
        {
            public MaximumAggregationType()
                : base(1, "Maximum")
            {
            }

            public override TValue? GetValue<TValue>(ITimeSeriesData<TValue> timeSeriesData)
            {
                return timeSeriesData.Maximum();
            }

            public override float? GetValue(IEnumerable<float?> values)
            {
                return values.Max();
            }

            public override double? GetValue(IEnumerable<double?> values)
            {
                return values.Max();
            }
        }

        [Serializable]
        private class AverageAggregationType : AggregationType
        {
            public AverageAggregationType()
                : base(2, "Average")
            {
            }

            public override TValue? GetValue<TValue>(ITimeSeriesData<TValue> timeSeriesData)
            {
                return timeSeriesData.Average();
            }
            public override float? GetValue(IEnumerable<float?> values)
            {
                return values.Average();
            }

            public override double? GetValue(IEnumerable<double?> values)
            {
                return values.Average();
            }
        }

        [Serializable]
        private class SumAggregationType : AggregationType
        {
            public SumAggregationType()
                : base(3, "Sum")
            {
            }

            public override TValue? GetValue<TValue>(ITimeSeriesData<TValue> timeSeriesData)
            {
                return timeSeriesData.Sum();
            }

            public override float? GetValue(IEnumerable<float?> values)
            {
                return values.Sum();
            }

            public override double? GetValue(IEnumerable<double?> values)
            {
                return values.Sum();
            }
        }
    }
}

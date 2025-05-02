namespace DHI.Services.TimeSeries
{
    using System;

    /// <summary>
    ///     Class TimeSeriesDataType.
    /// </summary>
    [Serializable]
    public abstract class TimeSeriesDataType : Enumeration
    {
        /// <summary>
        ///     Instantaneous. The value is defined at the time specified.
        /// </summary>
        public static readonly TimeSeriesDataType Instantaneous = new InstantaneousTimeSeriesDataType();

        /// <summary>
        ///     Accumulated. The value is an accumulated value from the start time of the file to the time specified.
        /// </summary>
        public static readonly TimeSeriesDataType Accumulated = new AccumulatedTimeSeriesDataType();

        /// <summary>
        ///     Step-accumulated. The value is accumulated between last time step to current time step.
        /// </summary>
        public static readonly TimeSeriesDataType StepAccumulated = new StepAccumulatedTimeSeriesDataType();

        /// <summary>
        ///     Mean-step-backward. Mean value from previous time step time to current time step time. This is also sometimes
        ///     called ‘mean-step-accumulated’.
        /// </summary>
        public static readonly TimeSeriesDataType MeanStepBackward = new MeanStepBackwardTimeSeriesDataType();

        /// <summary>
        ///     Mean-step-forward. Mean value from current time step time to next time step time. This is also sometimes called
        ///     ‘reverse-mean-step-accumulated’.
        /// </summary>
        public static readonly TimeSeriesDataType MeanStepForward = new MeanStepForwardTimeSeriesDataType();

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataType" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        protected TimeSeriesDataType(int value, string displayName)
            : base(value, displayName)
        {
        }

        /// <summary>
        ///     Interpolates a value at the given date time between the given data points.
        /// </summary>
        /// <param name="p0">The first data point.</param>
        /// <param name="p1">The second data point.</param>
        /// <param name="dateTime">The date time.</param>
        public virtual DataPoint<double> Interpolate(DataPoint<double> p0, DataPoint<double> p1, DateTime dateTime)
        {
            ValidateInterpolationArguments(p0, p1, dateTime);
            var h = (dateTime - p0.DateTime).TotalSeconds / (p1.DateTime - p0.DateTime).TotalSeconds;
            var value = p0.Value + (p1.Value - p0.Value) * h;
            return new DataPoint<double>(dateTime, value);
        }

        /// <summary>
        ///     Validates the interpolation arguments.
        /// </summary>
        /// <param name="p0">The first data point.</param>
        /// <param name="p1">The second data point.</param>
        /// <param name="dateTime">The date time.</param>
        protected static void ValidateInterpolationArguments(DataPoint<double> p0, DataPoint<double> p1, DateTime dateTime)
        {
            if (p0.Value == null || p1.Value == null)
            {
                throw new ArgumentException("Null values are not accepted in interpolation.");
            }

            if (!(p1.DateTime > p0.DateTime))
            {
                throw new ArgumentException(
                    $"End point DateTime '{p1.DateTime}' is less than start point DateTime '{p0.DateTime}'.");
            }

            if (!(dateTime >= p0.DateTime && dateTime <= p1.DateTime))
            {
                throw new ArgumentException(
                    $"'{dateTime}' is not within the time interval '{p0.DateTime}' - '{p1.DateTime}'.", nameof(dateTime));
            }
        }

        [Serializable]
        private class InstantaneousTimeSeriesDataType : TimeSeriesDataType
        {
            public InstantaneousTimeSeriesDataType()
                : base(0, "Instantaneous")
            {
            }
        }

        [Serializable]
        private class AccumulatedTimeSeriesDataType : TimeSeriesDataType
        {
            public AccumulatedTimeSeriesDataType()
                : base(1, "Accumulated")
            {
            }
        }

        [Serializable]
        private class StepAccumulatedTimeSeriesDataType : TimeSeriesDataType
        {
            public StepAccumulatedTimeSeriesDataType()
                : base(2, "Step Accumulated")
            {
            }
        }

        [Serializable]
        private class MeanStepBackwardTimeSeriesDataType : TimeSeriesDataType
        {
            public MeanStepBackwardTimeSeriesDataType()
                : base(3, "Mean Step Backward")
            {
            }

            public override DataPoint<double> Interpolate(DataPoint<double> p0, DataPoint<double> p1, DateTime dateTime)
            {
                ValidateInterpolationArguments(p0, p1, dateTime);
                return new DataPoint<double>(dateTime, p1.Value);
            }
        }

        [Serializable]
        private class MeanStepForwardTimeSeriesDataType : TimeSeriesDataType
        {
            public MeanStepForwardTimeSeriesDataType()
                : base(4, "Mean Step Forward")
            {
            }

            public override DataPoint<double> Interpolate(DataPoint<double> p0, DataPoint<double> p1, DateTime dateTime)
            {
                ValidateInterpolationArguments(p0, p1, dateTime);
                return new DataPoint<double>(dateTime, p0.Value);
            }
        }
    }
}
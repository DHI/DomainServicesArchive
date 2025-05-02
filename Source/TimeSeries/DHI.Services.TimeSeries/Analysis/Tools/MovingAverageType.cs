namespace DHI.Services.TimeSeries
{
    using System;

    /// <summary>
    ///     Enum MovingAverageType
    /// </summary>
    [Obsolete("Use MovingAggregationType instead. MovingAverageType might be removed in a future version.")]
    public enum MovingAverageType
    {
        /// <summary>
        ///     Determine if the moving average should be performed on data before the date time
        /// </summary>
        Backwards,

        /// <summary>
        ///     Determine if the moving average should be performed on data ahead the date time
        /// </summary>
        Forward,

        /// <summary>
        ///     Determine if the moving average should be performed equally around date time
        /// </summary>
        Middle
    }
}